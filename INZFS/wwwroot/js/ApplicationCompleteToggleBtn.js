function applicationCompletedCheck() {
    var button = document.getElementById('SaveAndContinue');
    button.disabled = false;
}

var EQSurveryYes = document.getElementById('yes-equality');
EQSurveryYes.onclick = function () {
    applicationCompletedCheck();
}

var EQSurveryNo = document.getElementById('no-equality');
EQSurveryNo.onclick = function () {
    applicationCompletedCheck();
}