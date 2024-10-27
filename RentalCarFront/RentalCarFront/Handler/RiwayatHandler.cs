using System;
using Newtonsoft.Json;
using RentalCarFront.Models.Output;
using RentalCarFront.Service;

namespace RentalCarFront.Handler;

public class RiwayatHandler : IRiwayat
{
    private readonly IConfiguration _configuration;
    private readonly  string baseURL = "";
    private HttpClient httpClient = new HttpClient();

    public RiwayatHandler(IConfiguration configuration){
        _configuration = configuration;
        baseURL = configuration["apiEndpoint"];
    }

    public async Task<ApiResponse<IEnumerable<GetRiwayatUser>>> GetHistoryUser(string id){
        string endpoint = $"{baseURL}/Rental/Riwayat?userId={id}";
        var historyOutput = new ApiResponse<IEnumerable<GetRiwayatUser>>();
        var response = await httpClient.GetAsync(endpoint);

        string apiresponse = await response.Content.ReadAsStringAsync();
        if(!string.IsNullOrEmpty(apiresponse)){
            historyOutput = JsonConvert.DeserializeObject<ApiResponse<IEnumerable<GetRiwayatUser>>>(apiresponse);
        }
        return historyOutput;
    }
}
