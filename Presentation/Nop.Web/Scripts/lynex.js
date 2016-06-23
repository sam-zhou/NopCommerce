var Lynex = {
    isJsPay: false,
    isDebug: true,
    log : function(msg) {
        if (Lynex.isDebug && console) {
            console.log(msg);
        }
    },
    getJsPay: function () {
        return Lynex.isJsPay;
    },
    setJsPay: function () {
        Lynex.isJsPay = true;
        $(document).trigger({
            type: "wxReady",
            message: Lynex.isJsPay,
            time: new Date()
        });
        $(".wechat-only").show();
        Lynex.log("setJsPay to " + Lynex.isJsPay);
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