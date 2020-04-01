using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Common.Entities
{
    public class COVID19Entity : TableEntity
    {

        public string Province_State { get; set; }
        public string Country_Region { get; set; }
        public DateTime Last_Update { get; set; }
        public int? Confirmed { get; set; }
        public int? Deaths { get; set; }
        public int? Recovered { get; set; }
        // Azure table storage doenst support decimals
        public string Latitude { get; set; }
        // Azure table storage doenst support decimals
        public string Longitude { get; set; }
        public string Point { get; set; }

        public COVID19Entity(string PartitionKey,
            string Province_State, string Country_Region, DateTime Last_Update,
            int? Confirmed, int? Deaths, int? Recovered, string Latitude, string Longitude)
        {

            this.PartitionKey = PartitionKey;
            this.Province_State = Province_State.Replace("\"", "");
            this.Country_Region = Country_Region.Replace("\"", "");
            this.Last_Update = Last_Update;
            this.Confirmed = Confirmed;
            this.Deaths = Deaths;
            this.Recovered = Recovered;
            this.Latitude = Latitude;
            this.Longitude = Longitude;
            this.Point = Latitude != null && Longitude != null ? $@"Point({Longitude}, {Latitude})" : null;

            RowKey = $"{PartitionKey}-{ this.Province_State.Replace(" ", "-").Replace(",", "-")}-{this.Country_Region.Replace(",", "-").Replace(" ", "-")}";
        }
    }
}