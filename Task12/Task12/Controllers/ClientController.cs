using Microsoft.AspNetCore.Mvc;
using Task12.Services;

namespace Task12.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpDelete("{idClient}")]
    public async Task<IActionResult> DeleteClient(int idClient)
    {
        var result = await _clientService.DeleteClientAsync(idClient);

        if (!result)
        {
            return BadRequest(new { message = "Client with id " + idClient +" is already assigned to a trip"});
        }

        return NoContent();
    }
}
