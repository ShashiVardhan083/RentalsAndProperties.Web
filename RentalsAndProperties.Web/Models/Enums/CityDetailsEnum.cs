namespace RentalsAndProperties.Web.Models.Enums
{
    public static class CityDetailsEnum
    {
        public static readonly Dictionary<CityEnum, string> Data = new()
        {
            { CityEnum.Karimnagar, "Karimnagar" },
            { CityEnum.Hyderabad, "Hyderabad" },
            { CityEnum.Jagitial, "Jagitial" },
            { CityEnum.Siddipet, "Siddipet" },
            { CityEnum.Bangalore, "Bangalore" }
        };

        public static string GetName(CityEnum city) => Data[city];
    }
}