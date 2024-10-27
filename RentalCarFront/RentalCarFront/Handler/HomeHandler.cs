using System;
using Newtonsoft.Json;
using RentalCarFront.Models.Output;
using RentalCarFront.Service;

namespace RentalCarFront.Handler;

public class HomeHandler : ICar
{
    private readonly IConfiguration _configuration;
    private readonly  string baseURL = "";
    private HttpClient httpClient = new HttpClient();

    public HomeHandler(IConfiguration configuration){
        _configuration = configuration;
        baseURL = configuration["apiEndpoint"];
    }
    
    public async Task<ApiResponse<IEnumerable<GetCarAvailableCard>>> GetAvailableCars(DateTime dateA, DateTime dateB, int? year = null){
        string endpoint = $"{baseURL}/Rental/available-cars";
        var cardOutput = new ApiResponse<IEnumerable<GetCarAvailableCard>>();
        var response = await httpClient.GetAsync(endpoint);

        string apiresponse = await response.Content.ReadAsStringAsync();
        if(!string.IsNullOrEmpty(apiresponse)){
            cardOutput = JsonConvert.DeserializeObject<ApiResponse<IEnumerable<GetCarAvailableCard>>>(apiresponse);
        }
        return cardOutput;
    }

    public async Task<ApiResponse<ApiResponse<IEnumerable<GetCarDesc>>>> GetCarInformation(DateTime dateStart, DateTime dateEnd){
        string endpoint = $"{baseURL}/Rental/info";
        var descOutput = new ApiResponse<ApiResponse<IEnumerable<GetCarDesc>>>();
        var response = await httpClient.GetAsync(endpoint);

        string apiresponse = await response.Content.ReadAsStringAsync();
        if(!string.IsNullOrEmpty(apiresponse)){
            descOutput = JsonConvert.DeserializeObject<ApiResponse<ApiResponse<IEnumerable<GetCarDesc>>>>(apiresponse);
        }
        return descOutput;
    }
}
