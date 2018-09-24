using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.LoyaltyPoints.Components;
using Plugin.LoyaltyPoints.Models;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Framework.Pipelines;
using Sitecore.Commerce.Plugin.Coupons;

namespace Plugin.LoyaltyPoints.Pipelines.Blocks
{
    [PipelineDisplayName("LoyaltyPoints.Block.GetCustomerLoyaltyCouponsViews")]
    public class GetCustomerLoyaltyCouponsViewBlock:PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly IFindEntityPipeline _findEntity;


        public GetCustomerLoyaltyCouponsViewBlock(IFindEntityPipeline findEntity)
        {
            _findEntity = findEntity;
        }
        public async override Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            EntityViewArgument request = context.CommerceContext.GetObject<EntityViewArgument>();
            

            if (request?.ViewName == "LoyaltyCoupons")
            {
                entityView.DisplayName = "Loyalty Coupons";


                var customer =
                    await _findEntity.Run(new FindEntityArgument(typeof(Customer), entityView.EntityId), context) as Customer;

                if (customer == null || !customer.HasComponent<LoyaltySummary>() || customer.GetComponent<LoyaltySummary>().CouponEntities.Count == 0)
                {
                    return entityView;
                }

                IEnumerable<CouponModel> models = await Task.WhenAll(customer.GetComponent<LoyaltySummary>().CouponEntities.Select(id => GetCouponModel(id, context)));


                entityView.ChildViews.AddRange(models);
            }

            return entityView;
        }

        private async Task<CouponModel> GetCouponModel(string id, CommercePipelineExecutionContext context)
        {
            var coupon = (Coupon) await _findEntity.Run(new FindEntityArgument(typeof(Coupon), id), context);
            if (coupon == null)
            {
                return null;
            }

            return new CouponModel
            {
                Code = coupon.Code,
                DateEarned = coupon.DateCreated,
                IsApplied = coupon.UsageCount > 0,  //TODO Determine how system marks a private coupon that has been used.
                DateUsed = coupon.UsageCount > 0 ? coupon.DateUpdated : null //TODO See if there is a more reliable date to use (e.g. a date on an "Applied" policy).
            };
        }
    }
}
