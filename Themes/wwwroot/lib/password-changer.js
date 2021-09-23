/// <reference path="../../../inzfs/wwwroot/lib/jquery/dist/jquery.min.js" />

$(document).ready(function() {
    $(".password-changer").click(function(event) {
        var controlName = '#' + $(this).data('control');
        var type = $(controlName).attr('type');
        if (type === "password") {
            $(controlName).attr('type', "text");
            $(this).html("Hide");
        } else {
            $(controlName).attr('type', "password");
            $(this).html("Show");
        }
    })
});

