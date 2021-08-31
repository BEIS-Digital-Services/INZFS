function returnToPreviousPage() {
    history.back(-1)
}

var backBtn = document.getElementById("govuk-back-link");
backBtn.onclick = function () {
    returnToPreviousPage();
}
