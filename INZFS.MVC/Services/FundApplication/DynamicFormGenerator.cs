using INZFS.MVC.Models.DynamicForm;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace INZFS.MVC.Services.FundApplication
{
    public class DynamicFormGenerator : Controller, IDynamicFormGenerator
    {
        private readonly ApplicationDefinition _applicationDefinition;
        public DynamicFormGenerator(ApplicationDefinition applicationDefinition)
        {
            _applicationDefinition = applicationDefinition;
        }
        public BaseModel PopulateModel(Page currentPage, BaseModel currentModel, Field field = null)
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

            if (!string.IsNullOrEmpty(field?.Data))
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
            }
            else
            {
                currentModel.ContinueButtonText = currentPage.ContinueButtonText;
            }
            currentModel.ReturnToSummaryPageLinkText = currentSection.ReturnToSummaryPageLinkText;
            currentModel.SectionUrl = currentSection.Url;
            currentModel.SectionInfo = currentSection;

            var currentPageIndex = currentSection.Pages.FindIndex(p => p.Name == currentPage.Name);
            if (currentPageIndex >= 1)
            {
                currentModel.PreviousPageName = currentPage.PreviousPageName ?? currentSection.Pages[currentPageIndex - 1].Name;
            }


            if (!string.IsNullOrEmpty(currentPage.Description))
            {
                currentModel.Description = currentPage.Description;
            }

            currentModel.DisplayQuestionCounter = currentPage.DisplayQuestionCounter;
            currentModel.GridDisplayType = currentPage.GridDisplayType == null ? currentSection.GridDisplayType : currentPage.GridDisplayType.Value;
            return currentModel;
        }

        public ViewResult PopulateViewModel(Page currentPage, BaseModel currentModel, Field field = null)
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

        public ViewResult GetViewModel(Page currentPage, Field field)
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
                    if (!string.IsNullOrEmpty(field?.AdditionalInformation))
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
        public void SetPageTitle(string title)
        {
            ViewData["Title"] = $"{title}";
        }
    }
}
