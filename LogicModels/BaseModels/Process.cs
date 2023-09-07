using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicModels.BaseModels
{
    public class Process
    {
        [Key]
        public int ProcessID { get; set; }
        public List<Location> _locations { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Location l in _locations)
            {
                if (l.Planes.Count != 0)
                {
                    sb.AppendLine($"{l.Id} has {l.Planes.Count} planes in it, plane details are:");
                    foreach (Plane plane in l.Planes)
                    {
                        sb.AppendLine(plane.ToString());
                    }
                }
                else
                {
                    sb.AppendLine($"{l.Id} has 0 planes in it");
                }
            }
            return sb.ToString();
        }
    }
}
