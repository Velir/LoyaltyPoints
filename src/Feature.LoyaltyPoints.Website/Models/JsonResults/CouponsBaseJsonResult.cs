﻿using System.Collections.Generic;
using System.Linq;
using Feature.LoyaltyPoints.Website.Managers;
 
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
            Coupons = new List<Coupon>();
        }

        public virtual void Initialize(ManagerResponse<GetCouponsResult, IEnumerable<Coupon>> managerResponse)
        {
            if (managerResponse?.Result != null)
            {
                Coupons = managerResponse.Result.Where(c=>!c.IsApplied).ToList();
            }
        }
 
    }
}