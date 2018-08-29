// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SampleController.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plugin.LoyaltyPoints.Commands;
using Plugin.LoyaltyPoints.Components;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;

namespace Plugin.LoyaltyPoints.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Defines a controller
    /// </summary>
    /// <seealso cref="T:Sitecore.Commerce.Core.CommerceController" />
    [Microsoft.AspNetCore.OData.EnableQuery]
    [Route("api/Sample")]
    public class SampleController : CommerceController
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CommerceEnvironment _globalEnvironment;
        private readonly GetSellableItemCommand _getSellableItemCommand;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Plugin.LoyaltyPoints.Controllers.SampleController" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="globalEnvironment">The global environment.</param>
        public SampleController(IServiceProvider serviceProvider, CommerceEnvironment globalEnvironment, GetSellableItemCommand getSellableItemCommand) : base(serviceProvider, globalEnvironment)
        {
            _serviceProvider = serviceProvider;
            _globalEnvironment = globalEnvironment;
            _getSellableItemCommand = getSellableItemCommand;
        }

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>A <see cref="IActionResult"/></returns>
        [HttpGet]
        [Route("(Id={id})")]
        [Microsoft.AspNetCore.OData.EnableQuery]
        public async Task<IActionResult> Get(string id)
        {
            SellableItem sellableItem = await _getSellableItemCommand.Process(this.CurrentContext, id, true);
            sellableItem.SetComponent(new LoyaltyPointsComponent());
            return new ObjectResult(sellableItem);
        }
    }
}
