using LogicModels.BaseModels;
using Microsoft.AspNetCore.SignalR;

namespace Server.Hubs
{
    //[HubName("FlightHub")]
    public sealed class FlightHub : Hub
    {
        public async Task SendMessage()
        {
            if(SimulatorLogic._flights != null)
                await Clients.All.SendAsync("ReceiveMessage", SimulatorLogic._flights.ToString());
        }

        public async Task SendSimulator()
        {
            // Call the broadcastSimulator method to update clients.
            if (SimulatorLogic._flights != null)
                await Clients.All.SendAsync("BroadcastSimulator", SimulatorLogic._flights);
        }
    }
}
