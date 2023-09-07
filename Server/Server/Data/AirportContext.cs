using LogicModels.BaseModels;
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data
{
    public class AirportContext : DbContext
    {
        /*
            3 tables: 
            a. current flights
            b. waiting flights
            c. past flights
        */
        public DbSet<GeneratedPlanes> GeneratedPlanes { get; set; } //flights that have been generated and need to access simulation
        public DbSet<FlightAndLocation> FlightsAndLocations { get; set; } // current flights in the simulation
        public DbSet<WaitingFlight> WaitingFlight { get; set; } //flights that landed and need to depart
        public DbSet<Plane> FlightHistory { get; set; } // contains all of the flights that have been generated 
        public AirportContext(DbContextOptions<AirportContext> options) : base(options)
        {

        }

        public AirportContext(string connectionString) : base(GetOptions(connectionString))
        {
        }

        private static DbContextOptions GetOptions(string connectionString)
        {
            return SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder(), connectionString).Options;
        }
    }
}
