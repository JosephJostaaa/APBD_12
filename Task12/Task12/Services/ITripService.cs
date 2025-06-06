using Task12.Dto;

namespace Task12.Services;

public interface ITripService
{
    public Task<TripPage> GetTripsAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<TripRegistrationResponse> AssignClientToTripAsync(int idTrip, AssignClientToTripDto dto);
}