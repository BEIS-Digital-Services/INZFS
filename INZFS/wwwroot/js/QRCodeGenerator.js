new QRCode(document.getElementById("qrCode"),
    {
        text: "@Html.Raw(Model.AuthenticatorUri)",
        width: 150,
        height: 150
    });