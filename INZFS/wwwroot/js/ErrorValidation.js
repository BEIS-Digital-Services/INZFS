errorSummary = document.querySelector('#validationSummary');
isValid = document.querySelector('.validation-summary-valid');
if (!isValid) {
    errorSummary.classList.add('govuk-error-summary');
}