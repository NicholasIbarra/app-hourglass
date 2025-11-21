using Microsoft.AspNetCore.SignalR;

namespace Infrasturcture.Hubs
{
    public interface IImportProgressClient
    {
        Task ReceiveMessage();
    }


    public class ImportProgressHub : Hub<IImportProgressClient>
    {
        public async Task SendMessage()
        {
            await Clients.All.ReceiveMessage();
        }
    }
}
