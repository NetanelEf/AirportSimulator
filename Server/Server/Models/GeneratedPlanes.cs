using LogicModels.BaseModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    public class GeneratedPlanes
    {
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecordID { get; set; }
        public int GeneratedPlaneID { get; set; }
    }
}
