using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Settings;
using YesSql;
using Microsoft.AspNetCore.Authorization;
using INZFS.MVC.Models;
using INZFS.MVC.Forms;
using INZFS.MVC.Models.ProposalWritten;
using INZFS.MVC.ViewModels.ProposalWritten;
using System.IO;
using Microsoft.AspNetCore.Http;
using OrchardCore.Media;
using Microsoft.Extensions.Logging;
using System.Globalization;
using INZFS.MVC.Drivers;
using System.Linq.Expressions;
using INZFS.MVC.Models.ProposalFinance;
using INZFS.MVC.ViewModels.ProposalFinance;
using OrchardCore.Flows.Models;
using Newtonsoft.Json.Linq;
using INZFS.MVC.Models.DynamicForm;
using INZFS.MVC.Records;
using INZFS.MVC.Services.FileUpload;
using INZFS.MVC.Services.VirusScan;

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

            // Section
            var section = _applicationDefinition.Application.Sections.FirstOrDefault(section => section.Url.Equals(pagename));
            if (section != null)
            {
                var sectionContentModel = new SectionContent();
                sectionContentModel.TotalQuestions = section.Pages.Count;
                sectionContentModel.Sections = new List<SectionModel>();
                var content = await _contentRepository.GetApplicationContent(User.Identity.Name);

                foreach (var pageContent in section.Pages)
                {
                    var sectionModel = new SectionModel();
                    sectionModel.Title = pageContent.Question;
                    sectionModel.Url = pageContent.Name;

                    var field = content?.Fields?.FirstOrDefault(f => f.Name.Equals(pageContent.FieldName));
                    
                    if (string.IsNullOrEmpty(field?.Data))
                    {
                        sectionModel.Status = "Not started";
                    }
                    else
                    {
                        
                        if (field.MarkAsComplete.HasValue && field.MarkAsComplete.Value == true)
                        {
                            sectionModel.Status = "Completed";
                            sectionContentModel.TotalQuestionsCompleted ++;
                        }
                        
                        else
                        {
                            sectionModel.Status = "In Progress";
                        }
                        
                    }


                    sectionContentModel.Sections.Add(sectionModel);

                }

                return View(section.RazorView, sectionContentModel);
            }

            if (pagename == "application-overview")
            {
                return View("ApplicationOverview");
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
        public async Task<IActionResult> Save([Bind(Prefix = "submit.Publish")] string submitPublish, string returnUrl, string pageName, IFormFile? file, BaseModel model)
        {
            if (ModelState.IsValid)
            {
                var contentToSave = await _contentRepository.GetApplicationContent(User.Identity.Name);
                if (contentToSave == null)
                {
                    contentToSave = new ApplicationContent();
                    contentToSave.Application = new Application();
                    contentToSave.Author = User.Identity.Name;
                    contentToSave.CreatedUtc = DateTime.UtcNow;
                    //newContent.Application.Sections = new Sections();
                    //newContent.Application.Sections.Pages = new List<Page>();
                }

                contentToSave.ModifiedUtc = DateTime.UtcNow;

                var field = _applicationDefinition.Application.AllPages.FirstOrDefault(p => p.Name.ToLower().Equals(pageName));

                var existingFieldData = contentToSave.Fields.FirstOrDefault(f => f.Name.Equals(field.FieldName));
                if(existingFieldData == null)
                {
                    contentToSave.Fields.Add(new Field { 
                        Name = field.FieldName, 
                        Data = model.GetData(),
                        MarkAsComplete =model.ShowMarkAsComplete ? model.MarkAsComplete : null
                    });
                }
                else
                {
                    existingFieldData.Data = model.GetData();
                    existingFieldData.MarkAsComplete = model.ShowMarkAsComplete ? model.MarkAsComplete : null;
                }
                

                _session.Save(contentToSave);

                var index = _applicationDefinition.Application.AllPages.FindIndex(p => p.Name.ToLower().Equals(pageName));
                var nextPage = _applicationDefinition.Application.AllPages.ElementAtOrDefault(index + 1);
                if(nextPage == null)
                {
                    return NotFound();
                }
                //TODO: Check of non-existing pages
                // check for the last page
                return RedirectToAction("section", new { pagename = nextPage.Name });
            }
            else
            {
                _session.Cancel();
                var currentPage = _applicationDefinition.Application.AllPages.FirstOrDefault(p => p.Name.ToLower().Equals(pageName));
                return PopulateViewModel(currentPage, model);
            }

            
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
                _session.Cancel();
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
                _session.Cancel();
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
                case FieldType.gdsTextBox:
                    model = new TextInputModel();
                    return View("TextInput", PopulateModel(currentPage, model, field));
                case FieldType.gdsTextArea:
                    model = new TextAreaModel();
                    return View("TextArea", PopulateModel(currentPage, model, field));
                case FieldType.gdsDateBox:
                    model = PopulateModel(currentPage, new DateModel(), field);
                    var inputDate = DateTime.Parse(model.DataInput);
                    var dateModel = (DateModel)model;
                    dateModel.Day = inputDate.Day;
                    dateModel.Month = inputDate.Month;
                    dateModel.Year = inputDate.Year;
                    

                    return View("DateInput", model);
                case FieldType.gdsSingleLineRadio:
                    model = new SingleRadioInputModel();
                    return View("SingleRadioInput", PopulateModel(currentPage, model, field));
                case FieldType.gdsMultiSelect:
                    model = new MultiSelectInputModel();
                    return View("MultiSelectInput", PopulateModel(currentPage, model, field));
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
                case FieldType.gdsSingleLineRadio:
                    return View("SingleRadioInput", PopulateModel(currentPage, currentModel));
                case FieldType.gdsMultiSelect:
                    return View("MultiSelectInput", PopulateModel(currentPage, currentModel));
                default:
                    throw new Exception("Invalid field type");
            }
        }

        private BaseModel PopulateModel(Page currentPage, BaseModel currentModel, Field field = null)
        {
            
            currentModel.Question = currentPage.Question;
            currentModel.PageName = currentPage.Name;
            currentModel.FieldName = currentPage.FieldName;
            currentModel.Hint = currentPage.Hint;
            currentModel.ShowMarkAsComplete = currentPage.ShowMarkComplete;
            currentModel.MaxLength = currentPage.MaxLength;
            if (currentPage.ShowMarkComplete)
            {
                currentModel.MarkAsComplete = field?.MarkAsComplete != null ? field?.MarkAsComplete.Value : false;
            }

            if (!string.IsNullOrEmpty(field ?.Data))
            {
                currentModel.DataInput = field?.Data;
            }
            var index = _applicationDefinition.Application.AllPages.FindIndex(p => p.Name.ToLower().Equals(currentPage.Name));

            
            var section = _applicationDefinition.Application.Sections.FirstOrDefault(section =>
                                         section.Pages.Any(page => page.Name == currentPage.Name));
            currentModel.QuestionNumber = index + 1;
            currentModel.TotalQuestions = section.Pages.Count;
            currentModel.ContinueButtonText = section.ContinueButtonText;
            currentModel.ReturnToSummaryPageLinkText = section.ReturnToSummaryPageLinkText;
            currentModel.SectionUrl = section.Url;
            
            var previousPage = _applicationDefinition.Application.AllPages.ElementAtOrDefault(index - 1);
            if (previousPage != null)
            {
                currentModel.PreviousPageName = previousPage.Name;
            }
           
            return currentModel;
        }
    }
}
