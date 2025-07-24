'use strict';

var app = app || {};

(function() {

    app.ui = app.ui || {};

    if (!FreezeUI || !UnFreezeUI) {
        return;
    }

    app.ui.setBusy = function (elm, text, delay) {
        FreezeUI({
            element: elm,
            text: text ? text : ' ',
            delay: delay,
        });
    };

    app.ui.clearBusy = function (elm, delay) {
        UnFreezeUI({
            element: elm,
            delay: delay,
        });
    };

})();