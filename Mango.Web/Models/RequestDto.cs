using static Mango.Web.Utility.SD;

namespace Mango.Web.Models
{
    // it will have erverything when we send an request
    public class RequestDto
    {
        public ApiType ApiType { get; set; } = ApiType.GET;
        public string Url { get; set; }
        public object Data { get; set; }
        public string AccessToken { get; set; }
        public ContentType ContentType { get; set; } = ContentType.Json;
    }
}
