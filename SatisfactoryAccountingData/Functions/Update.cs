using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SatisfactoryAccountingData.Services;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SatisfactoryAccountingData.Shared.Model;

namespace SatisfactoryAccountingData.Functions
{
    public class Update : FunctionBase<Update>
    {
        private readonly CosmosSatisfactoryClient _cosmosClient;
        private readonly AuthorizationService _authService;

        public Update(ILogger<Update> log, CosmosSatisfactoryClient cosmosClient, AuthorizationService authService) : base(log)
        {
            _cosmosClient = cosmosClient;
            _authService = authService;
        }

        [FunctionName("Update")]
        [OpenApiOperation("RunUpdate")]
        [OpenApiRequestBody("application/json", typeof(object), Description = "Documentation file from installed instance of the game\n\nSatisfactoryEarlyAccess\\CommunityResources\\Docs\\Docs.json")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(SatisfactoryModel))]
        [OpenApiSecurity("ApiKey", SecuritySchemeType.ApiKey, In = OpenApiSecurityLocationType.Header, Name = AuthorizationService.ApiKeyHeaderName)]
        public async Task<IActionResult> RunUpdate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, CancellationToken hostCancellationToken)
        {
            var authorizeResult = _authService.Authorize(req);
            if (authorizeResult != null) return authorizeResult;

            var cancellationToken = CombineTokens(req, hostCancellationToken);

            var modelFactory = new SatisfactoryModelFactory();

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            var model = modelFactory.FromDocsJson(requestBody);

            await _cosmosClient.DeleteAllModelsAsync(cancellationToken);
            await _cosmosClient.Container.CreateItemAsync(model, cancellationToken: cancellationToken);

            return new JsonResult(model);
        }
    }
}

