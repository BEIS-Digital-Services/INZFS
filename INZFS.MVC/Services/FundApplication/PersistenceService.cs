using INZFS.MVC.Models.DynamicForm;
using INZFS.MVC.Services.FileUpload;
using INZFS.MVC.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Services.FundApplication
{
    public class PersistenceService : IPersistenceService
    {
        private readonly ApplicationDefinition _applicationDefinition;
        private readonly IFileUploadService _fileUploadService;
        private readonly ApplicationOption _applicationOption;
        private readonly ModelStateDictionary _modelState;
        private readonly IDynamicFormGenerator _dynamicFormGenerator;
        private readonly IApplicationGeneratorService _applicationGeneratorService;

        public PersistenceService(
            ApplicationDefinition applicationDefinition,
            IFileUploadService fileUploadService, 
            ApplicationOption applicationOption,
            ModelStateDictionary modelState,
            IDynamicFormGenerator dynamicFormGenerator,
            IApplicationGeneratorService applicationGeneratorService
            )
        {
            _applicationDefinition = applicationDefinition;
            _fileUploadService = fileUploadService;
            _applicationOption = applicationOption;
            _modelState = modelState;
            _dynamicFormGenerator = dynamicFormGenerator;
            _applicationGeneratorService = applicationGeneratorService;
        }
        public Page GetCurrentPage(string pageName)
        {
            var currentPage = _applicationDefinition.Application.AllPages.FirstOrDefault(p => p.Name.ToLower().Equals(pageName));
            return currentPage;
        }

        public bool CheckFieldTypeIsFileUpload(string pageName)
        {
            var currentPage = GetCurrentPage(pageName);
            if(currentPage.FieldType == FieldType.gdsFileUpload)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void CheckValidityOfFile(IFormFile file, Page currentPage)
        {
            _modelState.Clear();
            var errorMessage = _fileUploadService.Validate(file, currentPage, _applicationOption.VirusScanningEnabled, _applicationOption.CloudmersiveApiKey);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                _modelState.AddModelError("DataInput", errorMessage);
            }
        }
        public void CheckFieldStatus(string pageName, string submitAction, BaseModel model, string userId)
        {
            var currentPage = GetCurrentPage(pageName);
            var fieldStatus =_dynamicFormGenerator.GetFieldStatus(currentPage, model);
            var contentToSave = _applicationGeneratorService.GetApplicationConent(userId).Result;
            var existingData = contentToSave.Fields.FirstOrDefault(f => f.Name.Equals(currentPage.FieldName));

            if (fieldStatus == FieldStatus.Completed)
            {
                if (submitAction != "DeleteFile" && string.IsNullOrEmpty(existingData?.AdditionalInformation))
                {
                    if (currentPage.Mandatory)
                    {
                        _modelState.AddModelError("DataInput", "This section has a mandatory file upload. You must first choose your file and then upload it using the upload button below.");
                    }
                    else
                    {
                        _modelState.AddModelError("DataInput", "You have not uploaded any evidence. Please upload a file before marking as complete. If this step is not relevant to your application please select 'not applicable'.");
                    }
                }
            }
            if (contentToSave != null)
            {

                if (fieldStatus == FieldStatus.NotApplicable)
                {
                    if (submitAction != "DeleteFile" && !string.IsNullOrEmpty(existingData?.AdditionalInformation))
                    {
                        _modelState.AddModelError("DataInput", "You have told us this step is not applicable, your evidence will not be added to your proposal. Please select 'mark this step as complete' if you would like your evidence to be reviewed. Otherwise, please remove the file.");
                        ReturnViewModel(currentPage, model, existingData);
                    }
                }
            }
        }
    
        public IActionResult ReturnViewModel(Page currentPage, BaseModel model, Field existingData)
        {
            return _dynamicFormGenerator.PopulateViewModel(currentPage, model, existingData);
        }
    }
}
