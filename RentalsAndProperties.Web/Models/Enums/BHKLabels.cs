namespace RentalsAndProperties.Web.Models.Enums
{
    public static class BHKLabels
    {
        public static string Get(BHKTypeWeb bhk) => bhk switch
        {
            BHKTypeWeb.OneRK => "1 RK",
            BHKTypeWeb.OneBHK => "1 BHK",
            BHKTypeWeb.TwoBHK => "2 BHK",
            BHKTypeWeb.ThreeBHK => "3 BHK",
            BHKTypeWeb.FourBHK => "4 BHK",
            BHKTypeWeb.Penthouse => "Penthouse",
            _ => bhk.ToString()
        };
    }
}
