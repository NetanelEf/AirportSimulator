using LogicModels.BaseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Server.Data;
using Server.Hubs;
using Server.Models;
using System;
using System.Text;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AirportController : Controller
    {
        private AirportContext _context { get; set; }
        private readonly IHubContext<FlightHub> _flightHub;
        public AirportController(AirportContext contx, IHubContext<FlightHub> hub)
        {
            _context = contx;
            _flightHub = hub;
        }

        [HttpGet]
        [Route("GetFlightHistory")]
        public ActionResult<IEnumerable<Plane>> GetHistory()
        {
            IEnumerable<Plane> response = SimulatorLogic.SendHistory(out bool success);
            if (success)
            {
                return Ok(response.ToList());
            }
            return NotFound();
        }

        [HttpPost]
        [Route("GetWaitingFlights")]
        public ActionResult<IEnumerable<WaitingFlight>> GetWaitingFlight()
        {
            IEnumerable<WaitingFlight> response = SimulatorLogic.SendWaitingFlights(out bool success);
            if(success)
            {
                return Ok(response.ToList());
            }
            return NotFound();
        }

        [HttpDelete]
        [Route("DeleteFlightFromBoard")]
        public async Task<IActionResult> DeleteFlight(int flightId)
        {
            var flights = SimulatorLogic._flights;
            bool found = false;

            foreach (var flightCollection in new[] { flights.ArrivingFlights, flights.WaitingFlights, flights.DepartingFlights })
            {
                foreach (var location in flightCollection._locations)
                {
                    var planeToRemove = location.Planes.FirstOrDefault(p => p.PlaneID == flightId);
                    if (planeToRemove != null)
                    {
                        location.Planes.Remove(planeToRemove);
                        found = true;
                        break;
                    }
                }
                if (found)
                    break;
            }

            if (found)
                return Ok();
            else
                return NotFound();
        }
    }
}
