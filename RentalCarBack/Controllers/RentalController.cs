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

            var customer = await _context.MsCustomer
                .FirstOrDefaultAsync(c => c.Email == request.Email); 

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

            bool isPasswordValid = false;
            if (customer.Password.StartsWith("$2a$") || customer.Password.StartsWith("$2b$") || customer.Password.StartsWith("$2y$"))
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, customer.Password);
            }
            else
            {
                isPasswordValid = request.Password == customer.Password;
            }

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

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, customer.CustomerId),
                new Claim(ClaimTypes.Name, customer.Name)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("Cookies", claimsPrincipal);
            
            HttpContext.Session.SetString("CustomerId", customer.CustomerId);
            HttpContext.Session.SetString("CustomerEmail", customer.Email);

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
                    x => x.payments.DefaultIfEmpty(), 
                    (x, payment) => new
                    {
                        x.rental,
                        PaymentDate = payment.PaymentDate 
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
                    RentalId = data.rental.RentalId,
                    RentalDate = $"{data.rental.RentalDate:dd MMMM yyyy} - {data.rental.ReturnDate:dd MMMM yyyy}",
                    CarName = data.car.Name,
                    PricePerDay = data.car.PricePerDay,
                    TotalDays = (data.rental.ReturnDate - data.rental.RentalDate).Days,
                    TotalPrice = data.car.PricePerDay * (data.rental.ReturnDate - data.rental.RentalDate).Days,
                    Status = data.PaymentDate.HasValue 
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


        // Insert payment history
        [HttpPost("Payment")]
        public async Task<IActionResult> PostPayment([FromBody] CreatePaymentRequest request) {
            try {
                if (!ModelState.IsValid) {
                    return BadRequest(new ApiResponse<string> {
                        StatusCode = StatusCodes.Status400BadRequest,
                        RequestMethod = HttpContext.Request.Method,
                        Data = "Invalid input data."
                    });
                }

                var lastPaymentID = await _context.TrPayment
                    .OrderByDescending(x => x.PaymentId)
                    .Select(x => x.PaymentId)
                    .FirstOrDefaultAsync();

                int num = 0;

                if (!string.IsNullOrEmpty(lastPaymentID) && lastPaymentID.StartsWith("PY")) {
                    var currPaymentID = lastPaymentID.Substring(2); 
                    if (int.TryParse(currPaymentID, out num)) {
                        num += 1;
                    } else {
                        return BadRequest(new ApiResponse<string> {
                            StatusCode = StatusCodes.Status400BadRequest,
                            RequestMethod = HttpContext.Request.Method,
                            Data = "Invalid last payment ID format."
                        });
                    }
                } else {
                    num = 1; 
                }

                var newPaymentID = num.ToString("D3");

                var newPaymentData = new TrPayment {
                    PaymentId = $"PY{newPaymentID}",
                    PaymentDate = request.PaymentDate,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    RentalId = request.RentalId
                };

                _context.TrPayment.Add(newPaymentData);
                await _context.SaveChangesAsync();

                var rental = await _context.TrRental.FirstOrDefaultAsync(r => r.RentalId == request.RentalId);
                if (rental != null) {
                    rental.PaymentStatus = true;
                    await _context.SaveChangesAsync();
                }

                var response = new ApiResponse<string> {
                    StatusCode = StatusCodes.Status201Created,
                    RequestMethod = HttpContext.Request.Method,
                    Data = "Payment is successful."
                };

                return StatusCode(StatusCodes.Status201Created, response);

            } catch (Exception e) {
                var response = new ApiResponse<string> {
                    StatusCode = StatusCodes.Status400BadRequest,
                    RequestMethod = HttpContext.Request.Method,
                    Data = e.Message
                };
                return BadRequest(response);
            }
        }



    }
}
