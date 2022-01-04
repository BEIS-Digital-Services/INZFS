﻿


$(document).ready(function () {

    var element = document.getElementById("qrCode");
    new QRCode(element,
        {
            text: element.getAttribute("code"),
            width: 150,
            height: 150
        });

    $("img", $("#qrCode")).attr("alt", "QR Code");

});
