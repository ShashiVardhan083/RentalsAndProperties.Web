namespace RentalsAndProperties.Web.Models.Enums
{
    public static class CityDetails
    {
        public static readonly Dictionary<CityWeb, string> Data = new()
        {
            { CityWeb.Karimnagar, "Karimnagar" },
            { CityWeb.Hyderabad, "Hyderabad" },
            { CityWeb.Jagitial, "Jagitial" },
            { CityWeb.Siddipet, "Siddipet" },
            { CityWeb.Bangalore, "Bangalore" }
        };

        public static string GetName(CityWeb city) => Data[city];
    }
}