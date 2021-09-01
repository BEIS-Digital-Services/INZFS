var passwordBtn1 = document.getElementById("buttonText");
passwordBtn1.onclick = function () {
    currentPassword()
}
var passwordBtn2 = document.getElementById("buttonText1");
passwordBtn2.onclick = function () {
    createPassword()
}
var passwordBtn3 = document.getElementById("buttonText2");
passwordBtn3.onclick = function () {
    confirmPassword() 
}
function currentPassword() {
        var x = document.getElementById("password-current");
        if (x.type === "password") {
            x.type = "text";
            document.getElementById("buttonText").innerHTML = "Hide";
        } else {
            x.type = "password";
            document.getElementById("buttonText").innerHTML = "Show";
        }
    }

    function createPassword() {
        var x = document.getElementById("password-create");
        if (x.type === "password") {
            x.type = "text";
            document.getElementById("buttonText1").innerHTML = "Hide";
        } else {
            x.type = "password";
            document.getElementById("buttonText1").innerHTML = "Show";
        }
    }

    function confirmPassword() {
        var x = document.getElementById("password-confirm");
        if (x.type === "password") {
            x.type = "text";
            document.getElementById("buttonText2").innerHTML = "Hide";
        } else {
            x.type = "password";
            document.getElementById("buttonText2").innerHTML = "Show";
        }
    }