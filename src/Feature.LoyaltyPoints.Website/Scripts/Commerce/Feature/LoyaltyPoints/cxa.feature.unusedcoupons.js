(function(root, factory) {

    if (typeof define === "function" && define.amd) {
        // use AMD define funtion to support AMD modules if in use
        define("CXA/Feature/LoyaltyPoints/UnusedCoupons", ["exports"], factory);

    } else if (typeof exports === "object") {
        // to support CommonJS
        factory(exports);

    }
    // browser global variable
    root.UnusedCoupons = factory;

}(this,
    function(element, model) {
        "use strict";

        var component = new Component(element, model);

        component.Name = "CXA/Feature/LoyaltyPoints/UnusedCoupons";

        var AddMockData = function(component) {
            component.Model.coupons.push(new CouponDetailModel({ Name: "LPXXXXXX01" }));
            component.Model.coupons.push(new CouponDetailModel({ Name: "LPXXXXXX02" }));
            component.Model.coupons.push(new CouponDetailModel({ Name: "LPXXXXXX03" }));
        };

        component.InExperienceEditorMode = function() {
            AddMockData(component);
            component.Visual.Disable();
        };

        component.Init = function() {
            if (CXAApplication.RunningMode === RunningModes.Normal) {

                // TODO
                //AjaxService.Post("/api/cxa/LoyaltyPonts/GetUnusedCoupons",
                //    function(data, success {
                //        if (success && data && data.Success) {
                //            component.Model.updateModel(data);
                //        }
                //    };
                console.log("Init invoked");
                AddMockData(component);
                return component;
            }
        }
    }
));  