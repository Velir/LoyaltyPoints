// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureSitecore.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Plugin.LoyaltyPoints.Pipelines;
using Plugin.LoyaltyPoints.Pipelines.Blocks;
using Plugin.LoyaltyPoints.Pipelines.Interfaces;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Configuration;
using Sitecore.Framework.Pipelines.Definitions.Extensions;
using Sitecore.Framework.Pipelines.Definitions.Modifier;

namespace Plugin.LoyaltyPoints
{
    /// <summary>
    /// The configure sitecore class.
    /// </summary>
    public class ConfigureSitecore : IConfigureSitecore
    {
        /// <summary>
        /// The configure services.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore().Pipelines(config => config

                .AddPipeline<IAddToProductPipeline, AddToProductPipeline>(
                    configure =>
                    {
                        configure
                            .Add<AddToProductBlock>()
                            .Add<PersistSellableItemBlock>();
                    })

                .AddPipeline<IGetBasisPricePipeline, GetBasisPricePipeline>(
                    configure => { configure.Add<GetListPriceAmountBlock>(); })

                .AddPipeline<IMakeComponentPipeline, MakeComponentPipeline>(
                    configure => { configure.Add<MakeComponentBlock>(); })

                .AddPipeline<IAllocateCouponPipeline, AllocateCouponPipeline>(
                    configure => { configure.Add<AllocateCouponBlock>(); })

                .AddPipeline<ICreateCouponsPipeline, CreateCouponsPipeline>(
                    configure => { configure.Add<CreateCouponsBlock>(); })

                .AddPipeline<IProcessCustomerPipeline, ProcessCustomerPipeline>(
                    configure => { configure.Add<ProcessCustomerBlock>(); })

                .AddPipeline<IProcessOrderPipeline, ProcessOrderPipeline>(
                    configure => { configure.Add<ProcessOrderBlock>(); })

                .AddPipeline<IIssueCouponPipeline, IssueCouponPipeline>(
                    configure => { configure.Add<IssueCouponBlock>(); })

                //TODO Confirm this is not used and remove, together wiht classes.
                .AddPipeline<IApplyLoyaltyPointsMinionPipeline,ApplyLoyaltyPointsMinionPipeline>(
                    configure =>
                    {
                        configure.Add<GetCustomerOrdersBlock>();
                    })

                .ConfigurePipeline<IAddCartLinePipeline>(
                    configure => configure
                        .Add<AddCartLineLoyaltyBlock>()
                        .After<AddCartLineBlock>())

                .ConfigurePipeline<IGetEntityViewPipeline>(
                    configure => configure
                        .Add<GetSellableItemLoyaltyPointsViewBlock>()
                        .After<GetSellableItemEditBlock>())
                                               


                .ConfigurePipeline<IConfigureServiceApiPipeline>(
                    configure => configure.Add<ConfigureServiceApiBlock>()));
            

                

            services.RegisterAllCommands(assembly);
        }
    }
}