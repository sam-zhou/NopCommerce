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
        Lynex.log("setJsPay to " + Lynex.isJsPay);
        $(document).trigger({
            type: "wxReady",
            value: Lynex.isJsPay,
            time: new Date()
        });
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


    $(".header-menu").on("swipeleft",function(){
        alert("You swiped left!");
    });
});