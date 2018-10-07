using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using Feature.LoyaltyPoints.Website.Managers;
using Feature.LoyaltyPoints.Website.Models;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Engine;
using Sitecore.Commerce.Engine.Connect.Entities;
using Sitecore.Commerce.Engine.Connect.Pipelines;
using Sitecore.Commerce.Entities;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Pipelines;
using Sitecore.Commerce.Services;
using Sitecore.Commerce.Services.Customers;
using Sitecore.Diagnostics;

namespace Feature.LoyaltyPoints.Website.Pipelines
{
    public class GetCustomerCoupons: Sitecore.Commerce.Engine.Connect.Pipelines.PipelineProcessor
    {
        public IEntityFactory EntityFactory { get; set; }

        public GetCustomerCoupons(IEntityFactory entityFactory)
        {
            Assert.ArgumentNotNull((object)entityFactory, nameof(entityFactory));
            this.EntityFactory = entityFactory;
        }

        public override void Process(ServicePipelineArgs args)
        {
            // Below in lieu of PipelineUtility.ValidateArguments, which is used by Sitecore's Commerce.Engine.Connect processors, but is internal.
            GetCouponsRequest request = args.Request as GetCouponsRequest;
            GetCouponsResult result = args.Result as GetCouponsResult;
            Assert.IsNotNull(request, $"args.Request is null or not of type {nameof(GetCouponsRequest)}.");
            Assert.IsNotNull(result, $"args.Result is null or not of type {nameof(GetCouponsResult)}.");
              
            Container container = this.GetContainer(request.Shop.Name, string.Empty, request.CustomerId, "", args.Request.CurrencyCode, new DateTime?());


            EntityView entityView = this.GetEntityView(container, request.CustomerId, string.Empty, "LoyaltyCoupons", string.Empty, result);

            if (result == null || result.Success == false)
            {
                return;
            }

            var couponList = entityView.ChildViews.OfType<EntityView>().Where(v=>v.Name=="Coupon").Select(ConvertEntityToCoupon).ToList();
          

            result.Coupons = couponList;  
        }

        private Coupon ConvertEntityToCoupon(EntityView ev)
        {
            return new Coupon(ev);
        }
    }
}
   