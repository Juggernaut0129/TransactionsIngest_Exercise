using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

class SnapshotHandler
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public SnapshotHandler(IConfiguration config, HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;
    }

    //This method calls the API for all transaction info in the last 24 hours
    public async Task<List<Transaction>> GetSnapshot()
    {
        //Call the API
        string apiUrl = _config["ApiUrl"];
        HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
        var jsonResponse = await response.Content.ReadAsStringAsync();

        //Deserialize the response into individual transactions
        var result = JsonConvert.DeserializeObject<List<Transaction>>(jsonResponse);
        
        return result ?? new List<Transaction>();
    }
}
