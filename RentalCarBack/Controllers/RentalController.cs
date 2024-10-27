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

                var newCustomerData = new MsCustomer{
                    CustomerId = $"CUS{newCustomerID}",
                    Email = request.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Name = request.Name,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    DriverLicenseNumber = $"DLN{newCustomerID}"
                };

                _context.MsCustomer.Add(newCustomerData);
                await _context.SaveChangesAsync();

                var response = new ApiResponse<string>{
                    StatusCode = StatusCodes.Status201Created,
                    RequestMethod = HttpContext.Request.Method,
                    Data = "New customer account is created."
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
        public async Task<ActionResult<ApiResponse<string>>> Login([FromBody] LoginCustomerRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    RequestMethod = HttpContext.Request.Method,
                    Data = "Invalid login request."
                });
            }

            var customer = await _context.MsCustomer
                .FirstOrDefaultAsync(c => c.Name == request.Name);

            if (customer == null)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    RequestMethod = HttpContext.Request.Method,
                    Data = "Invalid email or password."
                });
            }

            bool isPasswordValid = false;
            if (customer.Password.StartsWith("$2a$") || customer.Password.StartsWith("$2b$") || customer.Password.StartsWith("$2y$")){
                isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, customer.Password);
            }
            else{
                isPasswordValid = request.Password == customer.Password;
            }
            
            if (!isPasswordValid)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    RequestMethod = HttpContext.Request.Method,
                    Data = "Invalid email or password."
                });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, customer.CustomerId),
                new Claim(ClaimTypes.Name, customer.Name),
            };

            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("Cookies", claimsPrincipal);
            HttpContext.Session.SetString("CustomerId", customer.CustomerId);
            HttpContext.Session.SetString("CustomerName", customer.Name);

            return Ok(new ApiResponse<string>
            {
                StatusCode = StatusCodes.Status200OK,
                RequestMethod = HttpContext.Request.Method,
                Data = "Login successful."
            });
        }


        // Get current customer 
        [HttpGet("current-customer")]
        public async Task<ActionResult<GetCustomerInformation>> GetCurrentCustomer()
        {
            try{
                var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if(string.IsNullOrEmpty(userID)){
                    return Unauthorized("User not logged in.");
                }

                var userData = await _context.MsCustomer
                    .Where(c => c.CustomerId == userID)
                    .Select(c => new GetCustomerInformation{
                        CustomerId = c.CustomerId,
                        Name = c.Name,
                        Email = c.Email,
                        PhoneNumber = c.PhoneNumber,
                        Address = c.Address
                    }).FirstOrDefaultAsync();
                
                if(userData == null){
                    return NotFound("Customer not found.");
                }

                var response = new ApiResponse<GetCustomerInformation>
                {
                    StatusCode = StatusCodes.Status200OK,
                    RequestMethod = HttpContext.Request.Method,
                    Data = userData
                };

                return Ok(response);
            }

            catch(Exception e){
                var response = new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    RequestMethod = HttpContext.Request.Method,
                    Data = e.Message 
                };
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }


        // Get information tentang penyewa dan mobil saat klik sewa sekarang
        [HttpGet("info")]
        public async Task<ActionResult<ApiResponse<IEnumerable<GetCarInformation>>>> GetCarInformation(DateTime dateStart, DateTime dateEnd)
        {
            var customerName = HttpContext.Session.GetString("CustomerName");
            var customerId = HttpContext.Session.GetString("CustomerId");

            if (string.IsNullOrEmpty(customerId) || string.IsNullOrEmpty(customerName))
            {
                return NotFound("Customer not found.");
            }

            var availableCarsQuery = _context.MsCar
                .Where(car => car.CarImage != null && !_context.TrRental
                    .Any(rental =>
                        rental.CarId == car.CarId &&
                        rental.RentalDate < dateEnd &&
                        (rental.ReturnDate == null || rental.ReturnDate > dateStart)))
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
                });

            // Pagination
            // var totalCars = await availableCarsQuery.CountAsync();
            var availableCars = await availableCarsQuery.ToListAsync();
                // .Skip((page - 1) * pageSize)
                // .Take(pageSize)
                // .ToListAsync();

            if (!availableCars.Any()){
                return NoContent();
            }

            return Ok(new ApiResponse<IEnumerable<GetCarInformation>>
            {
                Data = availableCars,
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
        public async Task<ActionResult<IEnumerable<GetRentalHistory>>> GetHistory()
        {
            var userId = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("Customer not found.");
            }

            var paymentData = await _context.TrPayment
                .Join(_context.TrRental,
                    payment => payment.RentalId,
                    rental => rental.RentalId,
                    (payment, rental) => new { payment, rental })
                .Join(_context.MsCar,
                    combined => combined.rental.CarId,
                    car => car.CarId,
                    (combined, car) => new 
                    {
                        combined.payment,
                        combined.rental,
                        car
                    })
                .Where(data => data.rental.CustomerId == userId) // Filter by CustomerId before selecting
                .Select(data => new GetRentalHistory
                {
                    RentalDate = $"{data.rental.RentalDate:dd MMMM yyyy} - {data.rental.ReturnDate:dd MMMM yyyy}",
                    CarName = data.car.Name,
                    PricePerDay = data.car.PricePerDay,
                    TotalDays = (data.rental.ReturnDate - data.rental.RentalDate).Days,
                    TotalPrice = data.car.PricePerDay * (data.rental.ReturnDate - data.rental.RentalDate).Days,
                    Status = data.payment.PaymentDate.HasValue
                })
                .ToListAsync();

            if (!paymentData.Any())
            {
                return NoContent();
            }

            return Ok(paymentData);
        }

    }
}
