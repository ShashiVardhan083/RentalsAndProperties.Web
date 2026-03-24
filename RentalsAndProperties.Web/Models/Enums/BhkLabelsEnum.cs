namespace RentalsAndProperties.Web.Models.Enums
{
    public static class BhkLabelsEnum
    {
        public static string Get(BhkTypeEnum bhk) => bhk switch
        {
            BhkTypeEnum.OneRK => "1 RK",
            BhkTypeEnum.OneBHK => "1 BHK",
            BhkTypeEnum.TwoBHK => "2 BHK",
            BhkTypeEnum.ThreeBHK => "3 BHK",
            BhkTypeEnum.FourBHK => "4 BHK",
            BhkTypeEnum.Penthouse => "Penthouse",
            _ => bhk.ToString()
        };
    }
}
