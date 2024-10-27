using System;
using Newtonsoft.Json;
using RentalCarFront.Models.Input;
using RentalCarFront.Models.Output;
using RentalCarFront.Service;

namespace RentalCarFront.Handler;

public class UserHandler : IUser
{
    private readonly IConfiguration _configuration;
    private readonly  string baseURL = "";
    private HttpClient httpClient = new HttpClient();

    public UserHandler(IConfiguration configuration){
        _configuration = configuration;
        baseURL = configuration["apiEndpoint"];
    }

    public async Task<ApiResponse<string>> RegisterUser(CreateUserRequest request){
        if(request == null){
            return new ApiResponse<string>{
                StatusCode = "400",
                requestMethod = "POST",
                Data = "Bad Request"
            };
        }
        string endpoint = baseURL + "/Rental";
        var response = await httpClient.PostAsJsonAsync(endpoint, request);
        var apiresponse = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();

        return apiresponse;
    }

    public async Task<ApiResponse<string>> LoginUser(LoginUserRequest request){
        if (request == null)
        {
            return new ApiResponse<string>
            {
                StatusCode = "400",
                requestMethod = "POST",
                Data = "Bad Request"
            };
        }

        string endpoint = baseURL + "/Rental/login";
        var response = await httpClient.PostAsJsonAsync(endpoint, request);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();

        return apiResponse;
    }

    public async Task<ApiResponse<IEnumerable<GetUserOutput>>> GetCurrentUser(){
        string endpoint = baseURL +"/Rental/current-customer";
        var userOutput = new ApiResponse<IEnumerable<GetUserOutput>>();
        var response = await httpClient.GetAsync(endpoint);

        string apiresponse = await response.Content.ReadAsStringAsync();
        if(!string.IsNullOrEmpty(apiresponse)){
            userOutput = JsonConvert.DeserializeObject<ApiResponse<IEnumerable<GetUserOutput>>>(apiresponse);
        }

        return userOutput;
    }
}
