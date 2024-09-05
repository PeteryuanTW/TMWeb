﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.API.Parameter
{
    public class ItemRecordParam1
    {
        [Required]
        public string SerialNo { get; set; }
        [Required]
        public string RecordName { get; set; }
        [Required]
        public string RecordValue { get; set; }
    }
}
