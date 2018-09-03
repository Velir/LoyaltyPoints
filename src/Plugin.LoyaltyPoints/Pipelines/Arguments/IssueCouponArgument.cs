using System.Collections.Generic;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Customers;

namespace Plugin.LoyaltyPoints.Pipelines
{
    public class IssueCouponArgument:PipelineArgument
    {
        public IssueCouponArgument()
        {
            Coupons = new List<string>();
        }
        public Customer Customer { get; set; }
        public List<string> Coupons { get; set; }
    }
}