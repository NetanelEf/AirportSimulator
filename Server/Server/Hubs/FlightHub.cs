using LogicModels.BaseModels;
using Microsoft.AspNetCore.SignalR;

namespace Server.Hubs
{
    public sealed class FlightHub : Hub
    {
        protected static IHubContext<FlightHub> _context;

        public FlightHub(IHubContext<FlightHub> context)
        {
            _context = context;
        }

        public async Task SendMessage()
        {
            if(SimulatorLogic._flights != null)
                await _context.Clients.All.SendAsync("ReceiveMessage", SimulatorLogic._flights.ToString());
        }

        public static async Task SendSimulator()
        {
            // Call the broadcastSimulator method to update clients.
            if (_context != null && SimulatorLogic._flights != null)
                await _context.Clients.All.SendAsync("BroadcastSimulator", SimulatorLogic._flights);
        }
    }
}
