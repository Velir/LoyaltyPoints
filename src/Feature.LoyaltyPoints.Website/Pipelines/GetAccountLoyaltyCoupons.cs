using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Engine;
using Sitecore.Commerce.Engine.Connect.Entities;
using Sitecore.Commerce.Engine.Connect.Pipelines;
using Sitecore.Commerce.Entities;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Pipelines;
using Sitecore.Commerce.Plugin.Coupons;
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
            Assert.IsNotNull(request, "args.Request");
            Assert.IsNotNull(result, "args.Result");
              
            Container container = this.GetContainer(request.Shop.Name, string.Empty, request.CommerceCustomer.ExternalId, "", args.Request.CurrencyCode, new DateTime?());
            EntityView customerView = this.GetEntityView(container, request.CommerceCustomer.ExternalId, string.Empty, "Customer", string.Empty, (ServiceProviderResult)result);
            if (!result.Success)
                return;
             List<Coupon> couponList = new List<Coupon>();
            EntityView loyaltyCoupons = customerView.ChildViews.Where<Model>((Func<Model, bool>)(v => v.Name.Equals("LoyaltyCoupons", StringComparison.OrdinalIgnoreCase))).FirstOrDefault<Model>() as EntityView;
            if (loyaltyCoupons != null || loyaltyCoupons.ChildViews.Any<Model>())
            {
                foreach (Model childView in (Collection<Model>)loyaltyCoupons.ChildViews)
                {
                    Coupon couponEntity = this.TranslateViewToCoupon(childView as EntityView, (ServiceProviderResult)result);
                    couponList.Add(couponEntity);
                }
            }
            result.Coupons = (IReadOnlyCollection<Coupon>)couponList.AsReadOnly();  //EXPECTED FOR NOW
        }

        private Coupon TranslateViewToCoupon(EntityView entityView, ServiceProviderResult result)
        {
            throw new NotImplementedException();
        }
    }
}
   