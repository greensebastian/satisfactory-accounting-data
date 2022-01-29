using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SatisfactoryAccountingData.Configuration;
using System.Net;

namespace SatisfactoryAccountingData.Services
{
    public class AuthorizationService
    {
        private readonly AuthConfiguration _config;
        
        public const string ApiKeyHeaderName = "X-Key";

        public AuthorizationService(AuthConfiguration config)
        {
            _config = config;
        }

        public IActionResult Authorize(HttpRequest request)
        {
            if (!request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                return new ContentResult
                {
                    Content = "Api Key not provided",
                    StatusCode = (int)HttpStatusCode.Unauthorized
                };
            }

            var apiKey = _config.Key;

            if (!apiKey.Equals(extractedApiKey))
            {
                return new ContentResult
                {
                    Content = "Api Key not valid",
                    StatusCode = (int)HttpStatusCode.Unauthorized
                };
            }

            return null;
        }
    }
}
