﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public class CreatedLogDto
    {
        public int OrderId { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Message { get; set; }
    }
}
