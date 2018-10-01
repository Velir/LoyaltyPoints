using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Feature.LoyaltyPoints.Website.Repositories;
using Sitecore.Commerce.Services.Orders;
using Sitecore.Commerce.XA.Foundation.Common;
using Sitecore.Commerce.XA.Foundation.Connect;
using Sitecore.XA.Foundation.Mvc.Controllers;

namespace Feature.LoyaltyPoints.Website.Controllers
{
    //Handles highest level response orchestration.  All data comes from repository.
    public class UnusedCouponsController : StandardController
    {
        private readonly ICouponRepository _couponRepository;
        private readonly IStorefrontContext _storefrontContext;
        private readonly IVisitorContext _visitorContext;

        public UnusedCouponsController(ICouponRepository couponRepository, IStorefrontContext storefrontContext, IVisitorContext visitorContext)
        {
            _couponRepository = couponRepository;
            _storefrontContext = storefrontContext;
            _visitorContext = visitorContext;
        }
        protected override object GetModel()
        {
            return _couponRepository.GetUnusedCouponsRenderingModel();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public JsonResult GetUnusedCoupons()
        {
            return this.Json(_couponRepository.GetUnusedCoupons(_storefrontContext, _visitorContext)); 
        }

    }
}
