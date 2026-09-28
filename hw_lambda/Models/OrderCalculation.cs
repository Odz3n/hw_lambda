using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hw_lambda.Models
{
    public class OrderCalculation
    {
        public decimal Subtotal { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal Discount { get; set; }
        public decimal DeliveryPrice { get; set; }
        public decimal Total { get; set; }
    }
}
