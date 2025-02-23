
using BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using System.Net.Http.Json;
using Serilog;
using Polly.Timeout;
namespace BusinessLogicLayer.HttpClients
{
    public class UsersMicroserviceClient
    {
        private readonly HttpClient _httpClient;

        public UsersMicroserviceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<UserDTO?> GetUserByUserId(Guid userId)
        {
            try
            {
                HttpResponseMessage responseMessage = await _httpClient.GetAsync($"/api/User/{userId}");
                //so after line 17 this policy hander executes will do the checks.
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
                        //returning faulty daTA  instead of throwing Exception
                        return new UserDTO(PersonName: "Temporary UnAvailable", Email: "Temporary Unavailable"
                            , Gender: "Temporary Unavailable", UserId: Guid.Empty);
                    }
                }
                UserDTO? user = await responseMessage.Content.ReadFromJsonAsync<UserDTO?>();
                if (user == null)
                {
                    throw new ArgumentException("Invalid User Exception");
                }
                return user;
            }
            catch (BrokenCircuitException ex)
            {
                Log.Information(ex, "Request Failed Due to Circuit Breaker is in Open State, Return Dummy Data.");
                return new UserDTO(PersonName: " Broken Circuit Temporary UnAvailable", Email: "Brolken Circuit Temporary Unavailable"
                            , Gender: "Broken Circuit Temporary Unavailable", UserId: Guid.Empty);
            }
            catch(TimeoutRejectedException ex)
            {
                Log.Information(ex, " TImout Rejected Exception Occured and returning Dummy Data");
                return new UserDTO(PersonName: " Timeout Exception Temporary UnAvailable", Email: "Timeout Temporary Unavailable"
                            , Gender: "Timeout .Temporary Unavailable", UserId: Guid.Empty);
            }
            
        }

    }
}
