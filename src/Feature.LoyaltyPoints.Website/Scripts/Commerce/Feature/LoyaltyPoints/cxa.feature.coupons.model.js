// Patterned after \Scripts\Commerce\Feature\Account\cxa.feature.orders.model.js

function CouponsViewModel() {
    var self = this;

    self.coupons = ko.observableArray();

    self.updateModel = function (data) {
        $(data.Coupons).each(function () {
            self.coupons.push(new CouponDetailModel(this));
        });
    };
}

function CouponDetailModel(data) {
    var self = this;

    self.code = ko.observable(data.Code);
}