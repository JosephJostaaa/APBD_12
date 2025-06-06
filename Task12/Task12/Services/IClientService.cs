namespace Task12.Services;

public interface IClientService
{
    public Task<bool> DeleteClientAsync(int idClient);
}