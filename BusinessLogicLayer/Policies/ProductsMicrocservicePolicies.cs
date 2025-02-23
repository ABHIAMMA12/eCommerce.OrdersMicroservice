using BusinessLogicLayer.DTO;
using Polly;
using Polly.Fallback;
using Serilog;
using System.Text.Json;
using System.Text;
using Polly.Bulkhead;

namespace BusinessLogicLayer.Policies
{
    public class ProductsMicrocservicePolicies : IProductMicroservicePolicies
    {
        public IAsyncPolicy<HttpResponseMessage> GetFallBackPolicy()
        {
            AsyncFallbackPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                 .FallbackAsync(async (context) =>
                 {
                     Log.Warning("FallBack Triggered : The Request Failed Returing Dummy Data.");
                     ProductDTO product = new ProductDTO(ProductId: Guid.Empty,
                      ProductName: "Temporarily Unavailable (fallback)",
                      Category: "Temporarily Unavailable (fallback)",
                      UnitPrice: 0,
                      QuantityInStock: 0
                      );

                     var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                     {
                         Content = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json")
                     };

                     return response;
                 });
                return policy;
        }

        public IAsyncPolicy<HttpResponseMessage> GetBulkAheadIsolationPolicy()
        {
            AsyncBulkheadPolicy<HttpResponseMessage> policy = Policy.BulkheadAsync<HttpResponseMessage>(
                maxParallelization: 2,//max no of concurrent requests
                maxQueuingActions: 40,//max no of rreuqest to make in queue
                onBulkheadRejectedAsync: (context) =>
                {
                    Log.Warning("Bulk Ahead Isolation Has Occured. can't send more requests as the queue is full.");
                    throw new BulkheadRejectedException("Bulk Ahead Queue is full");
                }
               );
            return policy;
         }
    }
}
