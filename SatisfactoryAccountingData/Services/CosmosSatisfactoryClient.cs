using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using SatisfactoryAccountingData.Domain;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SatisfactoryAccountingData.Services
{
    public class CosmosSatisfactoryClient
    {
        private readonly CosmosClient _cosmosClient;

        public CosmosSatisfactoryClient(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
        }

        public Container Container => _cosmosClient.GetContainer("Satisfactory", "Model");

        public async IAsyncEnumerable<SatisfactoryModel> GetModelsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var modelsQueryable = Container.GetItemLinqQueryable<SatisfactoryModel>();
            using var modelsIterator = modelsQueryable.ToFeedIterator();
            while (modelsIterator.HasMoreResults)
            {
                var models = await modelsIterator.ReadNextAsync(cancellationToken);
                foreach (var satisfactoryModel in models)
                {
                    yield return satisfactoryModel;
                }
            }
        }

        public async Task<IList<SatisfactoryModel>> GetAllModelsAsync(CancellationToken cancellationToken)
        {
            var output = new List<SatisfactoryModel>();
            var items = GetModelsAsync(cancellationToken);
            await foreach (var model in items.WithCancellation(cancellationToken))
            {
                output.Add(model);
            }

            return output;
        }

        public async Task DeleteAllModelsAsync(CancellationToken cancellationToken)
        {
            await foreach (var model in GetModelsAsync(cancellationToken))
            {
                await Container.DeleteItemAsync<SatisfactoryModel>(model.Id.ToString(), PartitionKey.None, cancellationToken: cancellationToken);
            }
        }

    }
}
