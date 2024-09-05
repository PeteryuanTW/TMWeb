using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.API.Parameter
{
    public class StationOutParam1
    {
        [Required]
        public string stationName { get; set; }
        [Required]
        public bool pass { get; set; }
    }
}
