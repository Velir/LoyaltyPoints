using System;
using Feature.LoyaltyPoints.Website.Models.JsonResults;
using Sitecore.Commerce.Services;

namespace Feature.LoyaltyPoints.Website.Repositories
{
    public interface ICouponRepository
    {
        object GetUnusedCouponsRenderingModel();  //TODO Fix type
        CouponsBaseJsonResult GetUnusedCoupons();
    }
}