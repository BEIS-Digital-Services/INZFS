﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using INZFS.MVC.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Routing;
using Microsoft.AspNetCore.Authorization;
using INZFS.MVC.Models;
using INZFS.MVC.Forms;
using Microsoft.AspNetCore.Http;
using OrchardCore.Media;
using Microsoft.Extensions.Logging;
using INZFS.MVC.Models.DynamicForm;
using INZFS.MVC.Services.FileUpload;
using INZFS.MVC.Services.VirusScan;
using System.Text.Json;
using ClosedXML.Excel;
using OrchardCore.FileStorage;
using System.IO;
using ClosedXML.Excel.CalcEngine.Exceptions;
using System.Globalization;
using INZFS.MVC.Services;
using INZFS.MVC.Records;
using System.Security.Claims;
using INZFS.MVC.Extensions;
using INZFS.MVC.Constants;
using INZFS.MVC.Filters;
using INZFS.MVC.Settings;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using System.Text;
using Ganss.XSS;
using INZFS.MVC.Services.Zip;

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
            IWebHostEnvironment env)
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

            _sanitizer = new HtmlSanitizer();
            _sanitizer.AllowedAttributes.Clear();
            _sanitizer.AllowedTags.Clear();
            _sanitizer.AllowedCssProperties.Clear();
        }

        [HttpGet]
        [ServiceFilter(typeof(ApplicationRedirectionAttribute))]
        public async Task<IActionResult> Section(string pagename, string id)
        {
            if(string.IsNullOrEmpty(pagename))
            {
                return NotFound();
            }
            pagename = pagename.ToLower().Trim();


            var userId = GetUserId();
            var content = await _contentRepository.GetApplicationContent(userId);
            if(content == null)
            {
                content = await _contentRepository.CreateApplicationContent(userId);
            }
            
            if (string.IsNullOrEmpty(User.Identity.ApplicationNumber()))
            {
                var claimIdentity = (ClaimsIdentity)User.Identity;
                claimIdentity.AddClaim(new System.Security.Claims.Claim("ApplicationNumber", content.ApplicationNumber));
            }

            if(RedirectToApplicationSubmittedPage(pagename, content))
            {
                return RedirectToAction("ApplicationSent");
            }

            // Page
            var currentPage = _applicationDefinition.Application.AllPages.FirstOrDefault(p => p.Name.ToLower().Equals(pagename));
            if(currentPage != null)
            {
                var field = content?.Fields?.FirstOrDefault(f => f.Name.Equals(currentPage.FieldName));
                return GetViewModel(currentPage, field);
            }

            //Overview
            if (pagename.ToLower() == "application-overview")
            {
                var applicationOverviewContentModel = GetApplicationOverviewContent(content);

                SetPageTitle("Application Overview");
                return View("ApplicationOverview", applicationOverviewContentModel);
            }

            // Section
            var currentSection = _applicationDefinition.Application.Sections.FirstOrDefault(section => section.Url.Equals(pagename));
            if (currentSection != null)
            {
                var sectionContentModel = GetSectionContent(content, currentSection);
                SetPageTitle(currentSection.Title);
                return View(currentSection.RazorView, sectionContentModel);
            }


            return NotFound();

        }

        private bool RedirectToApplicationSubmittedPage(string pageName, ApplicationContent content)
        {
            if (content.ApplicationStatus != ApplicationStatus.InProgress)
            {
                if(pageName.ToLower() == "application-overview")
                {
                    return true;
                }
                var currentSection = _applicationDefinition.Application.Sections.FirstOrDefault(section =>
                 section.Pages.Any(page => page.Name.ToLower() == pageName.ToLower()));
                
                if (currentSection == null)
                {
                    currentSection = _applicationDefinition.Application.Sections.FirstOrDefault(section => section.Url.ToLower().Equals(pageName.ToLower()));
                }
                if(currentSection?.BelongsToApplication == true)
                {
                    return true;
                }
            }

            return false;
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
                    var contentToSave = await _contentRepository.GetApplicationContent(GetUserId());
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
                                return PopulateViewModel(currentPage, model, existingData);
                            }
                        }
                    }
                }
            }
            if (ModelState.IsValid || submitAction == "DeleteFile")
            {
                var contentToSave = await _contentRepository.GetApplicationContent(GetUserId());
                
                if (contentToSave == null)
                {
                    contentToSave = new ApplicationContent();
                    contentToSave.Application = new Application();
                    contentToSave.UserId = GetUserId();
                    contentToSave.CreatedUtc = DateTime.UtcNow;
                }

                contentToSave.ModifiedUtc = DateTime.UtcNow;

                
                string publicUrl = string.Empty;
                var additionalInformation = string.Empty;
                if (currentPage.FieldType == FieldType.gdsFileUpload)
                {
                    if (file != null || submitAction.ToLower() == "UploadFile".ToLower())
                    {
                        var directoryName = GetUserId();
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
                return PopulateViewModel(currentPage, model);
            }
        }

        private ViewResult AddErrorAndPopulateViewModel(Page currentPage, BaseModel currentModel,string fieldName, string errorMessage)
        {
            ModelState.AddModelError(fieldName, errorMessage);
            return PopulateViewModel(currentPage, currentModel);
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public async Task<IActionResult> Submit()
        {
            SetPageTitle("Submit application");
            
            var content = await _contentRepository.GetApplicationContent(GetUserId());
            if(content.ApplicationStatus != ApplicationStatus.InProgress)
            {
                return RedirectToAction("ApplicationSent");
            }
            var applicationOverviewContentModel = GetApplicationOverviewContent(content);
            if(applicationOverviewContentModel.TotalSections == applicationOverviewContentModel.TotalSectionsCompleted 
                                    && DateTime.UtcNow <= _applicationOption.EndDate)
            {
                bool applicationUploadSuccess = await AddApplicationToBlobStorage();

                if(!applicationUploadSuccess)
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

        public async Task<bool> AddApplicationToBlobStorage()
        {
            string userId = GetUserId();
            string applicationId = await _zipService.GetApplicationId(userId);
            byte[] zipFileBytes = await _zipService.GetZipFileBytes("pdf", userId, true);
            string companyName = await _zipService.GetApplicationCompanyName(userId);
            string name = $"_{companyName}_{applicationId}.zip";

            MemoryStream ms = new(zipFileBytes);
            FormFile file = new(ms, 0, zipFileBytes.Length, name, name);

            var url = await AddFileToBlobStorage(file);

            return url != null ? true : false;
        }

        public async Task<string> AddFileToBlobStorage(FormFile file)
        {
            try
            {
                var publicUrl = await _fileUploadService.SaveFile(file, "Submitted Applications");
                return publicUrl;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        [ServiceFilter(typeof(ApplicationRedirectionAttribute))]
        public async Task<IActionResult> Complete()
        {
            SetPageTitle("Application completed");
            var content = await _contentRepository.GetApplicationContent(GetUserId());
            if (content.ApplicationStatus != ApplicationStatus.InProgress)
            {
                return RedirectToAction("ApplicationSent");
            }
            var applicationOverviewContentModel = GetApplicationOverviewContent(content);
            if (applicationOverviewContentModel.TotalSections == applicationOverviewContentModel.TotalSectionsCompleted)
            {
                await _contentRepository.UpdateStatus(GetUserId(), ApplicationStatus.Submitted);
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
            var content = await _contentRepository.GetApplicationContent(GetUserId());
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

        private ViewResult GetViewModel(Page currentPage, Field field)
        {
            SetPageTitle(currentPage.PageTitle ?? currentPage.SectionTitle);
            BaseModel model;
            switch (currentPage.FieldType)
            {
                case FieldType.gdsSingleRadioSelectOption:
                    model = new RadioSingleSelectModel();
                    return View("SingleRadioSelectInput", PopulateModel(currentPage, model, field));
                case FieldType.gdsTextBox:
                    model = new TextInputModel();
                    return View("TextInput", PopulateModel(currentPage, model, field));
                case FieldType.gdsTextArea:
                    model = new TextAreaModel();
                    return View("TextArea", PopulateModel(currentPage, model, field));
                case FieldType.gdsDateBox:
                    model = PopulateModel(currentPage, new DateModel(), field);
                    var dateModel = (DateModel)model;
                    if (!string.IsNullOrEmpty(model.DataInput))
                    {
                        var inputDate = DateTime.Parse(model.DataInput, CultureInfo.GetCultureInfoByIetfLanguageTag("en-GB"));
                        dateModel.Day = inputDate.Day;
                        dateModel.Month = inputDate.Month;
                        dateModel.Year = inputDate.Year;
                    }
                    return View("DateInput", model);
                case FieldType.gdsMultiLineRadio:
                    model = new MultiRadioInputModel();
                    return View("MultiRadioInput", PopulateModel(currentPage, model, field));
                case FieldType.gdsYesorNoRadio:
                    model = new YesornoInputModel();
                    return View("YesornoInput", PopulateModel(currentPage, model, field));
                case FieldType.gdsMultiSelect:
                    model = PopulateModel(currentPage, new MultiSelectInputModel(), field);
                    var multiSelect = (MultiSelectInputModel)model;
                    if (!string.IsNullOrEmpty(model.DataInput))
                    {
                        var UserInputList = model.DataInput.Split(',').ToList();
                        var OtherOptionData = model.OtherOption;
                        multiSelect.UserInput = UserInputList;
                        multiSelect.OtherOption = OtherOptionData;
                    }
                    return View("MultiSelectInput", PopulateModel(currentPage, model));
                case FieldType.gdsAddressTextBox:
                    model = PopulateModel(currentPage, new AddressInputModel(), field);
                    var addressInputModel = (AddressInputModel)model;
                    if (!string.IsNullOrEmpty(model.DataInput))
                    {
                        var userAddress = model.DataInput.Split(',').ToList();
                        addressInputModel.AddressLine1 = userAddress[0];
                        addressInputModel.AddressLine2 = userAddress[1];
                        addressInputModel.City = userAddress[2];
                        addressInputModel.County = userAddress[3];
                        addressInputModel.PostCode = userAddress[4];
                    }
                    return View("AddressInput", PopulateModel(currentPage, model));
                case FieldType.gdsFileUpload:
                    model = PopulateModel(currentPage, new FileUploadModel(), field);
                    var uploadmodel = (FileUploadModel)model;
                    if(!string.IsNullOrEmpty(field?.AdditionalInformation))
                    {
                        uploadmodel.UploadedFile = JsonSerializer.Deserialize<UploadedFile>(field.AdditionalInformation);
                    }
                    uploadmodel.FieldStatus = field?.FieldStatus;
                    return View("FileUpload", uploadmodel);
                case FieldType.gdsStaticPage:
                    model = new StaticPageModel();
                    return View("_StaticPage", PopulateModel(currentPage, model, field));
                default:
                    throw new Exception("Invalid field type");
                
            }
        }

        protected ViewResult PopulateViewModel(Page currentPage, BaseModel currentModel, Field field = null)
        {
            SetPageTitle(currentPage.SectionTitle);
            switch (currentPage.FieldType)
            {
                case FieldType.gdsTextBox:
                    return View("TextInput", PopulateModel(currentPage, currentModel));
                case FieldType.gdsTextArea:
                    return View("TextArea", PopulateModel(currentPage, currentModel));
                case FieldType.gdsDateBox:
                    return View("DateInput", PopulateModel(currentPage, currentModel));
                case FieldType.gdsMultiLineRadio:
                    return View("MultiRadioInput", PopulateModel(currentPage, currentModel));
                case FieldType.gdsYesorNoRadio:
                    return View("YesornoInput", PopulateModel(currentPage, currentModel));
                case FieldType.gdsMultiSelect:
                    return View("MultiSelectInput", PopulateModel(currentPage, currentModel));
                case FieldType.gdsFileUpload:
                    var model = PopulateModel(currentPage, currentModel, field);
                    var uploadmodel = (FileUploadModel)model;
                    if (!string.IsNullOrEmpty(field?.AdditionalInformation))
                    {
                        uploadmodel.UploadedFile = JsonSerializer.Deserialize<UploadedFile>(field.AdditionalInformation);
                    }
                    uploadmodel.FieldStatus = field?.FieldStatus;
                    return View("FileUpload", uploadmodel);
                case FieldType.gdsCurrencyBox:
                    return View("CurrencyInput", PopulateModel(currentPage, currentModel));
                case FieldType.gdsSingleRadioSelectOption:
                    return View("SingleRadioSelectInput", PopulateModel(currentPage, currentModel));
                case FieldType.gdsAddressTextBox:
                    return View("AddressInput", PopulateModel(currentPage, currentModel));
                case FieldType.gdsStaticPage:
                    return View("_StaticPage", PopulateModel(currentPage, currentModel));
                default:
                    throw new Exception("Invalid field type");
            }
        }

        private BaseModel PopulateModel(Page currentPage, BaseModel currentModel, Field field = null)
        {
            currentModel.TextType = currentPage.TextType;
            currentModel.YesNoInput = currentPage.YesNoInput;
            currentModel.Question = currentPage.Question;
            currentModel.FriendlyFieldName = currentPage.FriendlyFieldName;
            currentModel.PageName = currentPage.Name;
            currentModel.FieldName = currentPage.FieldName;
            currentModel.SectionTitle = currentPage.SectionTitle ?? currentPage.Question;
            currentModel.Hint = currentPage.Hint;
            currentModel.NextPageName = currentPage.NextPageName;
            currentModel.ReturnPageName = currentPage.ReturnPageName;
            currentModel.ShowMarkAsComplete = currentPage.ShowMarkComplete;
            currentModel.HasOtherOption = currentPage.HasOtherOption;
            currentModel.MaxLength = currentPage.MaxLength;
            currentModel.MaxLengthValidationType = currentPage.MaxLengthValidationType;
            currentModel.SelectedOptions = currentPage.SelectOptions;
            currentModel.FieldType = currentPage.FieldType;
            currentModel.Mandatory = currentPage.Mandatory;
            currentModel.AccordianReference = currentPage.AccordianReference;
            currentModel.AcceptableFileExtensions = currentPage.AcceptableFileExtensions;
            if (currentPage.Actions?.Count > 0)
            {
                currentModel.Actions = currentPage.Actions;
            }
            if (currentPage.ShowMarkComplete)
            {
                currentModel.MarkAsComplete = field?.MarkAsComplete != null ? field.MarkAsComplete.Value : false;
            }
            if (currentPage.PreviousPage != null)
            {
                currentModel.PreviousPage = currentPage.PreviousPage;
            }

            currentModel.FileToDownload = currentPage.FileToDownload;

            if (!string.IsNullOrEmpty(field ?.Data))
            {
                currentModel.DataInput = field?.Data;
            }
            if (!string.IsNullOrEmpty(field?.OtherOption))
            {
                currentModel.OtherOption = field?.OtherOption;
            }


            var currentSection = _applicationDefinition.Application.Sections.FirstOrDefault(section =>
                                         section.Pages.Any(page => page.Name == currentPage.Name));
            var DynamicPagesInSection = currentSection.Pages.Where(p => p.HideFromSummary == false).ToList();
            var index = DynamicPagesInSection.FindIndex(p => p.Name.ToLower().Equals(currentPage.Name));

            currentModel.QuestionNumber = index + 1;
            currentModel.TotalQuestions = currentSection.Pages.Count(p => !p.HideFromSummary);
            currentModel.HideQuestionCounter = currentSection.HideQuestionCounter;
            currentModel.HideBreadCrumbs = currentSection.HideBreadCrumbs;

            if (string.IsNullOrEmpty(currentPage.ContinueButtonText))
            {
                currentModel.ContinueButtonText = currentSection.ContinueButtonText;
            }else
            {
                currentModel.ContinueButtonText = currentPage.ContinueButtonText;
            }
            currentModel.ReturnToSummaryPageLinkText = currentSection.ReturnToSummaryPageLinkText;
            currentModel.SectionUrl = currentSection.Url;
            currentModel.SectionInfo = currentSection;

            var currentPageIndex = currentSection.Pages.FindIndex(p => p.Name == currentPage.Name);
            if (currentPageIndex >= 1)
            {
                currentModel.PreviousPageName = currentPage.PreviousPageName ?? currentSection.Pages[currentPageIndex -1].Name;
            } 


            if (!string.IsNullOrEmpty(currentPage.Description))
            {
                currentModel.Description = currentPage.Description;
            }

            currentModel.DisplayQuestionCounter = currentPage.DisplayQuestionCounter;
            currentModel.GridDisplayType = currentPage.GridDisplayType == null ? currentSection.GridDisplayType : currentPage.GridDisplayType.Value;
            return currentModel;
        }

        private SectionContent GetSectionContent(ApplicationContent content, Section section)
        {
            var sectionContentModel = new SectionContent();
            sectionContentModel.ApplicationNumber = content.ApplicationNumber;
            sectionContentModel.TotalQuestions = section.Pages.Count(p => !p.HideFromSummary);
            sectionContentModel.Sections = new List<SectionModel>();
            sectionContentModel.Title = section.Title;
            sectionContentModel.OverviewTitle = section.OverviewTitle;
            sectionContentModel.Url = section.Url;
            sectionContentModel.ReturnUrl = section.ReturnUrl;
            sectionContentModel.HasErrors = TempData.ContainsKey(TempDataKeys.ApplicationOverviewError) && (bool)TempData.ContainsKey(TempDataKeys.ApplicationOverviewError);

            foreach (var pageContent in section.Pages)
            {
                var dependsOn = pageContent.DependsOn;
                if (dependsOn != null)
                {
                    var datafieldForCurrentPage = content?.Fields?.FirstOrDefault(f => f.Name.Equals(dependsOn.FieldName));
                    if (datafieldForCurrentPage?.Data != dependsOn.Value)
                    {
                        continue;
                    }
                }
                if(pageContent.HideFromSummary)
                {
                    continue;
                }

                var field = content?.Fields?.FirstOrDefault(f => f.Name.Equals(pageContent.FieldName));
                
                var sectionModel = new SectionModel();
                sectionModel.Title = pageContent.SectionTitle ?? pageContent.Question;
                sectionModel.Url = pageContent.Name;
                
                if (field?.FieldStatus == null)
                {
                    sectionModel.SectionStatus = FieldStatus.NotStarted;
                }
                else
                {
                    if(pageContent.CompletionDependsOn != null && pageContent.CompletionDependsOn.Value == field.Data)
                    {
                        // Get status of the dependant fields
                        bool areAlldependantFieldsComplete = false;
                        var dependantFields = content?.Fields?.Where(f => pageContent.CompletionDependsOn.Fields?.Contains(f.Name) == true);
                        if(dependantFields?.Any() == true)
                        {
                            areAlldependantFieldsComplete = dependantFields.All(f => f.FieldStatus.Value == FieldStatus.Completed);
                        }

                        sectionModel.SectionStatus = (areAlldependantFieldsComplete 
                                                    && field.FieldStatus.HasValue 
                                                    && field.FieldStatus.Value == FieldStatus.Completed) ? FieldStatus.Completed 
                                                                                                         : FieldStatus.InProgress;
                    }
                    else
                    {
                        sectionModel.SectionStatus = field.FieldStatus.HasValue ? field.FieldStatus.Value : FieldStatus.NotStarted;
                    }
                }

                if (pageContent.Name == "subsidy-requirements")
                {
                    var resultsFields = content?.Fields?.FindAll(f => f.Name.Contains("subsidy") && f.Name.Contains("result"));
                    if (resultsFields.Any(f => f.Data == "true"))
                    {
                        sectionModel.SectionStatus = FieldStatus.Completed;
                    }
                    else
                    {
                        sectionModel.SectionStatus = FieldStatus.NotStarted;
                    }
                }

                if (sectionModel.SectionStatus == FieldStatus.Completed)
                {
                    sectionContentModel.TotalQuestionsCompleted++;
                }
                sectionContentModel.Sections.Add(sectionModel);
            }


            return sectionContentModel;
        }

        private ApplicationOverviewContent GetApplicationOverviewContent(ApplicationContent content)
        {
            var sections = _applicationDefinition.Application.Sections.Where(s => s.BelongsToApplication == true);
            var applicationOverviewContentModel = new ApplicationOverviewContent();

            applicationOverviewContentModel.ApplicationNumber = content.ApplicationNumber;
            foreach (var section in sections)
            {
                var sectionContentModel = GetSectionContent(content, section);
                var applicationOverviewModel = new ApplicationOverviewModel();
                applicationOverviewModel.SectionTag = section.Tag;
                applicationOverviewModel.Title = sectionContentModel.OverviewTitle;
                applicationOverviewModel.Url = sectionContentModel.Url;
                applicationOverviewModel.SectionStatus = sectionContentModel.OverallStatus;

                applicationOverviewContentModel.Sections.Add(applicationOverviewModel);
            }

            applicationOverviewContentModel.TotalSections = sections.Count();
            applicationOverviewContentModel.TotalSectionsCompleted = applicationOverviewContentModel.
                                                Sections.Count(section => section.SectionStatus == FieldStatus.Completed);

            applicationOverviewContentModel.HasErrors = TempData.ContainsKey(TempDataKeys.ApplicationOverviewError) 
                                    && (bool)TempData.ContainsKey(TempDataKeys.ApplicationOverviewError);
            return applicationOverviewContentModel;
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
