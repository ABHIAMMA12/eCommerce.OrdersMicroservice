using Polly;
namespace BusinessLogicLayer.Policies
{
    public interface IProductMicroservicePolicies
    {
        IAsyncPolicy<HttpResponseMessage> GetFallBackPolicy();
        IAsyncPolicy<HttpResponseMessage> GetBulkAheadIsolationPolicy();
    }
}
