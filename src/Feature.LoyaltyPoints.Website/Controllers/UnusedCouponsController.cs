using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Feature.LoyaltyPoints.Website.Repositories;
using Sitecore.Commerce.Services.Orders;
using Sitecore.XA.Foundation.Mvc.Controllers;

namespace Feature.LoyaltyPoints.Website.Controllers
{
    //Handles highest level response orchestration.  All data comes from repository.
    public class UnusedCouponsController : StandardController
    {
        private readonly ICouponRepository _couponRepository;

        public UnusedCouponsController(ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
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
            return this.Json(_couponRepository.GetUnusedCoupons()); 
        }

    }
}
