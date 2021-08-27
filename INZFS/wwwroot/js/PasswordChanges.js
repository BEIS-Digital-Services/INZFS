function createPassword() {
    var x = document.getElementById("Password");
    if (x.type === "password") {
        x.type = "text";
        document.getElementById("buttonText").innerHTML = "Hide";
    } else {
        x.type = "password";
        document.getElementById("buttonText").innerHTML = "Show";
    }
}

function confirmPassword() {
    var x = document.getElementById("ConfirmPassword");
    if (x.type === "password") {
        x.type = "text";
        document.getElementById("buttonText1").innerHTML = "Hide";
    } else {
        x.type = "password";
        document.getElementById("buttonText1").innerHTML = "Show";
    }
}