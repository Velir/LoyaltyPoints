# Velir.Plugin.LoyaltyPoints

*Important*: This is demonstration code, and is not intended for use in a a production environment.

## Developer notes

Current task: Build pipeline to generate coupons

## Pipelines

### Process Customer pipeline

1. Is customer entitled to coupon?
2. Allocate coupon pipeline.
3. Mark LPs applied.
4. Allocate coupon.
5. Mark LPs as applied. 
6. Persist transaction.
7. Send Live Event.


### Allocate coupon pipeline

1. Get current coupon block.
2. Allocate coupon. Return coupon code.
3. If coupon block does not exist, call ProvisionCouponBlock.

### Provision Coupon pipeline

1. Copy template promotion.
2. Get next sequence number from entity, lock entity.
3. Generate coupons.
4. Approve promotion.
5. Update entity: sequence number, current promotion, unlock.

