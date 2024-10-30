using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace Mango.Services.ShoppingCartAPI.Utility
{
    // delegating handler is similar to .net cor middleware but delegating handler are on client side
    // if we making an http request to http client we can  levrage delegating handler to pass our bearer token to other request
    public class BackendApiAuthenticationHttpClientHandler : DelegatingHandler
    {
        // brearer token we can retrive from context accessor

        private readonly IHttpContextAccessor _contextAccessor;

        public BackendApiAuthenticationHttpClientHandler(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // we need to get the access token and add that to authorizaton headerx`
            // access_token is where token is stored
            var token = await _contextAccessor.HttpContext.GetTokenAsync("access_token");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
