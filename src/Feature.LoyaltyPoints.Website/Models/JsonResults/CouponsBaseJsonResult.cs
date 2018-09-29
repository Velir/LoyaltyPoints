using System.Collections.Generic;
using Sitecore.Commerce.XA.Foundation.Common;
using Sitecore.Commerce.XA.Foundation.Common.Models.JsonResults;

namespace Feature.LoyaltyPoints.Website.Models.JsonResults
{
    public class CouponsBaseJsonResult:BaseJsonResult
    {
        public virtual List<Coupon> Coupons { get; private set; }

        public CouponsBaseJsonResult(IStorefrontContext storefrontContext) : base(storefrontContext)
        {
            
        }

        public virtual void Initialize(/* TODO GetCustomerCouponsResult result*/)
        {
            Coupons = new List<Coupon>{new Coupon{Name="1"}, new Coupon { Name = "2" } , new Coupon { Name = "3" } };
        }
 
    }
}