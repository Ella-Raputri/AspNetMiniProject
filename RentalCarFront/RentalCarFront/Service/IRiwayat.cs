using System;
using RentalCarFront.Models.Output;

namespace RentalCarFront.Service;

public interface IRiwayat
{
    Task<ApiResponse<IEnumerable<GetRiwayatUser>>> GetHistoryUser(string id);

}
