namespace Common.Model
{
    public static class FileColumns
    {
        public static string Latitude = "Latitude";
        public static string Longitude = "Longitude";
        public static string Province_State = "Province_State";
        public static string Country_Region = "Country_Region";
        public static string Last_Update = "Last_Update";
        public static string Confirmed = "Confirmed";
        public static string Deaths = "Deaths";
        public static string Recovered = "Recovered";


        public static string[] GetColumnNamesAsArray()
        {
            return new string[] {

                Province_State ,
                Country_Region ,
                Last_Update,
                Confirmed,
                Deaths,
                Recovered,
                Latitude ,
                Longitude
            };
        }
    }
}