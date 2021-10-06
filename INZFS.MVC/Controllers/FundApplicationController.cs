using System;
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

namespace INZFS.MVC.Controllers
{
    [Authorize]
    public class FundApplicationController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IVirusScanService _virusScanService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IMediaFileStore _mediaFileStore;
        private readonly dynamic New;
        private readonly INotifier _notifier;
        private readonly YesSql.ISession _session;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly INavigation _navigation;
        private readonly ILogger _logger;
        private readonly IContentRepository _contentRepository;
        private readonly ApplicationDefinition _applicationDefinition;
        private readonly IApplicationEmailService _applicationEmailService;
        
        public FundApplicationController(ILogger<FundApplicationController> logger, IContentManager contentManager,
            IMediaFileStore mediaFileStore, IContentDefinitionManager contentDefinitionManager,
            IContentItemDisplayManager contentItemDisplayManager, IHtmlLocalizer<FundApplicationController> htmlLocalizer,
            INotifier notifier, YesSql.ISession session, IShapeFactory shapeFactory,
            IUpdateModelAccessor updateModelAccessor, INavigation navigation,
            IContentRepository contentRepository, IFileUploadService fileUploadService, 
            IVirusScanService virusScanService, ApplicationDefinition applicationDefinition, IApplicationEmailService applicationEmailService)
        {
            _contentManager = contentManager;
            _mediaFileStore = mediaFileStore;
            _notifier = notifier;
            _session = session;
            _updateModelAccessor = updateModelAccessor;
            _logger = logger;
            New = shapeFactory;
            _navigation = navigation;
            _contentRepository = contentRepository;
            _virusScanService = virusScanService;
            _fileUploadService = fileUploadService;
            _applicationDefinition = applicationDefinition;
            _applicationEmailService = applicationEmailService;
        }

        [HttpGet]
        public async Task<IActionResult> Section(string pagename, string id)
        {
            if(string.IsNullOrEmpty(pagename))
            {
                return NotFound();
            }
            pagename = pagename.ToLower().Trim();


            var content = await _contentRepository.GetApplicationContent(User.Identity.Name);
            if(content == null)
            {
                content = await _contentRepository.CreateApplicationContent(User.Identity.Name);
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
        public async Task<IActionResult> Save([Bind(Prefix = "submit.Publish")] string submitAction, string returnUrl, string pageName, IFormFile? file, BaseModel model)
        {
            var currentPage = _applicationDefinition.Application.AllPages.FirstOrDefault(p => p.Name.ToLower().Equals(pageName));
            if (currentPage.FieldType == FieldType.gdsFileUpload)
            {
                if (file != null || submitAction == "UploadFile")
                {
                    ModelState.Clear();
                    var errorMessage = await _fileUploadService.Validate(file, currentPage);
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        ModelState.AddModelError("DataInput", errorMessage);
                    }
                }

                if(ModelState.IsValid && submitAction != "UploadFile")
                {
                    var fieldStatus = GetFieldStatus(currentPage, model);
                    var contentToSave = await _contentRepository.GetApplicationContent(User.Identity.Name);
                    if (contentToSave != null)
                    {
                        var existingData = contentToSave.Fields.FirstOrDefault(f => f.Name.Equals(currentPage.FieldName));
                        if (fieldStatus == FieldStatus.Completed)
                        {
                            if (submitAction != "DeleteFile" && string.IsNullOrEmpty(existingData?.AdditionalInformation))
                            {
                                ModelState.AddModelError("DataInput", "No file was uploaded.");
                            }
                        }
                        if (fieldStatus == FieldStatus.NotApplicable)
                        {
                            if (submitAction != "DeleteFile" && !string.IsNullOrEmpty(existingData?.AdditionalInformation))
                            {
                                ModelState.AddModelError("DataInput", "Please remove the uploaded file if this question is not applicable.");
                                return PopulateViewModel(currentPage, model, existingData);
                            }
                        }
                    }
                }
            }
            if (ModelState.IsValid || submitAction == "DeleteFile")
            {
                var contentToSave = await _contentRepository.GetApplicationContent(User.Identity.Name);
                
                if (contentToSave == null)
                {
                    contentToSave = new ApplicationContent();
                    contentToSave.Application = new Application();
                    contentToSave.Author = User.Identity.Name;
                    contentToSave.CreatedUtc = DateTime.UtcNow;
                }

                contentToSave.ModifiedUtc = DateTime.UtcNow;

                
                string publicUrl = string.Empty;
                var additionalInformation = string.Empty;
                if (currentPage.FieldType == FieldType.gdsFileUpload)
                {
                    if (file != null || submitAction.ToLower() == "UploadFile".ToLower())
                    {
                        var directoryName = Guid.NewGuid().ToString();
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

                        if (file.FileName.ToLower().Contains(".xlsx") && currentPage.Name == "project-cost-breakdown")
                        {
                            try
                            {
                                XLWorkbook wb = new(file.OpenReadStream());

                                try
                                {
                                    IXLWorksheet ws = wb.Worksheet("A. Summary");
                                    IXLCell totalGrantFunding = ws.Search("Total BEIS grant applied for").First<IXLCell>();
                                    IXLCell totalMatchFunding = ws.Search("Total match funding contribution").First<IXLCell>();
                                    IXLCell totalProjectFunding = ws.Search("Total project costs").First<IXLCell>();

                                    bool spreadsheetValid = totalGrantFunding != null && totalMatchFunding != null && totalProjectFunding != null;

                                    if (spreadsheetValid)
                                    {
                                        try
                                        {
                                            ParsedExcelData parsedExcelData = new();
                                            parsedExcelData.ParsedTotalProjectCost = Double.Parse(totalProjectFunding.CellRight().CachedValue.ToString()).ToString("C", new CultureInfo("en-GB"));
                                            parsedExcelData.ParsedTotalGrantFunding = Double.Parse(totalGrantFunding.CellRight().CachedValue.ToString()).ToString("C", new CultureInfo("en-GB"));
                                            parsedExcelData.ParsedTotalGrantFundingPercentage = Double.Parse(totalGrantFunding.CellRight().CellRight().CachedValue.ToString()).ToString("0.00%");
                                            parsedExcelData.ParsedTotalMatchFunding = Double.Parse(totalMatchFunding.CellRight().CachedValue.ToString()).ToString("C", new CultureInfo("en-GB"));
                                            parsedExcelData.ParsedTotalMatchFundingPercentage = Double.Parse(totalMatchFunding.CellRight().CellRight().CachedValue.ToString()).ToString("0.00%");
                                            uploadedFile.ParsedExcelData = parsedExcelData;
                                        }
                                        catch (DivisionByZeroException e)
                                        {
                                            ModelState.AddModelError("DataInput", "Uploaded spreadsheet is incomplete. Complete all mandatory information within the template.");
                                        }
                                        catch (FormatException e)
                                        {
                                            ModelState.AddModelError("DataInput", "Uploaded spreadsheet is incomplete. Complete all mandatory information within the template.");
                                        }
                                    }
                                    else
                                    {
                                        ModelState.AddModelError("DataInput", "Uploaded spreadsheet does not match the template. Use the provided template.");
                                    }
                                }
                                catch (ArgumentException e)
                                {
                                    ModelState.AddModelError("DataInput", "Uploaded spreadsheet does not match the template. Use the provided template.");
                                }
                                catch (InvalidOperationException e)
                                {
                                    ModelState.AddModelError("DataInput", "Uploaded spreadsheet does not match the template. Use the provided template.");
                                }
                            }
                            catch (InvalidDataException e)
                            {
                                ModelState.AddModelError("DataInput", "Invalid file uploaded");
                                return PopulateViewModel(currentPage, model);
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
                        OtherOption = model.GetOtherSelected(),
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
                        if ((fileHasChanged && !string.IsNullOrEmpty(existingFieldData?.AdditionalInformation)) || submitAction == "DeleteFile")
                        {
                            var uploadedFile = JsonSerializer.Deserialize<UploadedFile>(existingFieldData.AdditionalInformation);
                            var deleteSucessful = await _fileUploadService.DeleteFile(uploadedFile.FileLocation);
                            if(submitAction == "DeleteFile")
                            {
                                additionalInformation = null;
                            }

                        }

                    }
                    existingFieldData.Data = model.GetData();
                    existingFieldData.FieldStatus = GetFieldStatus(currentPage, model);
                    if (!string.IsNullOrEmpty(existingFieldData.Data) && existingFieldData.Data.Contains("Other"))
                    {
                        existingFieldData.OtherOption = model.GetOtherSelected();
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


                _session.Save(contentToSave);

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

        public async Task<IActionResult> Submit()
        {
            SetPageTitle("Submit application");
            var content = await _contentRepository.GetApplicationContent(User.Identity.Name);
            if(content.ApplicationStatus != ApplicationStatus.InProgress)
            {
                return RedirectToAction("ApplicationSent");
            }
            var applicationOverviewContentModel = GetApplicationOverviewContent(content);
            if(applicationOverviewContentModel.TotalSections == applicationOverviewContentModel.TotalSectionsCompleted)
            {
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
        public async Task<IActionResult> Complete()
        {
            SetPageTitle("Application completed");
            var content = await _contentRepository.GetApplicationContent(User.Identity.Name);
            if (content.ApplicationStatus != ApplicationStatus.InProgress)
            {
                return RedirectToAction("ApplicationSent");
            }
            var applicationOverviewContentModel = GetApplicationOverviewContent(content);
            if (applicationOverviewContentModel.TotalSections == applicationOverviewContentModel.TotalSectionsCompleted)
            {
                await _contentRepository.UpdateStatus(User.Identity.Name, ApplicationStatus.Submitted);
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
            var content = await _contentRepository.GetApplicationContent(User.Identity.Name);
            return View("ApplicationSent", new ApplicationSentModel { 
                ApplicationNumber = content.ApplicationNumber,
                ApplicationStatus = content.ApplicationStatus.ToStatusString(),
                SubmittedDate = content.SubmittedUtc.Value
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
            SetPageTitle(currentPage.SectionTitle);
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
                    sectionModel.SectionStatus = field.FieldStatus.HasValue ? field.FieldStatus.Value : FieldStatus.NotStarted;
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
