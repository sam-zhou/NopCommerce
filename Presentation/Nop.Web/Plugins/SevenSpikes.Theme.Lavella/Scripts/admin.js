$(document).ready(function () {

    $(".theme-color .adminData div").click(function () {
        $(".theme-color .adminData div").removeClass("active");
        $(this).addClass("active");
        // check the current radio button with js, because as we hide the radio buttons they are not checked when clicking on them
        var inputs = $(".theme-color .adminData div input");
        $.each(inputs, function (index, item) {
            item.removeAttribute("checked");
        });
        $(this).find('input').attr("checked", "checked");
    });

    $('.radionButton input[checked=checked]').parent().addClass("active");
})