namespace Mango.Web.Utility
{
    public class SD
    {
        public static string CouponAPIBase { get; set; } = string.Empty;
        public static string AuthAPIBase { get; set; } = string.Empty;
        public static string ProductAPIBase { get; set; } = string.Empty;


        public const string TokenCookie = "JWTToken";

        public enum Role
        {
            ADMIN = 1,
            CUSTOMER = 2
        }

        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        }
    }
}
