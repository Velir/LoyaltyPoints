using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plugin.LoyaltyPoints.Pipelines.Arguments;
using Plugin.LoyaltyPoints.Pipelines.Interfaces;
using Plugin.LoyaltyPoints.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;


// It's not clear this needs to be a pipeline, since we expose LoyaltyPoint creation.  If another modification was desired to the Sellable item
namespace Plugin.LoyaltyPoints.Pipelines.Blocks
{
    // steps: get price of product (allows customization)
    //        calculate loyalty points
    //        add to product
    //        persist product


//TODO Better name.
    [PipelineDisplayName("LoyaltyPoints.Blocks.AddToProduct")]
    public class AddToProductBlock : PipelineBlock<AddLoyaltyPointsArgument, SellableItem, CommercePipelineExecutionContext>
    {
        private readonly IMakeComponentPipeline _makeComponentPipeline;
        private readonly GetSellableItemCommand _getSellableItemCommand;

        public AddToProductBlock(IMakeComponentPipeline makeComponentPipeline, GetSellableItemCommand getSellableItemCommand)
        {
            _makeComponentPipeline = makeComponentPipeline;
            _getSellableItemCommand = getSellableItemCommand;
        }


        public override async Task<SellableItem> Run(AddLoyaltyPointsArgument arg,
            CommercePipelineExecutionContext context)
        {
            SellableItem sellableItem = null;
            Condition.Requires(arg).IsNotNull($"{this.Name}: The argument can not be null");

            string productId = arg.ProductId;
            if (productId.Contains("|"))
                productId = productId.Replace("|", ",");
            if (!string.IsNullOrEmpty(productId))
            {
                sellableItem = await this._getSellableItemCommand.Process(context.CommerceContext, productId, false);
            }

            if (sellableItem == null)
            {
                context.Logger.LogWarning($"No SellableItem found for ProductId \"{productId}\".");
                return null;
            }

            MakeComponentArgument makeComponentArgument = new MakeComponentArgument
            {
                SellableItem = sellableItem,
                Percent = arg.Percent
            };
            var loyaltyPointsComponent = await _makeComponentPipeline.Run(makeComponentArgument, context);
            if (loyaltyPointsComponent == null)
            {
                context.Logger.LogWarning($"No LoyaltyPointComponent created for ProductId \"{productId}\".");
                return null;
            }

            sellableItem.SetComponent(loyaltyPointsComponent);
            sellableItem.IsPersisted = true; // required for updates
            return sellableItem;
        }

    }
    
}