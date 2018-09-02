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
    class CreateCouponsBlock:PipelineBlock<CreateCouponsArgument,CreateCouponsArgument, CommercePipelineExecutionContext>
    {
        public override Task<CreateCouponsArgument> Run(CreateCouponsArgument arg, CommercePipelineExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
