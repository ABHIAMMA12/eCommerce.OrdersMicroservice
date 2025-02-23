using BusinessLogicLayer.DTO;
using Polly.Bulkhead;
using System.Net.Http.Json;
using Serilog;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BusinessLogicLayer.HttpClients
{
    public class ProdcutsMicroserviceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IDistributedCache _distributedCache;
        public ProdcutsMicroserviceClient(HttpClient httpClient, IDistributedCache distributedCache)
        {
            _httpClient = httpClient;
            _distributedCache = distributedCache;
        }
        public async Task<ProductDTO?> GetProductByProductId(Guid productId)
        {
            try
            {
                //string cachekey = $"product:{productId}";
                //string? cachedProduct = await _distributedCache.GetStringAsync(cachekey);
                //if (cachedProduct != null)
                //{
                //   ProductDTO? productFromCache =  JsonSerializer.Deserialize<ProductDTO>(cachedProduct);
                //   return productFromCache;
                //}
                HttpResponseMessage responseMessage = await _httpClient.GetAsync($"/api/products/search/productId/{productId}");
                if (!responseMessage.IsSuccessStatusCode)
                {
                    if (responseMessage.StatusCode ==
                        System.Net.HttpStatusCode.NotFound)
                    {
                        return null;
                    }
                    else if (responseMessage.StatusCode ==
                        System.Net.HttpStatusCode.BadRequest)
                    {
                        throw new HttpRequestException("Bad Request", null, System.Net.HttpStatusCode.BadRequest);
                    }
                    else
                    {
                        throw new HttpRequestException($"Http Request Failed with status code :{responseMessage.StatusCode}");
                    }
                }
                ProductDTO? product = await responseMessage.Content.ReadFromJsonAsync<ProductDTO?>();
                if (product == null)
                {
                    throw new ArgumentException("Invalid Product Exceptionn");
                }
                return product;
            }
            catch (BulkheadRejectedException ex)
            {
                Log.Warning(ex, "Bulk Ahead Isolation blocks the request as the request queue is full");
                return new ProductDTO(ProductId: Guid.Empty, ProductName: "BulkAhead Temporary Unavailable",
                            Category:"Temporary Unavailable",UnitPrice:0,QuantityInStock:0
                    );
            }
           
        }
    }
}
