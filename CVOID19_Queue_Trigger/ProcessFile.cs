using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Entities;
using Common.Model;
using Common.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace CVOID19_Queue_Trigger
{
    public class ProcessFile
    {
        private readonly ICOVIDService _COVIDService;

        public ProcessFile(ICOVIDService COVIDService)
        {
            _COVIDService = COVIDService;
        }

        [FunctionName("Process_File")]
        public async Task RunAsync(
            [QueueTrigger("COIVD19FilesToProcess", Connection = "AzureWebJobsStorage")]string queueItem
            , [Table("ErrorLogs", Connection = "AzureWebJobsStorage")] CloudTable ErrorLogsTable
            , [Table("Covid19", Connection = "AzureWebJobsStorage")] CloudTable Covid19Table
            , ILogger log)
        {

            await Covid19Table.CreateIfNotExistsAsync();
            await ErrorLogsTable.CreateIfNotExistsAsync();

            List<ErrorLogEntity> Errors = new List<ErrorLogEntity>();
            GitCommitFile file = JsonConvert.DeserializeObject<GitCommitFile>(queueItem);
            var covidEntities = await _COVIDService.GetFileContentAsEntityListAsync(file);


            foreach (var entity in covidEntities)
            {
                try
                {
                    var coividInserOperation = TableOperation.InsertOrMerge(entity);
                    var covidResult = await Covid19Table.ExecuteAsync(coividInserOperation);
                }
                catch (Exception ex)
                {
                    var errorMsg = $"Source:{file.raw_url} -- Error inserting into Azure Table Storage {ex.Message}, Data: {JsonConvert.SerializeObject(entity)}";
                    log.LogError(errorMsg);
                    Errors.Add(new ErrorLogEntity(file.raw_url, errorMsg));
                }
            }


            if (Errors.Any())
            {
                TableBatchOperation batchOperation = new TableBatchOperation();
                Errors.ForEach(batchOperation.Insert);
                await ErrorLogsTable.ExecuteBatchAsync(batchOperation);
            }

        }
    }
}
