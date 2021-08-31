function checkingRadio() {
    var button = document.getElementById('continueBtn');
        button.disabled = false;
}

var checkYes = document.getElementById('checkYes');
checkYes.onclick = function () {
    checkingRadio();
}
var checkNo = document.getElementById('checkNo');
checkNo.onclick = function () {
    checkingRadio();
}

