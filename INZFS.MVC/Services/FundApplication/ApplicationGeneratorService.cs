using INZFS.MVC.Constants;
using INZFS.MVC.Extensions;
using INZFS.MVC.Models.DynamicForm;
using INZFS.MVC.Services.UserManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace INZFS.MVC.Services.FundApplication
{
    public class ApplicationGeneratorService : Controller, IApplicationGeneratorService
    {
        private readonly ApplicationDefinition _applicationDefinition;
        private readonly IContentRepository _contentRepository;
        private readonly IUserManagerService _userManagerService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDynamicFormGenerator _dynamicFormGenerator;
        public ApplicationGeneratorService(
            ApplicationDefinition applicationDefinition,
            IContentRepository contentRepository, 
            IUserManagerService userManagerService,
            IHttpContextAccessor httpContextAccessor, 
            IDynamicFormGenerator dynamicFormGenerator)
        {
            _applicationDefinition = applicationDefinition;
            _contentRepository = contentRepository;
            _userManagerService = userManagerService;
            _httpContextAccessor = httpContextAccessor;

        }

        public async Task<ApplicationContent> GetApplicationConent(string userId)
        {
            var content = await _contentRepository.GetApplicationContent(userId);
            return content;
        }

        public Section GetCurrenSection(string pageName)
        {
            var currentSection = _applicationDefinition.Application.Sections.FirstOrDefault(section => section.Url.Equals(pageName));
            return currentSection;

        }

        public Page GetCurrentPage(string pageName)
        {
            var currentPage = _applicationDefinition.Application.AllPages.FirstOrDefault(p => p.Name.ToLower().Equals(pageName));
            return currentPage;
        }

        public Field GetField(string fieldName, string userId)
        {
            var content = GetApplicationConent(userId).Result;
            var field = content?.Fields?.FirstOrDefault(f => f.Name.Equals(fieldName));
            return field;
        }

        public async Task<IActionResult> GetSection(string pagename, string id)
        {
            if (string.IsNullOrEmpty(pagename))
            {
                return NotFound();
            }
            pagename = pagename.ToLower().Trim();


            var userId = _userManagerService.GetUserId();
            var content = GetApplicationConent(userId).Result;
            if (content == null)
            {
                content = GetApplicationConent(userId).Result;
            }

            if (string.IsNullOrEmpty(_httpContextAccessor.HttpContext.User.Identity.ApplicationNumber()))
            {
                var claimIdentity = (ClaimsIdentity) _httpContextAccessor.HttpContext.User.Identity;
                claimIdentity.AddClaim(new System.Security.Claims.Claim("ApplicationNumber", content.ApplicationNumber));
            }

            if (RedirectToApplicationSubmittedPage(pagename, content))
            {
                return RedirectToAction("ApplicationSent");
            }

            // Page
            var currentPage = GetCurrentPage(pagename);
            if (currentPage != null)
            {
                var field = GetField(currentPage.FieldName, userId);
                return _dynamicFormGenerator.GetViewModel(currentPage, field);
            }

            //Overview
            if (pagename.ToLower() == "application-overview")
            {
                var applicationOverviewContentModel = GetApplicationOverviewContent(content);

                _dynamicFormGenerator.SetPageTitle("Application Overview");
                return View("ApplicationOverview", applicationOverviewContentModel);
            }

            // Section
            var currentSection = _applicationDefinition.Application.Sections.FirstOrDefault(section => section.Url.Equals(pagename));
            if (currentSection != null)
            {
                var sectionContentModel = GetSectionContent(content, currentSection);
                _dynamicFormGenerator.SetPageTitle(currentSection.Title);
                return View(currentSection.RazorView, sectionContentModel);
            }


            return NotFound();
        }
        private bool RedirectToApplicationSubmittedPage(string pageName, ApplicationContent content)
        {
            if (content.ApplicationStatus != ApplicationStatus.InProgress)
            {
                if (pageName.ToLower() == "application-overview")
                {
                    return true;
                }
                var currentSection = _applicationDefinition.Application.Sections.FirstOrDefault(section =>
                 section.Pages.Any(page => page.Name.ToLower() == pageName.ToLower()));

                if (currentSection == null)
                {
                    currentSection = _applicationDefinition.Application.Sections.FirstOrDefault(section => section.Url.ToLower().Equals(pageName.ToLower()));
                }
                if (currentSection?.BelongsToApplication == true)
                {
                    return true;
                }
            }

            return false;
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
                if (pageContent.HideFromSummary)
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
                    if (pageContent.CompletionDependsOn != null && pageContent.CompletionDependsOn.Value == field.Data)
                    {
                        // Get status of the dependant fields
                        bool areAlldependantFieldsComplete = false;
                        var dependantFields = content?.Fields?.Where(f => pageContent.CompletionDependsOn.Fields?.Contains(f.Name) == true);
                        if (dependantFields?.Any() == true)
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
       
        public ApplicationOverviewContent GetApplicationOverviewContent(ApplicationContent content)
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

    
    }
}
