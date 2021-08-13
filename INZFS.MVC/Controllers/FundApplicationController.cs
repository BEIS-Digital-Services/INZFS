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
using OrchardCore.ContentManagement.Records;
using INZFS.MVC.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Routing;
using Microsoft.AspNetCore.Authorization;
using INZFS.MVC.Models;
using INZFS.MVC.Forms;
using INZFS.MVC.Models.ProposalWritten;
using INZFS.MVC.ViewModels.ProposalWritten;
using Microsoft.AspNetCore.Http;
using OrchardCore.Media;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using INZFS.MVC.Models.ProposalFinance;
using INZFS.MVC.ViewModels.ProposalFinance;
using OrchardCore.Flows.Models;
using INZFS.MVC.Models.DynamicForm;
using INZFS.MVC.Services.FileUpload;
using INZFS.MVC.Services.VirusScan;
using System.Text.Json;
using ClosedXML.Excel;
using OrchardCore.FileStorage;
using System.IO;
using ClosedXML.Excel.CalcEngine.Exceptions;

namespace INZFS.MVC.Controllers
{
    [Authorize]
    public class FundApplicationController : Controller
    {
        private const string contentType = "ProposalSummaryPart";

        private readonly IContentManager _contentManager;
        private readonly IVirusScanService _virusScanService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IHtmlLocalizer H;
        private readonly IMediaFileStore _mediaFileStore;
        private readonly dynamic New;
        private readonly INotifier _notifier;
        private readonly YesSql.ISession _session;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly INavigation _navigation;
        private readonly ILogger _logger;
        private readonly IContentRepository _contentRepository;
        private readonly ApplicationDefinition _applicationDefinition;

        public FundApplicationController(ILogger<FundApplicationController> logger, IContentManager contentManager, IMediaFileStore mediaFileStore, IContentDefinitionManager contentDefinitionManager,
            IContentItemDisplayManager contentItemDisplayManager, IHtmlLocalizer<FundApplicationController> htmlLocalizer,
            INotifier notifier, YesSql.ISession session, IShapeFactory shapeFactory,
            IUpdateModelAccessor updateModelAccessor, INavigation navigation, IContentRepository contentRepository, IFileUploadService fileUploadService, IVirusScanService virusScanService, ApplicationDefinition applicationDefinition)
        {
            _contentManager = contentManager;
            _mediaFileStore = mediaFileStore;
            _contentDefinitionManager = contentDefinitionManager;
            _contentItemDisplayManager = contentItemDisplayManager;
            _notifier = notifier;
            _session = session;
            _updateModelAccessor = updateModelAccessor;
            _logger = logger;
            H = htmlLocalizer;
            New = shapeFactory;
            _navigation = navigation;
            _contentRepository = contentRepository;
            _virusScanService = virusScanService;
            _fileUploadService = fileUploadService;
            _applicationDefinition = applicationDefinition;
        }

        [HttpGet]
        public async Task<IActionResult> Section(string pagename, string id)
        {
            if(string.IsNullOrEmpty(pagename))
            {
                return NotFound();
            }
            pagename = pagename.ToLower().Trim();


            // Page
            var currentPage = _applicationDefinition.Application.AllPages.FirstOrDefault(p => p.Name.ToLower().Equals(pagename));
            if(currentPage != null)
            {
                var content= await _contentRepository.GetApplicationContent(User.Identity.Name);
                var field = content?.Fields?.FirstOrDefault(f => f.Name.Equals(currentPage.FieldName));
                return GetViewModel(currentPage, field);
            }

            //Overview
            if (pagename.ToLower() == "application-overview")
            {
                var sections = _applicationDefinition.Application.Sections;
                var applicationOverviewContentModel = new ApplicationOverviewContent();

                var content = await _contentRepository.GetApplicationContent(User.Identity.Name);

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

                applicationOverviewContentModel.TotalSections = sections.Count;
                applicationOverviewContentModel.TotalSectionsCompleted = applicationOverviewContentModel.
                                                    Sections.Count(section => section.SectionStatus == SectionStatus.Completed);

                return View("ApplicationOverview", applicationOverviewContentModel);
            }

            // Section
            var currentSection = _applicationDefinition.Application.Sections.FirstOrDefault(section => section.Url.Equals(pagename));
            if (currentSection != null)
            {
                var content = await _contentRepository.GetApplicationContent(User.Identity.Name);
                var sectionContentModel = GetSectionContent(content, currentSection);
                return View(currentSection.RazorView, sectionContentModel);
            }

            


            if (pagename == "application-summary")
            {
                var model = await GetApplicationSummaryModel();
                return View("ApplicationSummary", model);
            }

            if (pagename == "summary")
            {
                var model = await GetSummaryModel();
                return View("Summary", model);
            }
            if (pagename == "proposal-written-summary")
            {
                var model = await GetApplicationWrittenSummaryModel();
                return View("ProposalWrittenSummary", model);
            }
            if (pagename == "proposal-finance-summary")
            {
                var model = await GeProposalFinanceModel();
                return View("ProposalFinanceSummary", model);
            }

            var page = _navigation.GetPage(pagename);
            if (page == null)
            {
                return NotFound();
            }
            else
            {
                if(page is ViewPage)
                {
                    var viewPage = (ViewPage)page;
                    var applicationDocumentPart = await _contentRepository.GetContentItemFromBagPart<ApplicationDocumentPart>(viewPage.ContentType, User.Identity.Name);
                    var model = applicationDocumentPart ?? new ApplicationDocumentPart();
                    ViewBag.ContentItemId = model.ContentItem?.ContentItemId;
                    return View(viewPage.ViewName, model);
                }

                return await Create(((ContentPage)page).ContentType);
            }
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

        public async Task<IActionResult> Create(string contentType)
        {
            if (String.IsNullOrWhiteSpace(contentType))
            {
                return NotFound();
            }

            Expression<Func<ContentItemIndex, bool>> expression = index => index.ContentType == ContentTypes.INZFSApplicationContainer;
            var containerContentItems = await _contentRepository.GetContentItems(expression, User.Identity.Name);

            if (containerContentItems.Any())
            {
                var existingContainerContentItem = containerContentItems.FirstOrDefault();
                var applicationContainer = existingContainerContentItem?.ContentItem.As<BagPart>();
                var existingContentItem = applicationContainer.ContentItems.FirstOrDefault(ci => ci.ContentType == contentType);
                if (existingContentItem != null)
                {
                    return await Edit(existingContentItem.ContentItemId, contentType);
                }
            }

            var newContentItem = await _contentManager.NewAsync(contentType);
            var model = await _contentItemDisplayManager.BuildEditorAsync(newContentItem, _updateModelAccessor.ModelUpdater, true);

            return View("Create", model);

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
                    var errorMessage = await _fileUploadService.Validate(file);
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        //TODO - Handle validation Error
                        ModelState.AddModelError("DataInput", errorMessage);
                    }
                }
                //UploadFile
                //else
                //{
                //    //TODO - Handle validation Error
                //    if (submitAction != "DeleteFile")
                //    {
                //        ModelState.AddModelError("DataInput", "No file was uploaded.");
                //    }
                //}
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
                        //var errorMessage = await _fileUploadService.Validate(file);
                        //if (!string.IsNullOrEmpty(errorMessage))
                        //{
                        //    //TODO - Handle validation Error
                        //    ModelState.AddModelError("DataInput", errorMessage);
                        //}

                        var directoryName = Guid.NewGuid().ToString();
                        publicUrl = await _fileUploadService.SaveFile(file, directoryName);
                        model.DataInput = file.FileName;

                        var uploadedFile = new UploadedFile()
                        {
                            FileLocation = publicUrl,
                            Name = file.FileName,
                            Size = (file.Length / (double)Math.Pow(1024, 2)).ToString("0.00")
                        };

                        if (file.FileName.ToLower().Contains(".xlsx") && currentPage.Name == "project-cost-breakdown")
                        {
                            // If env is Development, prepend local filepath to publicUrl to ensure functionality
                            if(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                            {
                                publicUrl = _mediaFileStore.NormalizePath("/App_Data/Sites/Default" + publicUrl);
                            }

                            try
                            {
                                XLWorkbook wb = new(publicUrl);

                                try
                                {
                                    IXLWorksheet ws = wb.Worksheet("A. Summary");
                                    //IXLCell totalGrantFunding = ws.Cell("A8");
                                    IXLCell totalGrantFunding = ws.Search("Total sum requested from BEIS").First<IXLCell>();
                                    IXLCell totalMatchFunding = ws.Search("Match funding contribution").First<IXLCell>();
                                    IXLCell totalProjectFunding = ws.Search("Total project costs").First<IXLCell>();

                                    bool spreadsheetValid = totalGrantFunding != null && totalMatchFunding != null && totalProjectFunding != null;

                                    if (spreadsheetValid)
                                    {
                                        try
                                        {
                                            ParsedExcelData parsedExcelData = new();
                                            parsedExcelData.ParsedTotalProjectCost = totalProjectFunding.CellRight().GetValue<double>().ToString("£0.00");
                                            parsedExcelData.ParsedTotalGrantFunding = totalGrantFunding.CellRight().GetValue<double>().ToString("£0.00");
                                            parsedExcelData.ParsedTotalGrantFundingPercentage = totalGrantFunding.CellRight().CellRight().GetValue<double>().ToString("0.00%");
                                            parsedExcelData.ParsedTotalMatchFunding = totalMatchFunding.CellRight().GetValue<double>().ToString("£0.00");
                                            parsedExcelData.ParsedTotalMatchFundingPercentage = totalMatchFunding.CellRight().CellRight().GetValue<double>().ToString("0.00%");
                                            uploadedFile.ParsedExcelData = parsedExcelData;
                                        }
                                        catch (DivisionByZeroException e)
                                        {
                                            ModelState.AddModelError("DataInput", "Template spreadsheet is incomplete.");
                                        }
                                        catch (FormatException e)
                                        {
                                            ModelState.AddModelError("DataInput", "Template spreadsheet is incomplete.");

                                        }
                                    }
                                    else
                                    {
                                        ModelState.AddModelError("DataInput", "Uploaded spreadsheet does not match the expected formatting. Please use the provided template.");
                                    }
                                }
                                catch (ArgumentException e)
                                {
                                    ModelState.AddModelError("DataInput", "Uploaded spreadsheet does not match the expected formatting. Please use the provided template.");
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
                            ModelState.AddModelError("DataInput", "No file was uploaded.");
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
                        AdditionalInformation = currentPage.FieldType == FieldType.gdsFileUpload ? additionalInformation : null
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
                    if(existingFieldData.Data == "Other")
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

                var index = _applicationDefinition.Application.AllPages.FindIndex(p => p.Name.ToLower().Equals(pageName));
                var section = _applicationDefinition.Application.Sections.Where(s => s.Pages.Any(c => c.Name == pageName.ToLower())).FirstOrDefault();

                var nextPage = _applicationDefinition.Application.AllPages.ElementAtOrDefault(index + 1);

                if (nextPage == null)
                {
                    return RedirectToAction("section", new { pagename = section.ReturnUrl ?? section.Url }); ;
                }


                var inSection = section.Pages.Contains(nextPage);

                if (!inSection)
                {
                    return RedirectToAction("section", new { pagename = section.ReturnUrl ?? section.Url });
                     
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
            return View("ApplicationSubmit");
        }

        public async Task<IActionResult> Complete()
        {
            return Redirect("/complete/applicationcomplete");
        }

        [HttpPost, ActionName("Create")]
        [FormValueRequired("submit.Publish")]
        public async Task<IActionResult> CreateAndPublishPOSTOld([Bind(Prefix = "submit.Publish")] string submitPublish, string returnUrl, string contentType, IFormFile? file)
        {
            var stayOnSamePage = submitPublish == "submit.PublishAndContinue";

            return await CreatePOST(contentType, returnUrl, stayOnSamePage, file, async contentItem =>
            {
                await _contentManager.PublishAsync(contentItem);

                var currentContentType = contentItem.ContentType;

            });
        }


        private async Task<IActionResult> CreatePOST(string id, string returnUrl, bool stayOnSamePage, IFormFile? file, Func<ContentItem, Task> conditionallyPublish)
        {
            Expression<Func<ContentItemIndex, bool>> expression = index => index.ContentType == ContentTypes.INZFSApplicationContainer;
            var containerContentItems = await _contentRepository.GetContentItems(expression, User.Identity.Name);

            var existingContainerContentItem = containerContentItems.FirstOrDefault();
            if (existingContainerContentItem == null)
            {
                // Create the container item first - done only once
                existingContainerContentItem = await _contentManager.NewAsync(ContentTypes.INZFSApplicationContainer);
                _session.Save(existingContainerContentItem);
                await _contentManager.CreateAsync(existingContainerContentItem, VersionOptions.Draft);
                await conditionallyPublish(existingContainerContentItem);

                containerContentItems = await _contentRepository.GetContentItems(expression, User.Identity.Name);
                existingContainerContentItem = containerContentItems.FirstOrDefault();
            }

            var bagPart = existingContainerContentItem?.ContentItem.As<BagPart>();
            bagPart.ContentItem = existingContainerContentItem;

            var contentItem = bagPart.ContentItems.FirstOrDefault(ci => ci.ContentItemId == id);
            if (contentItem == null)
            {
                contentItem = await _contentManager.NewAsync(id);
            }

            if (file != null)
            {
                var errorMessage = await _fileUploadService.Validate(file);
                var page = Request.Form["pagename"].ToString();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    if(page == "ExperienceSkills" || page == "ProjectPlan")
                    {
                        ViewBag.ErrorMessage = errorMessage;
                        return View(page, new ApplicationDocumentPart());
                    }
                    ViewBag.ErrorMessage = errorMessage;
                    return View(page);
                }

                var publicUrl = await _fileUploadService.SaveFile(file, contentItem.ContentItemId);
                TempData["UploadDetail"] = new UploadDetail
                {
                    ContentItemProperty = Request.Form["contentTypeProperty"],
                    FileName = publicUrl
                };
            }

            contentItem.Owner = User.Identity.Name;

            var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, true);

            if (!ModelState.IsValid)
            {
               await _session.CancelAsync();
                return View("Create", model);
            }

            bagPart.ContentItems.Add(contentItem);
            bagPart.ContentItem.Apply(nameof(BagPart), bagPart);
            _session.Save(existingContainerContentItem);
            await conditionallyPublish(existingContainerContentItem);

            var nextPageUrl = GetNextPageUrl(contentItem.ContentType);
            if (string.IsNullOrEmpty(nextPageUrl))
            {
                return NotFound();
            }
            return RedirectToAction("section", new { pagename = nextPageUrl });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string contentItemId, string contentName)
        {
            Expression<Func<ContentItemIndex, bool>> expression = index => index.ContentType == ContentTypes.INZFSApplicationContainer;
            var containerContentItems = await _contentRepository.GetContentItems(expression, User.Identity.Name);

            var existingContainerContentItem = containerContentItems.FirstOrDefault();
            var applicationContainer = existingContainerContentItem?.ContentItem.As<BagPart>();
            var contentItem = applicationContainer.ContentItems.FirstOrDefault(ci => ci.ContentItemId == contentItemId);

            if (contentItem == null)
                return NotFound();

            var model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);

            return View("Edit", model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Publish")]
        public async Task<IActionResult> EditAndPublishPOST(string contentItemId, [Bind(Prefix = "submit.Publish")] string submitPublish, string returnUrl, IFormFile? file)
        {
            var stayOnSamePage = submitPublish == "submit.PublishAndContinue";

            if(file != null)
            {
                var errorMessage = await _fileUploadService.Validate(file);

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ViewBag.ErrorMessage = errorMessage;
                    var content = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest); // THIS IS NOT NECESSARY
                    return await Edit(contentItemId, content.ContentType);
                }
                var publicUrl = await _fileUploadService.SaveFile(file, contentItemId);
                TempData["UploadDetail"] = new UploadDetail
                {
                    ContentItemProperty = Request.Form["contentTypeProperty"],
                    FileName = publicUrl
                };
            }

            return await EditPOST(contentItemId, returnUrl, stayOnSamePage, async contentItem =>
            {
                await _contentManager.PublishAsync(contentItem);

                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            });
        }

        private async Task<IActionResult> EditPOST(string contentItemId, string returnUrl, bool stayOnSamePage, Func<ContentItem, Task> conditionallyPublish)
        {
            Expression<Func<ContentItemIndex, bool>> expression = index => index.ContentType == ContentTypes.INZFSApplicationContainer;
            var containerContentItems = await _contentRepository.GetContentItems(expression, User.Identity.Name);

            var existingContainerContentItem = containerContentItems.FirstOrDefault();
            if (existingContainerContentItem == null)
            {
                return NotFound();
            }

            var bagPart = existingContainerContentItem?.ContentItem.As<BagPart>();
            bagPart.ContentItem = existingContainerContentItem;

            var contentItemToUpdate = bagPart.ContentItems.FirstOrDefault(ci => ci.ContentItemId == contentItemId);
            if (contentItemToUpdate == null)
            {
                return NotFound();
            }

            var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItemToUpdate, _updateModelAccessor.ModelUpdater, false);
            if (!ModelState.IsValid)
            {
                await _session.CancelAsync();
                return View("Edit", model);
            }

            bagPart.ContentItem.Apply(nameof(BagPart), bagPart);


            _session.Save(existingContainerContentItem);
            await conditionallyPublish(existingContainerContentItem);

            var nextPageUrl = GetNextPageUrl(contentItemToUpdate.ContentType);
            if (string.IsNullOrEmpty(nextPageUrl))
            {
                return NotFound();
            }
            return RedirectToAction("section", new { pagename = nextPageUrl });
        }

        private async Task<SummaryViewModel> GetSummaryModel()
        {
            var items = await _contentRepository.GetContentItemListFromBagPart(User.Identity.Name);
            var companyDetailsPart = GetContentPart<CompanyDetailsPart>(items, ContentTypes.CompanyDetails);
            var projectSummaryPart = GetContentPart<ProjectSummaryPart>(items, ContentTypes.ProjectSummary);
            var projectDetailsPart = GetContentPart<ProjectDetailsPart>(items, ContentTypes.ProjectDetails);
            var fundingPart = GetContentPart<OrgFundingPart>(items, ContentTypes.OrgFunding);

            var model = new SummaryViewModel
            {
                CompanyDetailsViewModel = new CompanyDetailsViewModel
                {
                    CompanyName = companyDetailsPart.CompanyName,
                    CompanyNumber = companyDetailsPart.CompanyNumber
                },
                ProjectSummaryViewModel = new ProjectSummaryViewModel
                {
                    ProjectName = projectSummaryPart.ProjectName,
                    Day = projectSummaryPart.Day,
                    Month = projectSummaryPart.Month,
                    Year = projectSummaryPart.Year,
                },
                ProjectDetailsViewModel = new ProjectDetailsViewModel
                {
                    Summary = projectDetailsPart.Summary,
                    Timing = projectDetailsPart.Timing
                },
                OrgFundingViewModel = new OrgFundingViewModel
                {
                    NoFunding = fundingPart.NoFunding,
                    Funders = fundingPart.Funders,
                    FriendsAndFamily = fundingPart.FriendsAndFamily,
                    PublicSectorGrants = fundingPart.PublicSectorGrants,
                    AngelInvestment = fundingPart.AngelInvestment,
                    VentureCapital = fundingPart.VentureCapital,
                    PrivateEquity = fundingPart.PrivateEquity,
                    StockMarketFlotation = fundingPart.StockMarketFlotation
                },
            };

            return model;
        }

        private async Task<ProposalFinanceSummaryViewModel> GeProposalFinanceModel()
        {
            var items = await _contentRepository.GetContentItemListFromBagPart(User.Identity.Name);

            var financeTurnoverPart = GetContentPart<FinanceTurnoverPart>(items, ContentTypes.FinanceTurnover);
            var financeBalanceSheetPart = GetContentPart<FinanceBalanceSheetPart>(items, ContentTypes.FinanceBalanceSheet);
            var financeRecoverVatPart = GetContentPart<FinanceRecoverVatPart>(items, ContentTypes.FinanceRecoverVat);
            var financeBarriersPart = GetContentPart<FinanceBarriersPart>(items, ContentTypes.FinanceBarriers);

            var model = new ProposalFinanceSummaryViewModel
            {
                FinanceTurnoverViewModel = new FinanceTurnoverViewModel
                {
                    TurnoverAmount = financeTurnoverPart.TurnoverAmount,
                    Day = financeTurnoverPart.Day,
                    Month = financeTurnoverPart.Month,
                    Year = financeTurnoverPart.Year,
                },
                FinanceBalanceSheetViewModel = new FinanceBalanceSheetViewModel
                {
                    BalanceSheetTotal = financeBalanceSheetPart.BalanceSheetTotal,
                    Day = financeBalanceSheetPart.Day,
                    Month = financeBalanceSheetPart.Month,
                    Year = financeBalanceSheetPart.Year,
                },
                FinanceRecoverVatViewModel = new FinanceRecoverVatViewModel
                {
                    AbleToRecover = financeRecoverVatPart.AbleToRecover
                },
                FinanceBarriersViewModel = new FinanceBarriersViewModel
                {
                    Placeholder1 = financeBarriersPart.Placeholder1,
                    Placeholder2 = financeBarriersPart.Placeholder2,
                    Placeholder3 = financeBarriersPart.Placeholder3
                }
            };

            return model;
        }

        private async Task<ProposalWrittenSummaryViewModel> GetApplicationWrittenSummaryModel()
        {
            var items = await _contentRepository.GetContentItemListFromBagPart(User.Identity.Name);

            var projectProposalPart = GetContentPart<ProjectProposalDetailsPart>(items, ContentTypes.ProjectProposalDetails);
            var projectExperiencePart = GetContentPart<ProjectExperiencePart>(items, ContentTypes.ProjectExperience);
            var applicationDocumentPart = GetContentPart<ApplicationDocumentPart>(items, ContentTypes.ApplicationDocument);

            var model = new ProposalWrittenSummaryViewModel()
            {
                ProjectProposalDetailsViewModel = new ProjectProposalDetailsViewModel
                {
                    InnovationImpactSummary = projectProposalPart.InnovationImpactSummary,
                    Day = projectProposalPart.Day,
                    Month = projectProposalPart.Month,
                    Year = projectProposalPart.Year,
                },
                ProjectExperienceViewModel = new ProjectExperienceViewModel
                {
                    ExperienceSummary = projectExperiencePart.ExperienceSummary,
                }
            };

            return model;
        }

        private string GetNextPageUrl(string contentType)
        {
            string nextPage = Request.Form["nextPage"].ToString();
            if(string.IsNullOrEmpty(nextPage))
            {
                var page = _navigation.GetNextPageByContentType(contentType);
                return page.Name;
            }

            return nextPage;
        }

        private async Task<ApplicationSummaryModel> GetApplicationSummaryModel()
        {

            var items = await _contentRepository.GetContentItemListFromBagPart(User.Identity.Name);
            var model = new ApplicationSummaryModel()
            {
                TotalSections = 12
            };

            UpdateModel<CompanyDetailsPart>(items, ContentTypes.CompanyDetails, model, Pages.CompanyDetails);
            UpdateModel<ProjectSummaryPart>(items, ContentTypes.ProjectSummary, model, Pages.ProjectSummary);
            UpdateModel<ProjectDetailsPart>(items, ContentTypes.ProjectDetails, model, Pages.ProjectDetails);
            UpdateModel<OrgFundingPart>(items, ContentTypes.OrgFunding, model, Pages.Funding);
            UpdateModel<ProjectProposalDetailsPart>(items, ContentTypes.ProjectProposalDetails, model, Pages.ProjectProposalDetails);
            UpdateModel<ProjectExperiencePart>(items, ContentTypes.ProjectExperience, model, Pages.ProjectExperience);

            var contentItem = items?.FirstOrDefault(item => item.ContentType == ContentTypes.ApplicationDocument);
            var applicationDocumentPart = contentItem?.ContentItem.As<ApplicationDocumentPart>();
            if (applicationDocumentPart != null)
            {
                if(!string.IsNullOrEmpty(applicationDocumentPart.ProjectPlan))
                {
                    model.TotalCompletedSections++;
                    model.CompletedSections = model.CompletedSections | Pages.ProjectPlanUpload;
                }
                if (!string.IsNullOrEmpty(applicationDocumentPart.ExperienceAndSkills))
                {
                    model.TotalCompletedSections++;
                    model.CompletedSections = model.CompletedSections | Pages.ProjectExperienceSkillsUpload;
                }
            }

            UpdateModel<FinanceTurnoverPart>(items, ContentTypes.FinanceTurnover, model, Pages.FinanceTurnover);
            UpdateModel<FinanceBalanceSheetPart>(items, ContentTypes.FinanceBalanceSheet, model, Pages.FinanceBalanceSheet);
            UpdateModel<FinanceRecoverVatPart>(items, ContentTypes.FinanceRecoverVat, model, Pages.FinanceRecoverVat);
            UpdateModel<FinanceBarriersPart>(items, ContentTypes.FinanceBarriers, model, Pages.FinanceBarriers);

            return model;
        }


        private void UpdateModel<T>(IEnumerable<ContentItem> contentItems, string contentToFilter, ApplicationSummaryModel model, Pages section) where T : ContentPart
        {
            var contentItem = contentItems?.FirstOrDefault(item => item.ContentType == contentToFilter);
            var contentPart = contentItem?.ContentItem.As<T>();

            if (contentPart != null)
            {
                model.TotalCompletedSections++;
                model.CompletedSections = model.CompletedSections | section;
            }
        }

        private T GetContentPart<T>(IEnumerable<ContentItem> contentItems, string contentToFilter) where T : ContentPart
        {
            var contentItem = contentItems?.FirstOrDefault(item => item.ContentType == contentToFilter);
            return contentItem?.ContentItem.As<T>();
        }

        private ViewResult GetViewModel(Page currentPage, Field field)
        {
            BaseModel model;
            switch (currentPage.FieldType)
            {
                case FieldType.gdsSingleRadioSelectOption:
                    model = new RadioSingleSelectModel();
                    return View("SingleRadioSelectInput", PopulateModel(currentPage, model, field));
                case FieldType.gdsTextBox:
                    model = new TextInputModel();
                    return View("TextInput", PopulateModel(currentPage, model, field));
                case FieldType.gdsCurrencyBox:
                    model = new CurrencyInputModel();
                    return View("CurrencyInput", PopulateModel(currentPage, model, field));
                case FieldType.gdsTextArea:
                    model = new TextAreaModel();
                    return View("TextArea", PopulateModel(currentPage, model, field));
                case FieldType.gdsDateBox:
                    model = PopulateModel(currentPage, new DateModel(), field);
                    var dateModel = (DateModel)model;
                    if (!string.IsNullOrEmpty(model.DataInput))
                    {
                        var inputDate = DateTime.Parse(model.DataInput);
                        dateModel.Day = inputDate.Day;
                        dateModel.Month = inputDate.Month;
                        dateModel.Year = inputDate.Year;
                    }
                    return View("DateInput", model);
                case FieldType.gdsYesorNoRadio:
                    model = new YesornoInputModel();
                    return View("YesornoInput", PopulateModel(currentPage, model, field));
                case FieldType.gdsMultiSelect:
                    model = PopulateModel(currentPage, new MultiSelectInputModel(), field);
                    var multiSelect = (MultiSelectInputModel)model;
                    if (!string.IsNullOrEmpty(model.DataInput))
                    {
                        var UserInputList = model.DataInput.Split(',').ToList();
                        multiSelect.UserInput = UserInputList;
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
                    return View("FileUpload", uploadmodel);
                default:
                    throw new Exception("Invalid field type");
                
            }
        }

        protected ViewResult PopulateViewModel(Page currentPage, BaseModel currentModel)
        {
            switch (currentPage.FieldType)
            {
                case FieldType.gdsTextBox:
                    return View("TextInput", PopulateModel(currentPage, currentModel));
                case FieldType.gdsTextArea:
                    return View("TextArea", PopulateModel(currentPage, currentModel));
                case FieldType.gdsDateBox:
                    return View("DateInput", PopulateModel(currentPage, currentModel));
                case FieldType.gdsYesorNoRadio:
                    return View("YesornoInput", PopulateModel(currentPage, currentModel));
                case FieldType.gdsMultiSelect:
                    return View("MultiSelectInput", PopulateModel(currentPage, currentModel));
                case FieldType.gdsFileUpload:
                    return View("FileUpload", PopulateModel(currentPage, currentModel));
                case FieldType.gdsCurrencyBox:
                    return View("CurrencyInput", PopulateModel(currentPage, currentModel));
                case FieldType.gdsSingleRadioSelectOption:
                    return View("SingleRadioSelectInput", PopulateModel(currentPage, currentModel));
                case FieldType.gdsAddressTextBox:
                    return View("AddressInput", PopulateModel(currentPage, currentModel));
                default:
                    throw new Exception("Invalid field type");
            }
        }

        private BaseModel PopulateModel(Page currentPage, BaseModel currentModel, Field field = null)
        {

            currentModel.Question = currentPage.Question;
            currentModel.TitleQuestion = currentPage.TitleQuestion;
            currentModel.PageName = currentPage.Name;
            currentModel.FieldName = currentPage.FieldName;
            currentModel.Hint = currentPage.Hint;
            currentModel.ShowMarkAsComplete = currentPage.ShowMarkComplete;
            currentModel.HasOtherOption = currentPage.HasOtherOption;
            currentModel.MaxLength = currentPage.MaxLength;
            currentModel.MaxLengthValidationType = currentPage.MaxLengthValidationType;
            currentModel.SelectedOptions = currentPage.SelectOptions;
            
            if (currentPage.ShowMarkComplete)
            {
                currentModel.MarkAsComplete = field?.MarkAsComplete != null ? field.MarkAsComplete.Value : false;
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


            var section = _applicationDefinition.Application.Sections.FirstOrDefault(section =>
                                         section.Pages.Any(page => page.Name == currentPage.Name));
            var index = section.Pages.FindIndex(p => p.Name.ToLower().Equals(currentPage.Name));

            currentModel.QuestionNumber = index + 1;
            currentModel.TotalQuestions = section.Pages.Count;

            for (int i = 0; i < currentModel.TotalQuestions; i++)
            {

            }
            if (string.IsNullOrEmpty(currentPage.ContinueButtonText))
            {
                currentModel.ContinueButtonText = section.ContinueButtonText;
            }else
            {
                currentModel.ContinueButtonText = currentPage.ContinueButtonText;
            }
            currentModel.ReturnToSummaryPageLinkText = section.ReturnToSummaryPageLinkText;
            currentModel.SectionUrl = section.Url;
            currentModel.SectionInfo = section;

            var currentPageIndex = section.Pages.FindIndex(p => p.Name == currentPage.Name);// (index - 1);
            if (currentPageIndex >= 1)
            {
                currentModel.PreviousPageName = section.Pages[currentPageIndex -1 ].Name;
            } 

            if (!string.IsNullOrEmpty(currentPage.Description))
            {
                currentModel.Description = currentPage.Description;
            }

            if (!string.IsNullOrEmpty(currentPage.UploadText))
            {
                currentModel.UploadText = currentPage.UploadText;
            }

            currentPage.DisplayQuestionCounter = currentPage.DisplayQuestionCounter;

            return currentModel;
        }

        private SectionContent GetSectionContent(ApplicationContent content, Section section)
        {
            var sectionContentModel = new SectionContent();
            sectionContentModel.TotalQuestions = section.Pages.Count;
            sectionContentModel.Sections = new List<SectionModel>();
            sectionContentModel.Title = section.Title;
            sectionContentModel.OverviewTitle = section.OverviewTitle;
            sectionContentModel.Url = section.Url;
            sectionContentModel.ReturnUrl = section.ReturnUrl;

            foreach (var pageContent in section.Pages)
            {
                var sectionModel = new SectionModel();
                sectionModel.Title = pageContent.Question;
                sectionModel.Url = pageContent.Name;

                var field = content?.Fields?.FirstOrDefault(f => f.Name.Equals(pageContent.FieldName));

                if (string.IsNullOrEmpty(field?.Data) && string.IsNullOrEmpty(field?.AdditionalInformation))
                {
                    sectionModel.SectionStatus = SectionStatus.NotStarted;
                }
                else
                {
                    bool markAsComplete = true;
                    if (pageContent.ShowMarkComplete)
                    {
                        markAsComplete = field?.MarkAsComplete != null ? field.MarkAsComplete.Value : false;
                    }
                    
                    if (markAsComplete)
                    {
                        sectionModel.SectionStatus = SectionStatus.Completed;
                        sectionContentModel.TotalQuestionsCompleted++;
                    }
                    else
                    {
                        sectionModel.SectionStatus = SectionStatus.InProgress;
                    }

                }
                sectionContentModel.Sections.Add(sectionModel);
            }

            return sectionContentModel;
        }
    }
}
