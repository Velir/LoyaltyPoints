using System.Collections.Generic;
using System.Linq;
using Feature.LoyaltyPoints.Website.Managers;
using Sitecore.Commerce.Plugin.Coupons;
using Sitecore.Commerce.XA.Foundation.Common;
using Sitecore.Commerce.XA.Foundation.Common.Models.JsonResults;
using Sitecore.Commerce.XA.Foundation.Connect;
using Sitecore.Commerce.XA.Foundation.Connect.Managers;

namespace Feature.LoyaltyPoints.Website.Models.JsonResults
{
    public class CouponsBaseJsonResult:BaseJsonResult
    {
        public virtual List<Coupon> Coupons { get; private set; }

        public CouponsBaseJsonResult(IStorefrontContext storefrontContext) : base(storefrontContext)
        {
            
        }

        public virtual void Initialize(ManagerResponse<GetCouponsResult, IEnumerable<Coupon>> managerResponse)
        {
            Coupons = managerResponse.Result.ToList();
        }
 
    }
}