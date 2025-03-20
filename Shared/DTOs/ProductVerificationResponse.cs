using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public class ProductVerificationResponse
    {
        public int ProductId { get; set; }
        public bool IsValid { get; set; }
        public decimal Price { get; set; }
        public string Message { get; set; }
    }
}
