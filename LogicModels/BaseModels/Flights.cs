using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicModels.BaseModels
{
    public class Flights
    {
        [Key]
        public int id { get; set; }
        public Process ArrivingFlights { get; set; }
        public Process DepartingFlights { get; set; }

        public Process WaitingFlights { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(ArrivingFlights.ToString());
            sb.AppendLine();
            sb.AppendLine(WaitingFlights.ToString());
            sb.AppendLine();
            sb.AppendLine(DepartingFlights.ToString());
            return sb.ToString();
        }
    }
}
