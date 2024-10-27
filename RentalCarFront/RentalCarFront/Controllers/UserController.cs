using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using RentalCarFront.Models.Input;
using RentalCarFront.Service;

namespace RentalCarFront.Controllers
{
    public class UserController : Controller
    {
        private readonly IUser _userApi;

        public UserController(IUser userApi)
        {
            _userApi = userApi;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public async Task<IActionResult> RegisterUser(CreateUserRequest request)
        {
            var result = await _userApi.RegisterUser(request);
            return Json(result);
        }

        public async Task<IActionResult> LoginUser(LoginUserRequest request)
        {
            var result = await _userApi.LoginUser(request);
            return Json(result);
        }

        public async Task<IActionResult> GetCurrentUser()
        {
            var result = await _userApi.GetCurrentUser();
            return Json(result);
        }
    }
}
