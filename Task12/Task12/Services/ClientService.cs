using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Task12.Dto;
using Task12.Exceptions;
using Task12.Models;

namespace Task12.Services;

public class ClientService : IClientService
{
    private readonly Task12Context _context;

    public ClientService(Task12Context context)
    {
        _context = context;
    }

    public async Task<bool> DeleteClientAsync(int idClient)
    {
        var client = await _context.Clients
            .Include(c => c.ClientTrips)
            .FirstOrDefaultAsync(c => c.IdClient == idClient);

        if (client == null)
            throw new NotFoundException($"Client with ID {idClient} does not exist.");

        if (client.ClientTrips.Any())
            return false;

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return true;
    }

}