using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Routing;
using Microsoft.AspNetCore.Authorization;
using INZFS.MVC.Models;
using Microsoft.AspNetCore.Http;
using OrchardCore.Media;
using INZFS.MVC.Models.DynamicForm;
using INZFS.MVC.Services.FileUpload;
using System.Text.Json;
using ClosedXML.Excel;
using System.IO;
using ClosedXML.Excel.CalcEngine.Exceptions;
using System.Globalization;
using INZFS.MVC.Services;
using System.Security.Claims;
using INZFS.MVC.Extensions;
using INZFS.MVC.Constants;
using INZFS.MVC.Filters;
using INZFS.MVC.Settings;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using Ganss.XSS;
using INZFS.MVC.Services.Zip;
using INZFS.MVC.Services.FundApplication;
using INZFS.MVC.Services.UserManager;
using INZFS.MVC.Services.AzureStorage;

namespace INZFS.MVC.Controllers
{
    [Authorize]
    public class FundApplicationController : Controller
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IMediaFileStore _mediaFileStore;
        private readonly YesSql.ISession _session;
        private readonly IContentRepository _contentRepository;
        private readonly ApplicationDefinition _applicationDefinition;
        private readonly IApplicationEmailService _applicationEmailService;
        private readonly ApplicationOption _applicationOption;
        private readonly IZipService _zipService;
        private readonly IWebHostEnvironment _env;
        private readonly IApplicationGeneratorService _applicationGeneratorService;
        private readonly IUserManagerService _userManagerService;
        private readonly IAzureBlobService _azureBlobService;
        private readonly IDynamicFormGenerator _dynamicFormGenerator;
        private HtmlSanitizer _sanitizer;
        

        public FundApplicationController(
            IMediaFileStore mediaFileStore, 
            IHtmlLocalizer<FundApplicationController> htmlLocalizer,
            YesSql.ISession session,
            IContentRepository contentRepository, IFileUploadService fileUploadService, 
            ApplicationDefinition applicationDefinition, 
            IApplicationEmailService applicationEmailService,
            IOptions<ApplicationOption> applicationOption,
            IZipService zipService,
            IWebHostEnvironment env,
            IApplicationGeneratorService applicationGeneratorService,
            IUserManagerService userManagerService,
            IAzureBlobService azureBlobService,
            IDynamicFormGenerator dynamicFormGenerator)
        {
            _mediaFileStore = mediaFileStore;
            _session = session;
            _contentRepository = contentRepository;
            _fileUploadService = fileUploadService;
            _applicationDefinition = applicationDefinition;
            _applicationEmailService = applicationEmailService;
            _applicationOption = applicationOption.Value;
            _zipService = zipService;
            _env = env;
            _applicationGeneratorService = applicationGeneratorService;
            _userManagerService = userManagerService;
            _azureBlobService = azureBlobService;
            _dynamicFormGenerator = dynamicFormGenerator;

            _sanitizer = new HtmlSanitizer();
            _sanitizer.AllowedAttributes.Clear();
            _sanitizer.AllowedTags.Clear();
            _sanitizer.AllowedCssProperties.Clear();
        }

        [HttpGet]
        [ServiceFilter(typeof(ApplicationRedirectionAttribute))]
        public async Task<IActionResult> Section(string pagename, string id)
        {
            return await _applicationGeneratorService.GetSection(pagename, id);
        }

       

        public async Task<bool> CreateDirectory(string directoryName)
        {
            if(directoryName == null)
            {
                return false;
            }
            await _mediaFileStore.TryCreateDirectoryAsync(directoryName);
            return true;
        }

        [Route("FundApplication/section/{pageName}")]
        [HttpPost, ActionName("save")]
        [FormValueRequired("submit.Publish")]
        [ServiceFilter(typeof(ApplicationRedirectionAttribute))]
        public async Task<IActionResult> Save([Bind(Prefix = "submit.Publish")] string submitAction, string returnUrl, string pageName, IFormFile? file, BaseModel model)
        {
            var currentPage = _applicationDefinition.Application.AllPages.FirstOrDefault(p => p.Name.ToLower().Equals(pageName));
            if (currentPage.FieldType == FieldType.gdsFileUpload)
            {
                if (file != null || submitAction == "UploadFile")
                {
                    ModelState.Clear();
                    var errorMessage =  _fileUploadService.Validate(file, currentPage, _applicationOption.VirusScanningEnabled, _applicationOption.CloudmersiveApiKey);
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        ModelState.AddModelError("DataInput", errorMessage);
                    }
                }

                if(ModelState.IsValid && submitAction != "UploadFile")
                {
                    var fieldStatus = GetFieldStatus(currentPage, model);
                    var userId = _userManagerService.GetUserId();
                    var contentToSave = _applicationGeneratorService.GetApplicationConent(userId).Result;
                    if (contentToSave != null)
                    {
                        var existingData = contentToSave.Fields.FirstOrDefault(f => f.Name.Equals(currentPage.FieldName));
                        if (fieldStatus == FieldStatus.Completed)
                        {
                            if (submitAction != "DeleteFile" && string.IsNullOrEmpty(existingData?.AdditionalInformation))
                            {
                                if(currentPage.Mandatory)
                                {
                                    ModelState.AddModelError("DataInput", "This section has a mandatory file upload. You must first choose your file and then upload it using the upload button below.");
                                }
                                else
                                {
                                    ModelState.AddModelError("DataInput", "You have not uploaded any evidence. Please upload a file before marking as complete. If this step is not relevant to your application please select 'not applicable'.");
                                }
                            }
                        }
                        if (fieldStatus == FieldStatus.NotApplicable)
                        {
                            if (submitAction != "DeleteFile" && !string.IsNullOrEmpty(existingData?.AdditionalInformation))
                            {
                                ModelState.AddModelError("DataInput", "You have told us this step is not applicable, your evidence will not be added to your proposal. Please select 'mark this step as complete' if you would like your evidence to be reviewed. Otherwise, please remove the file.");
                                return _dynamicFormGenerator.PopulateViewModel(currentPage, model, existingData);
                            }
                        }
                    }
                }
            }
            if (ModelState.IsValid || submitAction == "DeleteFile")
            {
                var userId = _userManagerService.GetUserId();
                var contentToSave = _applicationGeneratorService.GetApplicationConent(userId).Result;

                if (contentToSave == null)
                {
                    contentToSave = new ApplicationContent();
                    contentToSave.Application = new Application();
                    contentToSave.UserId = userId;
                    contentToSave.CreatedUtc = DateTime.UtcNow;
                }

                contentToSave.ModifiedUtc = DateTime.UtcNow;

                
                string publicUrl = string.Empty;
                var additionalInformation = string.Empty;
                if (currentPage.FieldType == FieldType.gdsFileUpload)
                {
                    if (file != null || submitAction.ToLower() == "UploadFile".ToLower())
                    {
                        var directoryName = userId;
                        try
                        {
                            publicUrl = await _fileUploadService.SaveFile(file, directoryName);
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("DataInput", "The selected file could not be uploaded - try again");
                        }
                        
                        model.DataInput = file.FileName;

                        var uploadedFile = new UploadedFile()
                        {
                            FileLocation = publicUrl,
                            Name = file.FileName,
                            Size = (file.Length / (double)Math.Pow(1024, 2)).ToString("0.00")
                        };

                        model.UploadedFile = uploadedFile;

                        if (file.FileName.ToLower().Contains(".xlsx") && currentPage.Name == "project-cost-breakdown")
                        {
                            try
                            {
                                XLWorkbook wb = new(file.OpenReadStream());

                                try
                                {
                                    IXLWorksheet ws = wb.Worksheet("A. Summary");
                                    IXLCell totalGrantFunding = ws.Search("Total BEIS grant applied for").FirstOrDefault();
                                    IXLCell totalMatchFunding = ws.Search("Total match funding contribution").FirstOrDefault(cell => cell.GetString() == "Total match funding contribution");
                                    IXLCell totalProjectFunding = ws.Search("Total project costs").FirstOrDefault(cell => cell.GetString() == "Total project costs");

                                    bool spreadsheetValid = totalGrantFunding != null && totalMatchFunding != null && totalProjectFunding != null;

                                    if (spreadsheetValid)
                                    {
                                        try
                                        {
                                            ParsedExcelData parsedExcelData = new();
                                            var parsedTotalProjectCost = Double.Parse(totalProjectFunding.CellRight().CachedValue.ToString());
                                            var parsedTotalGrantFunding = Double.Parse(totalGrantFunding.CellRight().CachedValue.ToString());
                                            var parsedTotalMatchFunding = Double.Parse(totalMatchFunding.CellRight().CachedValue.ToString());
                                            if (parsedTotalProjectCost > 0D)
                                            {
                                                parsedExcelData.ParsedTotalProjectCost = parsedTotalProjectCost;
                                                contentToSave.TotalProjectCost = parsedTotalProjectCost;
                                            }
                                            else
                                            {
                                                return AddErrorAndPopulateViewModel(currentPage, model, "DataInput",
                                                            "Total project costs should be more than zero.");
                                            }
                                            parsedExcelData.ParsedTotalGrantFunding = parsedTotalGrantFunding;
                                            contentToSave.TotalGrantFunding = parsedTotalGrantFunding;

                                            parsedExcelData.ParsedTotalMatchFunding = parsedTotalMatchFunding;
                                            contentToSave.TotalMatchFunding = parsedTotalMatchFunding;

                                            uploadedFile.ParsedExcelData = parsedExcelData;
                                        }
                                        catch (DivisionByZeroException e)
                                        {
                                            return AddErrorAndPopulateViewModel(currentPage, model, "DataInput",
                                                "Uploaded spreadsheet is incomplete. Complete all mandatory information within the template.");
                                        }
                                        catch (FormatException e)
                                        {
                                            return AddErrorAndPopulateViewModel(currentPage, model, "DataInput", 
                                                "Uploaded spreadsheet is incomplete. Complete all mandatory information within the template.");
                                        }
                                    }
                                    else
                                    {
                                        return AddErrorAndPopulateViewModel(currentPage, model, "DataInput", "Uploaded spreadsheet does not match the template. Use the provided template.");
                                    }
                                }
                                catch (ArgumentException e)
                                {
                                    return AddErrorAndPopulateViewModel(currentPage, model, "DataInput", "Uploaded spreadsheet does not match the template. Use the provided template.");
                                }
                                catch (InvalidOperationException e)
                                {
                                    return AddErrorAndPopulateViewModel(currentPage, model, "DataInput", "Uploaded spreadsheet does not match the template. Use the provided template.");
                                }
                            }
                            catch (InvalidDataException e)
                            {
                                return AddErrorAndPopulateViewModel(currentPage, model, "DataInput", "Invalid file uploaded");
                            }
                            catch (Exception ex)
                            {
                                return AddErrorAndPopulateViewModel(currentPage, model, "DataInput", "Invalid file uploaded - try again.");
                            }
                        }

                        additionalInformation = JsonSerializer.Serialize(uploadedFile);
                    }
                    else
                    {
                        var existingData = contentToSave.Fields.FirstOrDefault(f => f.Name.Equals(currentPage.FieldName));
                        //TODO - Handle validation Error
                        if (submitAction != "DeleteFile" && string.IsNullOrEmpty(existingData?.AdditionalInformation))
                        {
                           // ModelState.AddModelError("DataInput", "No file was uploaded.");
                        }
                        else
                        {
                            additionalInformation = existingData.AdditionalInformation;
                        }

                    }
                }

                var existingFieldData = contentToSave.Fields.FirstOrDefault(f => f.Name.Equals(currentPage.FieldName));
                if(existingFieldData == null)
                {
                    contentToSave.Fields.Add(new Field {
                        Name = currentPage.FieldName,
                        Data = model.GetData(),
                        OtherOption = _sanitizer.Sanitize(model.GetOtherSelected()),
                        MarkAsComplete = model.ShowMarkAsComplete ? model.MarkAsComplete : null,
                        AdditionalInformation = currentPage.FieldType == FieldType.gdsFileUpload ? additionalInformation : null,
                        FieldStatus = GetFieldStatus(currentPage, model)
                    });
                }
                else
                {
                    if (currentPage.FieldType == FieldType.gdsFileUpload)
                    {
                        // TODO Delete  the old file

                        bool fileHasChanged = additionalInformation != existingFieldData?.AdditionalInformation;
                        if (!string.IsNullOrEmpty(existingFieldData?.AdditionalInformation))
                        {
                            model.UploadedFile = JsonSerializer.Deserialize<UploadedFile>(existingFieldData.AdditionalInformation);
                        }
                        if ((fileHasChanged && !string.IsNullOrEmpty(existingFieldData?.AdditionalInformation)) || submitAction == "DeleteFile")
                        {
                            var uploadedFile = JsonSerializer.Deserialize<UploadedFile>(existingFieldData.AdditionalInformation);
                            model.UploadedFile = uploadedFile;
                            var deleteSucessful = await _fileUploadService.DeleteFile(uploadedFile.FileLocation);
                            if(submitAction == "DeleteFile")
                            {
                                additionalInformation = null;
                                model.UploadedFile = null;
                            }

                        }

                    }
                    existingFieldData.Data = model.GetData();
                    existingFieldData.FieldStatus = GetFieldStatus(currentPage, model);
                    if (!string.IsNullOrEmpty(existingFieldData.Data) && existingFieldData.Data.Contains("Other"))
                    {
                        existingFieldData.OtherOption = _sanitizer.Sanitize(model.GetOtherSelected());
                    }
                    else
                    {
                        existingFieldData.OtherOption = null;
                    }
                    existingFieldData.MarkAsComplete = model.ShowMarkAsComplete ? model.MarkAsComplete : null;
                    existingFieldData.AdditionalInformation = currentPage.FieldType == FieldType.gdsFileUpload ? additionalInformation : null;
                }

                //Delete the data from dependants
                var datafieldForCurrentPage = contentToSave.Fields.FirstOrDefault(f => f.Name == currentPage.FieldName);
                //Get all pages that depends on the current field and its value
                var dependantPages = _applicationDefinition.Application.AllPages.Where(page => page.DependsOn?.FieldName == currentPage.FieldName);

                foreach (var dependantPage in dependantPages)
                {
                    if(dependantPage.DependsOn.Value != datafieldForCurrentPage.Data)
                    {
                        contentToSave.Fields.RemoveAll(field => field.Name == dependantPage.FieldName);
                    }
                }

                //Sanitize any field where a user can possibly input a string to prevent XSS attempts being saved to the DB 
                if(currentPage.FieldType == FieldType.gdsAddressTextBox || 
                    currentPage.FieldType == FieldType.gdsTextArea || 
                    currentPage.FieldType == FieldType.gdsTextBox || 
                    currentPage.FieldType == FieldType.gdsMultiLineRadio || 
                    currentPage.FieldType == FieldType.gdsSingleRadioSelectOption)
                {
                    datafieldForCurrentPage.Data = _sanitizer.Sanitize(datafieldForCurrentPage.Data);
                }

                _session.Save(contentToSave);
                await _session.SaveChangesAsync();

                if (currentPage != null && currentPage.Actions != null && currentPage.Actions.Count > 0)
                {
                    var action = currentPage.Actions.FirstOrDefault(a => a.Value.ToLower().Equals(model.GetData()));
                    // action logic based on value
                    return RedirectToAction("section", new { pagename = action.PageName });
                }

                if (submitAction == "DeleteFile" || submitAction == "SaveProgress" || submitAction == "UploadFile")
                {
                    return RedirectToAction("section", new { pagename = pageName });
                }

                if (currentPage.NextPageName != null)
                {
                    return RedirectToAction("section", new { pagename = currentPage.NextPageName });
                }

                //TODO - replace all the references to AllPages with section.Pages
                var index = _applicationDefinition.Application.AllPages.FindIndex(p => p.Name.ToLower().Equals(pageName));
                var currentSection = _applicationDefinition.Application.Sections.Where(s => s.Pages.Any(c => c.Name == pageName.ToLower())).FirstOrDefault();

                //Dependant pages
                Page nextPage = null;
                while(true)
                {
                    nextPage = _applicationDefinition.Application.AllPages.ElementAtOrDefault(index + 1);
                    var dependsOn = nextPage?.DependsOn;
                    if (dependsOn == null)
                    {
                        break;
                    }

                    var dependantPageField = contentToSave.Fields.FirstOrDefault(field => field.Name.ToLower().Equals(dependsOn.FieldName));
                    
                    //TODO This will NOT work for all page types for now
                    if(dependantPageField.Data == dependsOn.Value)
                    {
                        break;
                    }

                    index++;
                }

                
                if (nextPage == null)
                {
                    //If there is no other page, then redirect back to the section page
                    return RedirectToAction("section", new { pagename = currentSection.ReturnUrl ?? currentSection.Url }); ;
                }

                
                var inSection = currentSection.Pages.Contains(nextPage);
                if (!inSection)
                {
                    // If next page exists, but it is optional or not applicable ( depending on the answer to the previous question),
                    // the also redirect back to section
                    return RedirectToAction("section", new { pagename = currentSection.ReturnUrl ?? currentSection.Url });
                     
                }
                
                //TODO: Check of non-existing pages
                // check for the last page
                return RedirectToAction("section", new { pagename = nextPage.Name });
            }
            else
            {
                await _session.CancelAsync();
                currentPage = _applicationDefinition.Application.AllPages.FirstOrDefault(p => p.Name.ToLower().Equals(pageName));
                return _dynamicFormGenerator.PopulateViewModel(currentPage, model);
            }
        }

        private ViewResult AddErrorAndPopulateViewModel(Page currentPage, BaseModel currentModel,string fieldName, string errorMessage)
        {
            ModelState.AddModelError(fieldName, errorMessage);
            return _dynamicFormGenerator.PopulateViewModel(currentPage, currentModel);
        }


        public async Task<IActionResult> Submit()
        {
            SetPageTitle("Submit application");
            var userId = _userManagerService.GetUserId();
            var content = _applicationGeneratorService.GetApplicationConent(userId).Result;
            if(content.ApplicationStatus != ApplicationStatus.InProgress)
            {
                return RedirectToAction("ApplicationSent");
            }
            var applicationOverviewContentModel = _applicationGeneratorService.GetApplicationOverviewContent(content);
            if(applicationOverviewContentModel.TotalSections == applicationOverviewContentModel.TotalSectionsCompleted 
                                    && DateTime.UtcNow <= _applicationOption.EndDate)
            {
                bool applicationUploadSuccess = await _azureBlobService.AddApplicationToBlobStorage();                if(!applicationUploadSuccess)
                {
                    TempData[TempDataKeys.ApplicationOverviewError] = true;
                    return RedirectToAction("section", new { pagename = "application-overview" });
                }

                TempData.Remove(TempDataKeys.ApplicationOverviewError);
                var model = new CommonModel
                {
                    ShowBackLink = true,
                    BackLinkText = "Back",
                    BackLinkUrl = Url.ActionLink("section", "FundController", new { pagename = "application-overview" })
                };
                return View("ApplicationSubmit", model);
            }
            else
            {
                TempData[TempDataKeys.ApplicationOverviewError] = true;
                return RedirectToAction("section", new { pagename = "application-overview" });
            }
        }

        [ServiceFilter(typeof(ApplicationRedirectionAttribute))]
        public async Task<IActionResult> Complete()
        {
            SetPageTitle("Application completed");
            var userId = _userManagerService.GetUserId();
            var content = _applicationGeneratorService.GetApplicationConent(userId).Result;
            if (content.ApplicationStatus != ApplicationStatus.InProgress)
            {
                return RedirectToAction("ApplicationSent");
            }
            var applicationOverviewContentModel = _applicationGeneratorService.GetApplicationOverviewContent(content);
            if (applicationOverviewContentModel.TotalSections == applicationOverviewContentModel.TotalSectionsCompleted)
            {
                await _contentRepository.UpdateStatus(userId, ApplicationStatus.Submitted);
                var model = new CommonModel { 
                    ApplicationNumber = content.ApplicationNumber,
                    ShowBackLink = true,
                    BackLinkText = "Back",
                    BackLinkUrl = Url.ActionLink("Submit", "FundController")
                };

                var url = Url.Action("ApplicationSent", "FundApplication",null, Request.Scheme);
                await _applicationEmailService.SendConfirmationEmailAsync(User, applicationOverviewContentModel.ApplicationNumber, url);
                return View("ApplicationComplete", model);
            }
            else
            {
                return RedirectToAction("section", new { pagename = "application-overview" });
            }
        }

        [ServiceFilter(typeof(ApplicationRedirectionAttribute))]
        public async Task<IActionResult> ApplicationEquality()
        {
            SetPageTitle("Equality questions");
            var model = new CommonModel
            {
                ShowBackLink = true,
                BackLinkText = "Back to application overview",
                BackLinkUrl = Url.ActionLink("section", "FundController", new { pagename = "application-overview" })
            };
            return View("ApplicationEquality", model);
        }

        public async Task<IActionResult> ApplicationSent()
        {
            SetPageTitle("Your application");
            var userId = _userManagerService.GetUserId();
            var content = await _contentRepository.GetApplicationContent(userId);
            var status = content.ApplicationStatus == ApplicationStatus.InProgress ? ApplicationStatus.NotSubmitted : content.ApplicationStatus;
            var outcomeStatus = await _contentRepository.GetApplicationOutcomeStatusAsync(content.Id, content.UserId);

            return View("ApplicationSent", new ApplicationSentModel { 
                ApplicationNumber = content.ApplicationNumber ?? "N/A",
                ApplicationStatus = outcomeStatus.ToOutcomeStatusString(status),
                SubmittedDate = content.SubmittedUtc.HasValue ? content.SubmittedUtc.Value : DateTime.UtcNow,
                Status = status,
                OutcomeStatus = outcomeStatus
            } );
        }
        

        [HttpPost, ActionName("ApplicationComplete")]
        public async Task<IActionResult> ApplicationComplete(string equality)
        {
            if (equality == "1")
            {
                return RedirectToAction("section", new { pagename = "eq-survey-question-one" });
            }
            else 
            {
                return RedirectToAction("ApplicationSent");
            }
        }

        private void SetPageTitle(string title)
        {
            ViewData["Title"] = $"{title}";
        }

        private FieldStatus GetFieldStatus(Page currentPage, BaseModel model)
        {
            if(currentPage.FieldType == FieldType.gdsFileUpload && model.Mandatory.HasValue && model.Mandatory.Value == false)
            {
                // Use the values from the radio buttons
               return model.FieldStatus == null ? FieldStatus.InProgress : (FieldStatus)model.FieldStatus;
            }

            if (model.ShowMarkAsComplete)
            {
                if (model.MarkAsComplete)
                {
                    return FieldStatus.Completed;
                }
                else
                {
                    return FieldStatus.InProgress;
                }
            }

            return FieldStatus.NotStarted;
        }
    }
}
