using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.LoyaltyPoints.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines.Blocks
{
    /// <summary>
    /// Ensures existance of available coupon list, checks count.
    /// If necessary, creates new promotion, creates coupon, adds to
    /// list, and  approves promotion.
    ///
    /// Developer notes:
    /// I'm not sure whether allocation is a necessary step for coupons
    /// to be applied, or whether this is just for managing workflow on
    /// screens. I will add that step if necesary.
    ///
    /// I will try adding the coupon to a list by simply adding the list
    /// to the MemberOfLists component, and saving the item. That seens like
    /// a nice way of managing this, if possible.
    /// </summary>
    class CreateCouponsBlock:PipelineBlock<CreateCouponsArgument,CreateCouponsArgument, CommercePipelineExecutionContext>
    {
        public override Task<CreateCouponsArgument> Run(CreateCouponsArgument arg, CommercePipelineExecutionContext context)
        {
            // Does list exist? If not create.
            // Get count.
            // If count is above threashold, exit.
            // If below, for now, just log.
            throw new NotImplementedException();
        }
    }
}
