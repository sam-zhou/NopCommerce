var Lynex = {
    isJsPay: false,
    getJsPay: function() {
        return this.isJsPay;
    },
    setJsPay: function() {
        this.isJsPay = true;
        $(".wechat-only").show();
    }
};


$(document).ready(function () {
    if (typeof WeixinJSBridge == "undefined") {
        if (document.addEventListener) {
            document.addEventListener('WeixinJSBridgeReady', Lynex.setJsPay, false);
        }
        else if (document.attachEvent) {
            document.attachEvent('WeixinJSBridgeReady', Lynex.setJsPay);
            document.attachEvent('onWeixinJSBridgeReady', Lynex.setJsPay);
        }
    }
    else {
        Lynex.setJsPay();
    }
});