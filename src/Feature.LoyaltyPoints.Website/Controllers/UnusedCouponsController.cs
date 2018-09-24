using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Feature.LoyaltyPoints.Website.Models;
using Sitecore.XA.Foundation.Mvc.Controllers;

namespace Feature.LoyaltyPoints.Website.Controllers
{
    public class UnusedCouponsController : StandardController
    {
        private readonly ICouponRepository _couponRepository;

        public UnusedCouponsController(ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
        }
        protected override object GetModel()
        {
            return new UnusedCouponsRenderingModel();
        }
    }

    public interface ICouponRepository
    {
         //TODO Figure this out. How do I talk to the engine, make use of the Loyalty Coupons view?
    }
}
