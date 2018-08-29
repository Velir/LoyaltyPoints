using System.Collections.Generic;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Commerce.Plugin.Orders;

namespace Plugin.LoyaltyPoints.Pipelines.Arguments
{
    public class AllocateCouponArgument
    {
        public AllocateCouponArgument(Customer customer, IList<Order> orders)
        {
            Customer = customer;
            Orders = orders;
        }
        public Customer Customer { get; set; }
        public IList<Order> Orders { get; set; }
        
        public string CouponCode { get; set; }

    
    }
}