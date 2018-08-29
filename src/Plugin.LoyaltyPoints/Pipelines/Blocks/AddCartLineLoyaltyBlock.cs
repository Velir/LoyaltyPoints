using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines.Blocks
{
    class AddCartLineLoyaltyBlock: PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {
        public override Task<Cart> Run(Cart cart, CommercePipelineExecutionContext context)
        {
            Condition.Requires(cart).IsNotNull($"{this.Name}: {nameof(cart)} cannot be null.");

            var sellableItem = context.CommerceContext.GetObjects<SellableItem>().First();
            var arg = context.CommerceContext.GetObjects<CartLineArgument>().First();
            var cartLine = arg.Line;
            var lp = sellableItem.GetComponent<Components.LoyaltyPointsComponent>();
            if (lp != null)
            {
                cartLine.SetComponent(lp);
            }
            return Task.FromResult(cart);
        }
    }
}
