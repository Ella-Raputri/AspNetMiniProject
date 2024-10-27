using System;
using RentalCarFront.Models.Output;

namespace RentalCarFront.Service;

public interface ICar
{
    Task<ApiResponse<IEnumerable<GetCarAvailableCard>>> GetAvailableCars(DateTime dateA, DateTime dateB, int? year = null);
    Task<ApiResponse<ApiResponse<IEnumerable<GetCarDesc>>>> GetCarInformation(DateTime dateStart, DateTime dateEnd);
}
