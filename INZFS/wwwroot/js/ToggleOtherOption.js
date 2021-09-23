
$(document).ready(function () {
    $(".hide-input").hide();

    $("#checkboxSelect-other").click(function () {
        if ($(this).is(":checked")) {
            $(".hide-input").show();
        } else {
            $(".hide-input").hide();
        }
    });

});
