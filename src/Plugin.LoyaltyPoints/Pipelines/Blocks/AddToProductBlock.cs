using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plugin.LoyaltyPoints.Pipelines.Arguments;
using Plugin.LoyaltyPoints.Pipelines.Interfaces;
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
        private readonly IGetSellableItemPipeline _getSellableItemPipeline;
        private readonly IMakeComponentPipeline _makeComponentPipeline;
        private readonly GetSellableItemCommand _getSellableItemCommand;

        public AddToProductBlock(IGetSellableItemPipeline getSellableItemPipeline, IMakeComponentPipeline makeComponentPipeline, GetSellableItemCommand getSellableItemCommand)
        {
            _getSellableItemPipeline = getSellableItemPipeline; 
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
                if (productId.Split(',').Length == 3)
                {
                    string[] strArray = productId.Split(',');
                    ProductArgument argument = new ProductArgument(strArray[0], strArray[1])
                    {
                        VariantId = strArray[2]
                    };
                    //sellableItem = await _getSellableItemPipeline.Run(argument, context);
                    sellableItem = await this._getSellableItemCommand.Process(context.CommerceContext, productId, false);
                }
            }

            if (sellableItem == null)
            {
                context.Logger.LogWarning($"No SellableItem found for ProductId \"{productId}\".");
                return null;

                //TODO Put correct error handling here, basd on Sitecore examples.
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
                return null; //TODO Proper pipeline failure code.
            }

            sellableItem.SetComponent(loyaltyPointsComponent);
            sellableItem.IsPersisted = true; // required for updates
            return sellableItem;
        }

    }
    
}