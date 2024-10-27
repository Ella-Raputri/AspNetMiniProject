using System;

namespace RentalCarFront.Models.Output;

public class ApiResponse<T>
{
    public string StatusCode { get; set; }
    public string requestMethod { get; set; }
    public T Data { get; set; }
}
