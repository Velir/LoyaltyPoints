using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Commerce.Services;
using Sitecore.Commerce.XA.Foundation.Common;
using Sitecore.Commerce.XA.Foundation.Common.Models.JsonResults;

namespace Feature.LoyaltyPoints.Website.Models.JsonResults
{
    public class LoyaltyCouponListJsonResult: BaseJsonResult
    {
        public LoyaltyCouponListJsonResult(IStorefrontContext storefrontContext) : base(storefrontContext)
        {
        }

        //TODO
    }
}