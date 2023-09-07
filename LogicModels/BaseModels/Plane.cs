using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicModels.BaseModels
{
    public class Plane
    {
        [Key]
        public int PlaneID { get; set; }
        public string PlaneCompany { get; set; }
        public string LocationFrom { get; set; }
        public string LocationTo { get; set; }
        public bool MovedLocationAirfield { get; set; } = false;

        public bool IsDeparting { get; set; } = false;

        public DateTime EntryTime { get; set; } = DateTime.Now;
        public DateTime? DepartTime { get; set; }

        public override string ToString()
        {
            return $"{PlaneID} - Plane is coming from: {LocationFrom} and going to: {LocationTo}";
        }
    }
}
