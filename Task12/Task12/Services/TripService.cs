using Microsoft.EntityFrameworkCore;
using Task12.Dto;
using Task12.Exceptions;
using Task12.Models;

namespace Task12.Services;

public class TripService : ITripService
{
    private readonly Task12Context _context;
    public TripService(Task12Context context)
    {
        _context = context;
    }
    public async Task<TripPage> GetTripsAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var trips = await _context.Trips
            .Include(t => t.ClientTrips)
            .ThenInclude(ct => ct.IdClientNavigation)
            .Include(t => t.IdCountries)
            .OrderByDescending(t => t.DateFrom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalTrips = await _context.Trips.CountAsync(cancellationToken);
        var allPages = (int)Math.Ceiling((double)totalTrips / pageSize);
        
        var tripPage = new TripPage
        {
            PageNumber = page,
            PageSize = pageSize,
            AllPages = allPages,
            
            Trips = trips.Select(t => new TripDto
            {
                Name = t.Name,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                Description = t.Description,
                MaxPeople = t.MaxPeople,
                Countries = t.IdCountries.Select(c => new CountryDto
                {
                    Name = c.Name
                }).ToList(),
                Clients = t.ClientTrips.Select(ct => new ClientDto
                {
                    FirstName = ct.IdClientNavigation.FirstName,
                    LastName = ct.IdClientNavigation.LastName,
                }).ToList()
            }).ToList()
        };
        return tripPage;
    }

    public async Task<TripRegistrationResponse> AssignClientToTripAsync(int idTrip, AssignClientToTripDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
            try
            {
                var trip = await _context.Trips
                    .Include(t => t.ClientTrips)
                    .FirstOrDefaultAsync(t => t.IdTrip == idTrip);
        
                if (trip == null)
                    throw new NotFoundException("Trip not found.");
        
                if (trip.DateFrom <= DateTime.Now)
                    return new TripRegistrationResponse {Success = false, Message = "Trip has already started."};
        
                var existingClient = await _context.Clients
                    .FirstOrDefaultAsync(c => c.Pesel == dto.Pesel);
        
                if (existingClient != null)
                {
                    var alreadyRegistered = await _context.ClientTrips.AnyAsync(ct =>
                        ct.IdClient == existingClient.IdClient && ct.IdTrip == idTrip);
        
                    if (alreadyRegistered)
                        return new TripRegistrationResponse {Success = false, Message = "Client is already registered for this trip."};
        
                    return new TripRegistrationResponse {Success = false, Message = "Client with this PESEL already exists."};
                }
        
                var newClient = new Client
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    Telephone = dto.Telephone,
                    Pesel = dto.Pesel
                };
        
                await _context.Clients.AddAsync(newClient);
                await _context.SaveChangesAsync();
        
                DateTime? paymentDate = null;
                if (dto.PaymentDate != null)
                {
                    paymentDate = DateTime.Parse(dto.PaymentDate);
                }
        
                var clientTrip = new ClientTrip
                {
                    IdClient = newClient.IdClient,
                    IdTrip = idTrip,
                    RegisteredAt = DateTime.Now,
                    PaymentDate = paymentDate
                };
        
                await _context.ClientTrips.AddAsync(clientTrip);
                
        
                await _context.SaveChangesAsync();
        
                await transaction.CommitAsync();
                return new TripRegistrationResponse{Success = true, Message = null};
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new TripRegistrationResponse{Success = false, Message = "An error occurred while assigning client to trip."};
            }
    }
}