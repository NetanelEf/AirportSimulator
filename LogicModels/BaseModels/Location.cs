using LogicModels.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicModels.BaseModels
{
    public class Location
    {
        [Key]
        public int Id { get; set; }
        public LocationType _location { get; set; }
        public List<Plane> Planes { get; set; }
    }
}
