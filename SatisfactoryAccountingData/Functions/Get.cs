using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using SatisfactoryAccountingData.Services;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SatisfactoryAccountingData.Shared.Model;

namespace SatisfactoryAccountingData.Functions
{
    public class Get : FunctionBase<Get>
    {
        private readonly CosmosSatisfactoryClient _cosmosClient;

        public Get(ILogger<Get> log, CosmosSatisfactoryClient cosmosClient) : base(log)
        {
            _cosmosClient = cosmosClient;
        }

        [FunctionName("Get")]
        [OpenApiOperation("RunGet")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(SatisfactoryModel))]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent)]
        public async Task<IActionResult> RunGet(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, CancellationToken hostCancellationToken)
        {
            var cancellationToken = CombineTokens(req, hostCancellationToken);

            var items = _cosmosClient.GetModelsAsync(cancellationToken);

            // There should only be one item
            await foreach (var item in items.WithCancellation(cancellationToken))
            {
                return new JsonResult(item);
            }

            return new NoContentResult();
        }
    }
}

