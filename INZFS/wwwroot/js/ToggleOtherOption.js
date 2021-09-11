$(document).ready(function () {

    $("#checkboxSelect-other").click(function () {
        if ($(this).is(":checked")) {
            $(".hide-input").show();
        } else {
            $(".hide-input").hide();
        }
    });

});
