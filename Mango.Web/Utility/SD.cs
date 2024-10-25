namespace Mango.Web.Utility
{
    public class SD
    {
        public static string CouponAPIBase { get; set; } = String.Empty;
        public enum ApiType
        {
            GET,
            POST, 
            PUT,
            DELETE 
        }
    }
}
