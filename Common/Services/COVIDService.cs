using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common.Entities;
using Common.Model;
using Newtonsoft.Json;

namespace Common.Services
{
    public interface ICOVIDService
    {
        Task<IEnumerable<GithubRepoCommit>> GetCommitsAsync();
        Task<IEnumerable<GitCommitFile>> GetCommitFileListAsync(GithubRepoCommit commit);
        Task<IEnumerable<COVID19Entity>> GetFileContentAsEntityListAsync(GitCommitFile item);
        //Task<string> GetFileContentAsStringAsync(COVID19File item);
    }

    public class COVIDService : ICOVIDService
    {
        private static readonly string gitContentsUrl = "https://api.github.com/repos/CSSEGISandData/COVID-19/contents/csse_covid_19_data/csse_covid_19_daily_reports";
        private static readonly string gitCommitsUrl = "https://api.github.com/repos/CSSEGISandData/COVID-19/commits";
        private readonly HttpClient _client;

        public COVIDService(HttpClient httpClient)
        {
            _client = httpClient;
            _client.DefaultRequestHeaders.Add("User-Agent", "Awesome-Octocat-App");
        }

        public async Task<IEnumerable<GithubRepoCommit>> GetCommitsAsync()
        {

            var gitContentRes = await _client.GetAsync(gitCommitsUrl);
            var githubContent = await gitContentRes.Content.ReadAsAsync<IEnumerable<GithubRepoCommit>>();
            return githubContent;

        }

        public async Task<IEnumerable<GitCommitFile>> GetCommitFileListAsync(GithubRepoCommit commit)
        {
            var gitContentRes = await _client.GetAsync(commit.url);
            var githubContent = await gitContentRes.Content.ReadAsAsync<GitCommitDetails>();
            return githubContent.files.Where(x => x.filename.Contains("csse_covid_19_daily_reports"));
        }

        public async Task<IEnumerable<COVID19File>> GetListOfAllFilesAsync()
        {

            var gitContentRes = await _client.GetAsync(gitContentsUrl);
            var githubContent = await gitContentRes.Content.ReadAsAsync<IEnumerable<COVID19File>>();
            return githubContent;
        }

        public async Task<IEnumerable<COVID19Entity>> GetFileContentAsEntityListAsync(GitCommitFile item)
        {
            var fileDataRes = await _client.GetAsync(item.raw_url);
            var fileData = await fileDataRes.Content.ReadAsStringAsync();
            var standardiseData = StandardiseFile(fileData);
            return ConvertDT2COVID19Entities(item.filename, standardiseData);
        }

        public async Task<string> GetFileContentAsStringAsync(COVID19File item)
        {
            var fileDataRes = await _client.GetAsync(item.download_url);
            var fileData = await fileDataRes.Content.ReadAsStringAsync();
            var standardiseData = StandardiseFile(fileData);
            return ConvertTableTOCSVString(standardiseData);
        }

        public IEnumerable<COVID19Entity> ConvertDT2COVID19Entities(string filename, DataTable dt)
        {
            filename = filename.Substring(filename.LastIndexOf("/") + 1).Replace(".csv", "");

            return (from DataRow dr in dt.Rows

                    select new COVID19Entity(
                        PartitionKey: filename,
                        Province_State: dr[FileColumns.Province_State].ToString(),
                        Country_Region: dr[FileColumns.Country_Region].ToString(),
                        Last_Update: DateTime.Parse(dr[FileColumns.Last_Update].ToString()),
                        Confirmed: dr[FileColumns.Confirmed].ToString().ToNullableInt(),
                        Deaths: dr[FileColumns.Deaths].ToString().ToNullableInt(),
                        Recovered: dr[FileColumns.Recovered].ToString().ToNullableInt(),
                        Latitude: dr[FileColumns.Latitude].ToString().TrimAndNullIfEmpty(),
                        Longitude: dr[FileColumns.Longitude].ToString().TrimAndNullIfEmpty()
                    )
                ).ToList();
        }

        public DataTable StandardiseFile(string csvStringData)
        {

            DataTable dtCsv = new DataTable();
            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");


            //split full file text into rows  
            string[] rows = csvStringData.Split('\n');
            for (int i = 0; i < rows.Count() - 1; i++)
            {
                //split each row with comma to get individual values  
                string[] rowValues = CSVParser.Split(rows[i].Trim());
                {
                    if (i == 0) // Header
                    {
                        for (int j = 0; j < rowValues.Count(); j++)
                        {
                            string columnName = rowValues[j].Trim();
                            switch (columnName)
                            {
                                case "Province/State":
                                    {
                                        columnName = FileColumns.Province_State;
                                        break;
                                    }
                                case "Country/Region":
                                    {
                                        columnName = FileColumns.Country_Region;
                                        break;
                                    }
                                case "Last Update":
                                    {
                                        columnName = FileColumns.Last_Update;
                                        break;
                                    }
                                case "Confirmed":
                                    {
                                        columnName = FileColumns.Confirmed;
                                        break;
                                    }
                                case "Deaths":
                                    {
                                        columnName = FileColumns.Deaths;
                                        break;
                                    }
                                case "Recovered":
                                    {
                                        columnName = FileColumns.Recovered;
                                        break;
                                    }
                                case "Lat":
                                    {
                                        columnName = FileColumns.Latitude;
                                        break;
                                    }
                                case "Long_":
                                    {
                                        columnName = FileColumns.Longitude;
                                        break;
                                    }

                            }

                            dtCsv.Columns.Add(columnName); //add headers
                        }
                    }
                    else
                    {
                        DataRow dr = dtCsv.NewRow();
                        for (int k = 0; k < rowValues.Count(); k++)
                        {
                            var value = rowValues[k].ToString()
                                .TrimAndNullIfEmpty();
                            dr[k] = value;
                        }
                        dtCsv.Rows.Add(dr); //add other rows  
                    }
                }
            }


            // column adjust
            if (dtCsv.Columns.Contains("Latitude") == false)
            {
                dtCsv.Columns.Add("Latitude");
            }
            if (dtCsv.Columns.Contains("Longitude") == false)
            {
                dtCsv.Columns.Add("Longitude");
            }
            var finalColumnList = FileColumns.GetColumnNamesAsArray();
            var extraColumns = dtCsv.Columns.Cast<DataColumn>()
                .Where(x => finalColumnList.Contains(x.ColumnName) == false)
                .Select(column => column.ColumnName)
                .ToList();

            // remove un-needed columns
            extraColumns.ForEach(dtCsv.Columns.Remove);

            return dtCsv;
        }

        public string ConvertTableTOCSVString(DataTable dtCsv)
        {

            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = dtCsv.Columns.Cast<DataColumn>().
                        Select(column => column.ColumnName);

            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dtCsv.Rows)
            {

                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                var value = string.Join(",", fields);
                sb.AppendLine(value);
            }


            return sb.ToString();
        }

    }
}
