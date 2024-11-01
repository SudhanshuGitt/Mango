namespace Mango.Web.Utility
{
    public class SD
    {
        public static string CouponAPIBase { get; set; } = string.Empty;
        public static string AuthAPIBase { get; set; } = string.Empty;
        public static string ProductAPIBase { get; set; } = string.Empty;
        public static string ShoppingCartAPIBase { get; set; } = string.Empty;
        public static string OrderAPIBase { get; set; } = string.Empty;


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
        public enum OrderStatus
        {
            PENDING = 1,
            APPROVED = 2,
            READYFORPICUP = 3,
            COMPLETED = 4,
            REFUNDED = 5,
            CANCELLED = 6,
        }
    }
}
