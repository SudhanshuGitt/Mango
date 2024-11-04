using Mango.Web.Models;
using Mango.Web.Service.IService;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using static Mango.Web.Utility.SD;

namespace Mango.Web.Service
{
    /// <summary>
    /// Dynamic servie to call all the API
    /// </summary>
    public class BaseService : IBaseService
    {
        /// <summary>
        /// problem with the http client is there is overhead of instantiation as new http clent object is created
        /// for every request
        /// and HttpClient will hold open the socket that it used for some time after the request is completed
        /// and this can lead to a socket exhaustion problem when your traffic increases.
        /// </summary>

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenProvider _tokenProvider;
        public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider)
        {
            _httpClientFactory = httpClientFactory;
            _tokenProvider = tokenProvider;
        }

        public async Task<ResponseDto>? SendAsync(RequestDto requestDto, bool withBearer = true)
        {
            try
            {
                HttpClient httpClient = _httpClientFactory.CreateClient("MangoAPI");
                HttpRequestMessage message = new();

                if (requestDto.ContentType == ContentType.MutipartFormData)
                {
                    // means any media type 
                    message.Headers.Add("Accept", "*/*");
                }
                else
                {
                    message.Headers.Add("Accept", "application/json");
                }


                //token
                if (withBearer)
                {
                    var token = _tokenProvider.GetToken();
                    message.Headers.Add("Authorization", $"Bearer {token}");
                }

                message.RequestUri = new Uri(requestDto.Url);


                if (requestDto.ContentType == ContentType.MutipartFormData)
                {
                    // append the content
                    var content = new MultipartFormDataContent();

                    foreach (var prop in requestDto.Data.GetType().GetProperties())
                    {
                        // we need to read the file and add that as a new stream content

                        var value = prop.GetValue(requestDto.Data);

                        if (value is FormFile)
                        {
                            var file = (FormFile)value;

                            if (file != null)
                            {
                                content.Add(new StreamContent(file.OpenReadStream()), prop.Name, file.FileName);
                            }

                        }
                        else
                        {
                            content.Add(new StringContent(value == null ? string.Empty : value.ToString()), prop.Name);
                        }
                    }

                    message.Content = content;
                }
                else
                {
                    if (requestDto.Data != null)
                    {
                        message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8, "application/json");
                    }

                }

                HttpResponseMessage? apiResponse = null;

                switch (requestDto.ApiType)
                {
                    case ApiType.POST:
                        message.Method = HttpMethod.Post;
                        break;
                    case ApiType.PUT:
                        message.Method = HttpMethod.Put;
                        break;
                    case ApiType.DELETE:
                        message.Method = HttpMethod.Delete;
                        break;
                    default:
                        message.Method = HttpMethod.Get;
                        break;
                }

                apiResponse = await httpClient.SendAsync(message);

                switch (apiResponse.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return new ResponseDto { IsSuccess = false, Message = "Not Found" };
                    case HttpStatusCode.Forbidden:
                        return new ResponseDto { IsSuccess = false, Message = "Access Denied" };
                    case HttpStatusCode.Unauthorized:
                        return new ResponseDto { IsSuccess = false, Message = "Unauthorized" };
                    case HttpStatusCode.InternalServerError:
                        return new ResponseDto { IsSuccess = false, Message = "Internal Server Error" };
                    default:
                        var apiContent = await apiResponse.Content.ReadAsStringAsync();
                        var apiResponseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
                        return apiResponseDto;
                }
            }
            catch (Exception ex)
            {
                var dto = new ResponseDto
                {
                    IsSuccess = false,
                    Message = ex.Message.ToString()
                };
                return dto;
            }

        }
    }
}
