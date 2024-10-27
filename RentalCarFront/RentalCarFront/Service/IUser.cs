using System;
using RentalCarFront.Models.Input;
using RentalCarFront.Models.Output;

namespace RentalCarFront.Service;

public interface IUser
{
    Task<ApiResponse<string>> RegisterUser(CreateUserRequest request);
    Task<ApiResponse<string>> LoginUser(LoginUserRequest request);
    Task<ApiResponse<IEnumerable<GetUserOutput>>> GetCurrentUser();

}
