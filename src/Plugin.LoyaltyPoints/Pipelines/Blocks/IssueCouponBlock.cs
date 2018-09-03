using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.LoyaltyPoints.Components;
using Plugin.LoyaltyPoints.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Coupons;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Commerce.Plugin.ManagedLists;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines.Blocks
{


    /// <summary>
    /// Issue coupon
    /// </summary>
    class IssueCouponBlock : PipelineBlock<IssueCouponArgument, IssueCouponArgument, CommercePipelineExecutionContext>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IPersistEntityPipeline _persistEntityPipeline;
        private readonly FindEntitiesInListCommand _findEntitiesInListCommand;
        private readonly GetManagedListCommand _getManagedListCommand;
        private readonly RemoveListEntitiesCommand _removeListEntitiesCommand;

        public IssueCouponBlock(
            IServiceProvider serviceProvider,
            IPersistEntityPipeline persistEntityPipeline,
            FindEntitiesInListCommand findEntitiesInListCommand,
            GetManagedListCommand getManagedListCommand,
            RemoveListEntitiesCommand removeListEntitiesCommand)
        {
            _findEntitiesInListCommand = findEntitiesInListCommand;
            _persistEntityPipeline = persistEntityPipeline;
            _serviceProvider = serviceProvider;
            _getManagedListCommand = getManagedListCommand;
            _removeListEntitiesCommand = removeListEntitiesCommand;
        }

        public override async Task<IssueCouponArgument> Run(IssueCouponArgument arg,
            CommercePipelineExecutionContext context)
        {
            var policy = context.GetPolicy<LoyaltyPointsPolicy>();

            var summary = arg.Customer.GetComponent<LoyaltySummary>();

            if (summary.TotalPoints - summary.AppliedPoints < policy.PointsForCoupon)
            {
                return arg;
            }

            ManagedList couponList =
                await _getManagedListCommand.Process(context.CommerceContext, Constants.AvailableCouponsList);
            if (couponList == null)
            {
                return arg;
            }

            var commerceCommand = new CommerceCommand(_serviceProvider);
            await commerceCommand.PerformTransaction(context.CommerceContext,
                async () =>
                {
                    int couponsToIssue = ((summary.TotalPoints - summary.AppliedPoints) / policy.PointsForCoupon);

                    CommerceList<Coupon> list =
                        await _findEntitiesInListCommand.Process<Coupon>(context.CommerceContext,
                            Constants.AvailableCouponsList, 0, couponsToIssue);

                    if (list.Items.Count < couponsToIssue)
                    {
                        context.Abort(
                            $"{Name}: Unable to provision required {couponsToIssue} coupons. Examine List {Constants.AvailableCouponsList} and CreateCouponsMinion process.",
                            context);
                        return;
                    }

                    List<string> couponCodes = list.Items.Select(coupon => coupon.Code).ToList();
                    List<string> entityIds = list.Items.Select(coupon => coupon.Id).ToList();

                    var result = await _removeListEntitiesCommand.Process(context.CommerceContext,
                        Constants.AvailableCouponsList, entityIds);
                    if (!result.Success)
                    {
                        context.Abort($"{Name}: Unable to remove coupon(s) from List {Constants.AvailableCouponsList}",
                            context);
                        return;
                    }

                    summary.AppliedPoints += couponsToIssue * policy.PointsForCoupon;
                    summary.CouponCodes.AddRange(couponCodes);
                    arg.Coupons.AddRange(couponCodes);

                    await _persistEntityPipeline.Run(new PersistEntityArgument(arg.Customer), context);


                });
            return arg;
        }
    }
}