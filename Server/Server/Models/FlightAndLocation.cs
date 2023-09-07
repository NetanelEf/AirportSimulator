using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    public class FlightAndLocation
    {
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecordID { get; set; }
        public int CurrPlaneID { get; set; }
        public int LocationID { get; set; }
    }
}