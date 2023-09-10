using LogicModels;
using LogicModels.BaseModels;
using LogicModels.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Data;
using Server.Models;
using System.Data.Common;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;
using Server.Hubs;

namespace Server
{
    public class SimulatorLogic
    {
        private Service _service;
        private Process _landing, _departing, _waiting;

        public static Flights _flights { get; private set; }
        //public static FlightHub _hub { get; private set; }

        /*
         Constructor that initializes the simulation with the locations and flight list
         Constructor also hooks the simulator functions to event and starts to run it
         */
        public SimulatorLogic()
        {
            _service = new Service();
            //_hub = new FlightHub();

            _landing = new Process()
            {
                _locations = new List<Location>() {
                    new Location() { Id = 1, Planes = new List<Plane>() { }, _location = LocationType.LandingAirspace},
                    new Location() { Id = 2, Planes = new List<Plane>() { }, _location = LocationType.LandingAirspace },
                    new Location() { Id = 3, Planes = new List<Plane>() { }, _location = LocationType.LandingAirspace },
                    new Location() { Id = 4, Planes = new List<Plane>() { }, _location = LocationType.Track},
                    new Location() { Id = 5, Planes = new List<Plane>() { }, _location = LocationType.LandingTransferTrack}
                }
            };

            _departing = new Process()
            {
                _locations = new List<Location>()
                {
                    new Location() { Id = 8, Planes = new List<Plane>() { }, _location = LocationType.DepartingTransferTrack},
                    new Location() { Id = 4, Planes = new List<Plane>() { }, _location = LocationType.Track},
                    new Location() { Id = 9, Planes = new List<Plane>() { }, _location = LocationType.DepartAirspace}
                }
            };

            _waiting = new Process()
            {
                _locations = new List<Location>()
                {
                    new Location() { Id = 6, Planes = new List<Plane>() { }, _location = LocationType.Waiting},
                    new Location() { Id = 7, Planes = new List<Plane>() { }, _location = LocationType.Waiting}
                }
            };

            _flights = new Flights() { ArrivingFlights = _landing, DepartingFlights = _departing, WaitingFlights = _waiting };

            //if there are existing flights in the system initializes the flight simulator with them
            using (var context = new AirportContext("server=(localdb)\\MSSQLLocalDB;database=AirportDB;MultipleActiveResultSets=true;"))
            {
                if (context.FlightsAndLocations.Count() != 0)
                    InitializeLocations(_flights);
            }

            //hooking simulator functions to events
            EventAdd addPlaneFunction = PlaneAdd;
            _service.HookFunction(addPlaneFunction);

            EventRunSimulation functionPass = RunSimulation;
            _service.HookFunction(functionPass, _flights);

            EventWaiting waitingProcess = ProcessingWaiting;
            _service.HookFunction(waitingProcess, _flights);

            EventAddFromWaiting addBackPlane = AddBackToWaiting;
            _service.HookFunction(addBackPlane);

            EventSendSimulatorData sendSimData = FlightHub.SendSimulator;
            _service.HookFunction(sendSimData);
        }

        #region SimulatorFunctions

        /*
         Function is responsible for generating new planes and adding them to generated flight queue
         */
        public async void PlaneAdd(object sender, EventArgs e)
        {
            Random rng = new Random();
            List<string> destinations = new List<string>() { "New York", "London", "Miami", "Bangkok", "Mexico City", "Tel Aviv", "Vienna", "Budapest", "Madrid", "Berlin", "Amsterdam", "Paris", "Warsaw", "Tokyo", "Manchester" };
            List<string> PlaneCompanies = new List<string>() { "Boeing", "Lockheed Martin", "Aurbus", "Raytheon", "Northrop Grumman", "Safran", "Leonardo", "Bombardier" };
            string arrivalDest = destinations[rng.Next(0, destinations.Count)];
            string departDest = destinations[rng.Next(0, destinations.Count)];
            while (arrivalDest.Equals(departDest))
            {
                departDest = destinations[rng.Next(0, destinations.Count)];
            }
            Plane newPlane = new Plane { IsDeparting = false, LocationFrom = arrivalDest, LocationTo = departDest, MovedLocationAirfield = true, PlaneCompany = PlaneCompanies[rng.Next(0, PlaneCompanies.Count)] };
            using (var context = new AirportContext("server=(localdb)\\MSSQLLocalDB;database=AirportDB;MultipleActiveResultSets=true;"))
            {
                if (context.FlightsAndLocations.Where(record => record.LocationID == 1).ToList().Count() == 0)
                {
                    newPlane.MovedLocationAirfield = true;
                    newPlane.EntryTime = DateTime.Now;
                    context.FlightHistory.Add(newPlane);
                    context.SaveChanges();
                    context.GeneratedPlanes.Add(new GeneratedPlanes() { GeneratedPlaneID = newPlane.PlaneID });
                    context.FlightsAndLocations.Add(new FlightAndLocation { CurrPlaneID = newPlane.PlaneID, LocationID = 1 });
                    context.SaveChanges();
                }
            }
        }

        /*
         Function is responsible for running the simulation:
         1. checks if new planes have been added and if so adds them to the simulator
         2. moves the planes in the arriving and departing location lists
         */
        public void RunSimulation(object sender, EventArgs e, Flights flights)
        {
            using (var context = new AirportContext("server=(localdb)\\MSSQLLocalDB;database=AirportDB;MultipleActiveResultSets=true;"))
            {
                Console.WriteLine(_flights.ToString());
                if (context.GeneratedPlanes.Count() != 0)
                {
                    var plane = context.FlightHistory.Join(context.FlightHistory, plane => plane.PlaneID, genPlane => genPlane.PlaneID, (a, b) => a).OrderBy(p => p.EntryTime).Last();
                    GeneratedPlanes planeID = context.GeneratedPlanes.Find(plane.PlaneID);
                    OrderAirControl(plane, planeID);
                    var temp = context.GeneratedPlanes.FirstOrDefault(item => item.GeneratedPlaneID == plane.PlaneID);
                    if (temp != null)
                    {
                        context.GeneratedPlanes.Remove(temp);
                        context.SaveChanges();
                    }
                }

                Processing(_flights.ArrivingFlights, _flights.WaitingFlights, true);
                Processing(_flights.DepartingFlights, _flights.WaitingFlights, false);

                foreach (var loc in _flights.ArrivingFlights._locations) //reset moves
                    loc.Planes.ForEach(plane => plane.MovedLocationAirfield = false);

                foreach (var loc in _flights.DepartingFlights._locations) //reset moves
                    loc.Planes.ForEach(plane => plane.MovedLocationAirfield = false);
            }
        }

        /*
         Function is responsible for handling planes that have entered the waiting process:
         1. if plane is in waiting process and not departing then insert to waiting queue
         2. if plane is in waiting process and departing then send to departing process (if place is available)
         */
        public void ProcessingWaiting(object sender, EventArgs e, Flights flights)
        {
            using (var context = new AirportContext("server=(localdb)\\MSSQLLocalDB;database=AirportDB;MultipleActiveResultSets=true;"))
            {
                for (int i = 0; i < 2; i++) // arriving flights -> waiting flights or waiting flights -> departing flights
                {
                    if (_flights.WaitingFlights._locations[i].Planes.Count != 0)
                    {
                        if (_flights.WaitingFlights._locations[i].Planes.ElementAt(0).IsDeparting && _flights.DepartingFlights._locations[0].Planes.Count() == 0)
                        {
                            var plane = _flights.WaitingFlights._locations[i].Planes.ElementAt(0);
                            plane.MovedLocationAirfield = true;
                            _flights.DepartingFlights._locations[0].Planes.Add(plane);
                            _flights.WaitingFlights._locations[i].Planes.RemoveAt(0);
                            context.FlightsAndLocations.Add(new FlightAndLocation() { CurrPlaneID = plane.PlaneID, LocationID = _flights.DepartingFlights._locations[0].Id });
                            context.WaitingFlight.Remove(context.WaitingFlight.First(item => item.CurrPlaneID == plane.PlaneID));
                            context.SaveChanges();
                        }
                        else if (!_flights.WaitingFlights._locations[i].Planes.ElementAt(0).IsDeparting) //if arriving flight wants to go to waiting queue in db from waiting process
                        {
                            var plane = _flights.WaitingFlights._locations[i].Planes.ElementAt(0);
                            context.FlightHistory.First(item => item.PlaneID == plane.PlaneID).IsDeparting = true;
                            context.WaitingFlight.Add(new WaitingFlight() { CurrPlaneID = plane.PlaneID });
                            context.FlightsAndLocations.Remove(context.FlightsAndLocations.First(item => item.CurrPlaneID == plane.PlaneID));
                            _flights.WaitingFlights._locations[i].Planes.RemoveAt(0);
                            context.SaveChanges();
                        }
                    }
                }
            }
        }

        /*
         Function is responsible for adding flights back from waiting queue
         */
        public void AddBackToWaiting(object sender, EventArgs e)
        {
            bool flag = true;
            using (var context = new AirportContext("server=(localdb)\\MSSQLLocalDB;database=AirportDB;MultipleActiveResultSets=true;"))
            {
                if (context.WaitingFlight.Count() != 0)
                {
                    var plane = context.FlightHistory.First(item => item.PlaneID == context.WaitingFlight.ToList().ElementAt(0).CurrPlaneID);
                    for (int i = 0; i < 2 && flag; i++)
                    {
                        if (_flights.WaitingFlights._locations[i].Planes.Count() == 0)
                        {
                            _flights.WaitingFlights._locations[i].Planes.Add(plane);
                            flag = false;
                        }
                    }
                }
            }
        }

        #endregion

        #region Helpers

        /*
         Function helps to make the code cleaner and sends each process to their own co-responding Move-Operation
         */
        public void Processing(Process workingProcess, Process WaitingFlights, bool isArriving)
        {
            if (isArriving)
                ArrivingOperation(workingProcess, WaitingFlights);
            else
                DepartingOperation(workingProcess, WaitingFlights);
        }

        /*
         Function Moves the Arriving flights with it's own behavior
         */
        private void ArrivingOperation(Process workingProcess, Process WaitingFlights)
        {
            bool flag = false;
            using (var context = new AirportContext("server=(localdb)\\MSSQLLocalDB;database=AirportDB;MultipleActiveResultSets=true;"))
            {

                for (int i = 0; i < workingProcess._locations.Count; i++)
                {
                    var location = workingProcess._locations[i];
                    if (location.Planes.Count != 0 && location.Planes.ElementAt(0).MovedLocationAirfield != true)
                    {
                        if (i == workingProcess._locations.Count - 1) //last index of arriving flight locations
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                if (_flights.WaitingFlights._locations[j].Planes.Count == 0)
                                {
                                    _flights.WaitingFlights._locations[j].Planes.Add(location.Planes.ElementAt(0));
                                    location.Planes.RemoveAt(0);
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        else if (workingProcess._locations[i + 1].Planes.Count <= 1 &&
                            (workingProcess._locations[i + 1]._location == LocationType.Track || workingProcess._locations[i + 1]._location == LocationType.LandingTransferTrack))
                        {
                            location.Planes.First().MovedLocationAirfield = true;
                            workingProcess._locations[i + 1].Planes.Add(location.Planes.First());
                            var flightAndLocation = context.FlightsAndLocations.FirstOrDefault(item => item.CurrPlaneID == location.Planes.First().PlaneID);
                            if (flightAndLocation != null)
                            {
                                flightAndLocation.LocationID += 1;
                                location.Planes.RemoveAt(0);
                                flag = true;
                            }
                        }
                        else if (workingProcess._locations[i + 1].Planes.Count == 0)
                        {
                            location.Planes.First().MovedLocationAirfield = true;
                            workingProcess._locations[i + 1].Planes.Add(location.Planes.First());
                            var flightAndLocation = context.FlightsAndLocations.FirstOrDefault(item => item.CurrPlaneID == location.Planes.First().PlaneID);
                            if (flightAndLocation != null)
                            {
                                flightAndLocation.LocationID += 1;
                                location.Planes.RemoveAt(0);
                                flag = true;
                            }
                        }
                    }
                }
                if (flag)
                    context.SaveChanges();
            }
        }

        /*
        Function Moves the Departing flights with it's own behavior
        */
        private void DepartingOperation(Process workingProcess, Process WaitingFlights)
        {
            using (var context = new AirportContext("server=(localdb)\\MSSQLLocalDB;database=AirportDB;MultipleActiveResultSets=true;"))
            {
                for (int i = 0; i < workingProcess._locations.Count; i++)
                {
                    switch (i)
                    {
                        case 0:
                            if (workingProcess._locations[i].Planes.Count != 0 && workingProcess._locations[i + 1].Planes.Count <= 1 && workingProcess._locations[i].Planes.ElementAt(0).MovedLocationAirfield != true) //if there are any planes in departing track move them to track
                            {
                                var plane = workingProcess._locations[i].Planes.ElementAt(0);
                                plane.MovedLocationAirfield = true;
                                workingProcess._locations[i + 1].Planes.Add(plane);
                                context.FlightsAndLocations.First(item => item.CurrPlaneID == plane.PlaneID).LocationID = 8;
                                context.SaveChanges();
                                workingProcess._locations[i].Planes.RemoveAt(0);
                            }
                            break;
                        case 1:
                            if (workingProcess._locations[i + 1].Planes.Count == 0 && workingProcess._locations[i].Planes.Count != 0 && workingProcess._locations[i].Planes.ElementAt(0).MovedLocationAirfield != true) //if there are any planes in track move them to departing airspace
                            {
                                var plane = workingProcess._locations[i].Planes.ElementAt(0);
                                workingProcess._locations[i].Planes.ElementAt(0).MovedLocationAirfield = true;
                                workingProcess._locations[i + 1].Planes.Add(workingProcess._locations[i].Planes.ElementAt(0));
                                context.FlightsAndLocations.First(item => item.CurrPlaneID == plane.PlaneID).LocationID = 9;
                                context.SaveChanges();
                                workingProcess._locations[i].Planes.RemoveAt(0);
                            }
                            break;
                        case 2:
                            if (workingProcess._locations[i].Planes.Count != 0 && workingProcess._locations[i].Planes.ElementAt(0).MovedLocationAirfield != true) //if there are any planes in departing airspace remove them
                            {
                                var plane = workingProcess._locations[i].Planes.ElementAt(0);
                                context.FlightHistory.First(i => i.PlaneID == plane.PlaneID).DepartTime = DateTime.Now;
                                context.FlightsAndLocations.Remove(context.FlightsAndLocations.First(item => item.CurrPlaneID == plane.PlaneID));
                                context.SaveChanges();
                                workingProcess._locations[i].Planes.RemoveAt(0);
                                Console.WriteLine("************** PLANE HAS DEPARTED **************");
                            }
                            break;

                    }
                }
            }
        }

        /*
         Function inserts the new plane to the matching process
         */
        public void OrderAirControl( Plane newPlane, GeneratedPlanes planeToRemove)
        {
            if (!newPlane.IsDeparting)
                _flights.ArrivingFlights._locations[0].Planes.Add(newPlane);
            else
                _flights.DepartingFlights._locations[0].Planes.Add(newPlane);
        }

        /*
         Function initializes the processes of the flights objects
         */
        private void InitializeLocations(Flights flights)
        {
            using (var context = new AirportContext("server=(localdb)\\MSSQLLocalDB;database=AirportDB;MultipleActiveResultSets=true;"))
            {
                foreach (var el in context.FlightsAndLocations)
                {
                    var currPlane = context.FlightHistory.First(item => item.PlaneID == el.CurrPlaneID);
                    if (_flights.WaitingFlights._locations.Any(item => item.Id == el.LocationID))
                    {
                        _flights.WaitingFlights._locations.First(item => item.Id == el.LocationID).Planes.Add(currPlane);
                    }
                    else if (currPlane.IsDeparting)
                    {
                        _flights.DepartingFlights._locations.First(item => item.Id == el.LocationID).Planes.Add(currPlane);
                    }
                    else
                    {
                        _flights.ArrivingFlights._locations.First(item => item.Id == el.LocationID).Planes.Add(currPlane);
                    }
                }
            }
        }
        #endregion

        #region WebAPIDataFunctions
        public static IEnumerable<Plane> SendHistory(out bool success)
        {
            success = false;
            using (var context = new AirportContext("server=(localdb)\\MSSQLLocalDB;database=AirportDB;MultipleActiveResultSets=true;"))
            {
                if (context.FlightsAndLocations != null)
                {
                    // Materialize the data before disposing of the context
                    var flightHistory = context.FlightHistory.ToList();

                    if (flightHistory.Count > 0)
                    {
                        success = true;
                        return flightHistory;
                    }
                }
            }
            return Enumerable.Empty<Plane>().ToList();
        }

        public static IEnumerable<WaitingFlight> SendWaitingFlights(out bool success)
        {
            success = false;
            using (var context = new AirportContext("server=(localdb)\\MSSQLLocalDB;database=AirportDB;MultipleActiveResultSets=true;"))
            {
                if (context.WaitingFlight != null)
                {
                    // Materialize the data before disposing of the context
                    var waitingFlights = context.WaitingFlight.ToList();

                    if (waitingFlights.Count > 0)
                    {
                        success = true;
                        return waitingFlights;
                    }
                }
            }
            return Enumerable.Empty<WaitingFlight>().ToList();
        }
    }
    #endregion
}