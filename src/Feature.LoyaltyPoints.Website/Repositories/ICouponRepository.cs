using System;
using Feature.LoyaltyPoints.Website.Models;
using Feature.LoyaltyPoints.Website.Models.JsonResults;
using Sitecore.Commerce.Services;
using Sitecore.Commerce.XA.Foundation.Common;
using Sitecore.Commerce.XA.Foundation.Connect;

namespace Feature.LoyaltyPoints.Website.Repositories
{
    public interface ICouponRepository
    {
        LoyaltyCouponsRenderingModel GetUnusedCouponsRenderingModel();
        CouponsBaseJsonResult GetUnusedCoupons(IStorefrontContext storefrontContext, IVisitorContext visitorContext);
    }
}