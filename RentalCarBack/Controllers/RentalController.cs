using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RentalCarBack.Data;
using Microsoft.EntityFrameworkCore;
using RentalCarBack.Model.Result;
using RentalCarBack.Model.Request;
using RentalCarBack.Model;
using BCrypt.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json;

namespace RentalCarBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentalController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RentalController(AppDbContext context){
            _context = context;
        }

        // Get car info for the cards
        [HttpGet("available-cars")]
        public async Task<ActionResult<IEnumerable<GetCarAvailableCard>>> GetAvailableCars(DateTime dateA, DateTime dateB, int? year = null)
        {
            var availableCarsQuery = _context.MsCar
                .Where(car => !_context.TrRental
                    .Any(rental =>
                        rental.CarId == car.CarId &&
                        rental.RentalDate <= dateB &&
                        (rental.ReturnDate == null || rental.ReturnDate >= dateA)));

            if (year.HasValue){
                availableCarsQuery = availableCarsQuery.Where(car => car.Year == year.Value);
            }

            var availableCars = await availableCarsQuery                
                .Select(car => new GetCarAvailableCard
                {
                    CarId = car.CarId,
                    Name = car.Name,
                    CarImageLink = car.CarImage.ImageLink,
                    PricePerDay = car.PricePerDay
                })
                .ToListAsync();


            var response = new ApiResponse<IEnumerable<GetCarAvailableCard>>
            {
                StatusCode = StatusCodes.Status200OK,
                RequestMethod = HttpContext.Request.Method,
                Data = availableCars
            };            

            return Ok(response);
        }


        // Register customer
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateCustomerRequest request){
            try{
                if (!ModelState.IsValid){
                    return BadRequest(new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        RequestMethod = HttpContext.Request.Method,
                        Data = "Invalid input data."
                    });
                }

                var lastCustomerID = await _context.MsCustomer
                .OrderByDescending(x => x.CustomerId)
                .Select(x => x.CustomerId).FirstOrDefaultAsync();

                var currCustomerID = lastCustomerID?.Substring(3);
                var num = currCustomerID != null ? int.Parse(currCustomerID) : 0;

                var isUsernameExist = await _context.MsCustomer.AnyAsync(x => x.Name == request.Name);
                if(isUsernameExist){
                    throw new Exception("Username already exists.");
                }

                if(request.Password != request.ConfirmPassword){
                    throw new Exception("Password unmatched.");
                }

                num += 1;
                var newCustomerID = num.ToString("D3");
                var newId = $"CUS{newCustomerID}";

                var newCustomerData = new MsCustomer{
                    CustomerId = newId,
                    Email = request.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Name = request.Name,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    DriverLicenseNumber = $"DLN{newCustomerID}"
                };

                _context.MsCustomer.Add(newCustomerData);
                await _context.SaveChangesAsync();

                var response = new ApiResponse<LoginResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    RequestMethod = HttpContext.Request.Method,
                    Data = new LoginResponse
                    {
                        Message = "Register successful.",
                        UserData = new GetCustomerInformation 
                        {
                            CustomerId = newId,
                            Name = request.Name,
                            Email = request.Email,
                            PhoneNumber = request.PhoneNumber,
                            Address = request.Address
                        }
                    }
                };

                return Ok(response);

            }
            catch(Exception e){
                var response =  new ApiResponse<string>{
                    StatusCode = StatusCodes.Status400BadRequest,
                    RequestMethod = HttpContext.Request.Method,
                    Data = e.Message
                };
                return BadRequest(response);
            }
        }


        // Login customer
       [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginCustomerRequest request)
        {
            // Check if the model state is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<LoginResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    RequestMethod = HttpContext.Request.Method,
                    Data = new LoginResponse
                    {
                        Message = "Invalid login request."
                    }
                });
            }

            // Attempt to find the customer by name (or email if that’s the intended logic)
            var customer = await _context.MsCustomer
                .FirstOrDefaultAsync(c => c.Name == request.Name); // Ensure you want to use Name for login

            // Check if the customer exists
            if (customer == null)
            {
                return Unauthorized(new ApiResponse<LoginResponse>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    RequestMethod = HttpContext.Request.Method,
                    Data = new LoginResponse
                    {
                        Message = "Invalid email or password."
                    }
                });
            }

            // Validate the password
            bool isPasswordValid = false;
            if (customer.Password.StartsWith("$2a$") || customer.Password.StartsWith("$2b$") || customer.Password.StartsWith("$2y$"))
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, customer.Password);
            }
            else
            {
                isPasswordValid = request.Password == customer.Password;
            }

            // If password is invalid, return unauthorized
            if (!isPasswordValid)
            {
                return Unauthorized(new ApiResponse<LoginResponse>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    RequestMethod = HttpContext.Request.Method,
                    Data = new LoginResponse
                    {
                        Message = "Invalid email or password."
                    }
                });
            }

            // Create claims for the authenticated user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, customer.CustomerId),
                new Claim(ClaimTypes.Name, customer.Name)
            };

            // Create claims identity and principal
            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Sign in the user
            await HttpContext.SignInAsync("Cookies", claimsPrincipal);
            
            // Set session values
            HttpContext.Session.SetString("CustomerId", customer.CustomerId);
            HttpContext.Session.SetString("CustomerName", customer.Name);

            // Return success response with structured message
            return Ok(new ApiResponse<LoginResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                RequestMethod = HttpContext.Request.Method,
                Data = new LoginResponse
                {
                    Message = "Login successful.",
                    UserData = new GetCustomerInformation 
                    {
                        CustomerId = customer.CustomerId,
                        Name = customer.Name,
                        Email = customer.Email,
                        PhoneNumber = customer.PhoneNumber,
                        Address = customer.Address
                        // Add any other user-related info you want to return
                    }
                }
            });
        }



        // Get current customer 
        [HttpGet("current-customer")]
        public async Task<ActionResult<GetCustomerInformation>> GetCurrentCustomer()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User not logged in.");
            }

            try
            {
                var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userData = await _context.MsCustomer
                    .Where(c => c.CustomerId == userID)
                    .Select(c => new GetCustomerInformation
                    {
                        CustomerId = c.CustomerId,
                        Name = c.Name,
                        Email = c.Email,
                        PhoneNumber = c.PhoneNumber,
                        Address = c.Address
                    }).FirstOrDefaultAsync();
                
                if (userData == null)
                {
                    return NotFound("Customer not found.");
                }

                return Ok(new ApiResponse<GetCustomerInformation>
                {
                    StatusCode = StatusCodes.Status200OK,
                    RequestMethod = HttpContext.Request.Method,
                    Data = userData
                });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }


        // Get information tentang penyewa dan mobil saat klik sewa sekarang
        [HttpGet("info")]
        public async Task<ActionResult<ApiResponse<GetCarInformation>>> GetCarInfo(string customerName, 
            string id, DateTime dateStart, DateTime dateEnd)
        {
            // var customerName = HttpContext.Session.GetString("CustomerName");
            // var customerId = HttpContext.Session.GetString("CustomerId");

            if (string.IsNullOrEmpty(customerName))
            {
                return NotFound("Customer not found.");
            }

            var carInformation = await _context.MsCar
                .Where(car => car.CarId == id && car.CarImage != null)
                .Select(car => new GetCarInformation
                {
                    CarId = car.CarId,
                    CarImageLink = car.CarImage != null ? car.CarImage.ImageLink : "default_image_link.jpg",
                    Model = car.Model,
                    CarName = car.Name,
                    Transmission = car.Transmission,
                    NumberOfCarSeats = car.NumberOfCarSeats,
                    PricePerDay = car.PricePerDay,
                    CustomerName = customerName,
                    dateStart = dateStart,
                    dateEnd = dateEnd,
                    totalPrice = car.PricePerDay * (dateEnd - dateStart).Days
                })
                .FirstOrDefaultAsync();

            if (carInformation == null)
            {
                return NotFound("Car not found.");
            }

            return Ok(new ApiResponse<GetCarInformation>
            {
                Data = carInformation,
            });
        }


        // Insert rental/booking history
        [HttpPost("Booking")]
        public async Task<IActionResult> PostRental([FromBody] CreateRentalHistoryRequest request){
            try{
                if (!ModelState.IsValid){
                    return BadRequest(new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        RequestMethod = HttpContext.Request.Method,
                        Data = "Invalid input data."
                    });
                }

                var lastRentalID = await _context.TrRental
                .OrderByDescending(x => x.RentalId)
                .Select(x => x.RentalId).FirstOrDefaultAsync();

                var currRentalID = lastRentalID?.Substring(3);
                var num = currRentalID != null ? int.Parse(currRentalID) : 0;

                num += 1;
                var newRentalID = num.ToString("D3");

                var newRentalData = new TrRental{
                    RentalId = $"RTD{newRentalID}",
                    RentalDate = request.RentalDate,
                    ReturnDate = request.ReturnDate,
                    TotalPrice = request.TotalPrice,
                    PaymentStatus = request.PaymentStatus,
                    CustomerId = request.CustomerId,
                    CarId = request.CarId
                };

                _context.TrRental.Add(newRentalData);
                await _context.SaveChangesAsync();

                var response = new ApiResponse<string>{
                    StatusCode = StatusCodes.Status201Created,
                    RequestMethod = HttpContext.Request.Method,
                    Data = "Car rented successfully."
                };

                return Ok(response);

            }
            catch(Exception e){
                var response =  new ApiResponse<string>{
                    StatusCode = StatusCodes.Status400BadRequest,
                    RequestMethod = HttpContext.Request.Method,
                    Data = e.Message
                };
                return BadRequest(response);
            }
        }

        // Get rental history
        [HttpGet("Riwayat")]
        public async Task<ActionResult<IEnumerable<GetRentalHistory>>> GetHistory([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required.");
            }

            var rentalHistory = await _context.TrRental
                .Where(rental => rental.CustomerId == userId)
                .GroupJoin(
                    _context.TrPayment,
                    rental => rental.RentalId,
                    payment => payment.RentalId,
                    (rental, payments) => new { rental, payments }
                )
                .SelectMany(
                    x => x.payments.DefaultIfEmpty(), // This ensures a left join
                    (x, payment) => new
                    {
                        x.rental,
                        PaymentDate = payment.PaymentDate // This will be null if no payment exists
                    }
                )
                .Join(
                    _context.MsCar,
                    combined => combined.rental.CarId,
                    car => car.CarId,
                    (combined, car) => new 
                    {
                        combined.rental,
                        car,
                        combined.PaymentDate
                    }
                )
                .Select(data => new GetRentalHistory
                {
                    RentalDate = $"{data.rental.RentalDate:dd MMMM yyyy} - {data.rental.ReturnDate:dd MMMM yyyy}",
                    CarName = data.car.Name,
                    PricePerDay = data.car.PricePerDay,
                    TotalDays = (data.rental.ReturnDate - data.rental.RentalDate).Days,
                    TotalPrice = data.car.PricePerDay * (data.rental.ReturnDate - data.rental.RentalDate).Days,
                    Status = data.PaymentDate.HasValue // True if there is a payment, false otherwise
                })
                .Distinct()
                .ToListAsync();

            if (!rentalHistory.Any())
            {
                return NoContent();
            }

            var jsonResponse = JsonConvert.SerializeObject(rentalHistory);
            Console.WriteLine(jsonResponse);

            return Ok(rentalHistory);
        }


    }
}
