# Velir.Plugin.LoyaltyPoints

*Important*: This is demonstration code, and is not intended for use in a a production environment. 

## Functionality Highlights

* API to create loyalty points
* Addition of loyatly points to product view
* Minions to manage coupon creation, customer point totals, and coupon awards
* xConnect event registration
* Unused Coupons report on storefront

## Design Highlights

* Pipelines for component creation can be replaced:
    * Replace MakeComponentBlock to add your own component type
    * Replace GetListPriceAmountBlock to use different logic to obtain basis price
* Policy can be replaced by subclass to modify ValidateOrder logic
* Minions use transactions to ensure coupons, customers, and orders are in a consistent state
* Helper methods to register and detect errors.  Illustrates how to register errors to trigger transactions rollback.
* Commerce Excelerator rendering to display coupons to user. Follows CXA patterns, illustrates how to transmit data from engine to storefront using EntityViews.

## Installation instructions

* Build on top of Sitecore Experience Commerce 9.01. 
* Add Plugin as dependency of Website project in SDK.
* Run engine from SDK as described in Sitecore Commerce Developer Guide. 
* Deploy Website project to storefront.
* IN PROGRESS Load package to add renderings and goals.
* In Commerce Promotions screen, create a promotion group called "X", and a promtion callsed "Y" with desired benefit. Attach promoton book to Habitat catalog.
    * Note: The name of this promotion can be changed via the policy. TODO
* Set the following values in the Loyalty Points policy: TODO

