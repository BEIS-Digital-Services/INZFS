using INZFS.MVC.Records;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC
{
    public enum FieldType
    {
        gdsTextBox,
        gdsTextArea,
        gdsDateBox,
        gdsYesorNoRadio,
        gdsMultiLineRadio,
        gdsMultiSelect,
        gdsFileUpload,
        gdsCurrencyBox,
        gdsSingleRadioSelectOption,
        gdsAddressTextBox,
        gdsStaticPage
    }
    public enum TextType
    {
        Standard,
        CurrencyBox,
        NumberBox
    }

    public enum YesNoType
    {
        Standard,
        CustomInput
    }
    public enum MaxLengthValidationType
    {
        Character,
        Word
    }

    public enum GridDisplayType
    {
        TwoThird,
        FullPage
    }

    public class Page
    {
        public string Name { get; set; }
        public string Question { get; set; }
        public bool DisplayQuestionCounter { get; set; } = true;
        public GridDisplayType GridDisplayType { get; set; }
        public string  SectionTitle { get; set; }
        public string Description { get; set; }
        public string NextPageName { get; set; }
        public string ReturnPageName { get; set; }
        public string Hint { get; set; }
        //[JsonProperty("error-massage")]
        public string ErrorMessage { get; set; }
        public FieldType FieldType { get; set; }
        public TextType TextType { get; set; }
        public YesNoType YesNoInput { get; set; }
    public string FieldName { get; set; }
        public string FriendlyFieldName { get; set; }
        public bool Mandatory { get; set; }

        //[JsonProperty("accordian-reference")]
        public string AccordianReference { get; set; }

        //[JsonProperty("max-length")]
        public int? MaxLength { get; set; }
        public bool HasOtherOption { get; set; }
        public bool ShowMarkComplete { get; set; }
        public string ReturnToSummaryPageLinkText { get; set; }
        public string ContinueButtonText { get; set; }
        public string FileToDownload { get; set; }
        public MaxLengthValidationType MaxLengthValidationType { get; set; }
        public List<Action> Actions { get; set; }
        public List<string> SelectOptions { get; set; }
        public bool HideFromSummary { get; set; }
        public PreviousPage PreviousPage { get; set; }
        public DependsOn DependsOn { get; set; }
        public string CustomValidator { get; set; }
        public string AcceptableFileExtensions { get; set; }
        
    }

    public class DependsOn
    {
        public string FieldName { get; set; }
        public string Value { get; set; }
    }

    public class Section
    {
        public Section()
        {
            BelongsToApplication = true;
        }
        public string Title { get; set; }
        public string OverviewTitle { get; set; }
        public string Tag { get; set; }
        public string Url { get; set; }
        public string ReturnUrl { get; set; }
        public string ReturnToSummaryPageLinkText { get; set; }
        public string ContinueButtonText { get; set; }
        public string RazorView { get; set; }
        public bool HideQuestionCounter { get; set; }
        public bool BelongsToApplication { get; set; }
        public bool HideBreadCrumbs { get; set; }
        public List<Page> Pages { get; set; }
    }

    public class Application
    {
        public string Name { get; set; }
        public List<Section> Sections { get; set; }

        public List<Page> AllPages => Sections?.SelectMany(c => c.Pages).ToList();
    }

    public class ApplicationDefinition
    {
        public Application Application { get; set; }
    }

    public enum ApplicationStatus
    {
        InProgress,
        Submitted,
        Assessment,
        IndependentAssessment,
        Successful,
        Unsuccessful,
        Withdrawn
    }

    public class ApplicationContent
    {
        public ApplicationContent()
        {
            Fields = new List<Field>();
        }
        public Application Application { get; set; }

        //
        // Summary:
        //     The primary key in the database.
        public int Id { get; set; }
        //
        // Summary:
        //     The logical identifier of the content item across versions.
        public string ContentItemId { get; set; }
        //
        // Summary:
        //     The logical identifier of the versioned content item.
        public string ContentItemVersionId { get; set; }
        //
        // Summary:
        //     The content type of the content item.
        public string ContentType { get; set; }
        //
        // Summary:
        //     Whether the version is published or not.
        public bool Published { get; set; }
        //
        // Summary:
        //     Whether the version is the latest version of the content item.
        public bool Latest { get; set; }
        //
        // Summary:
        //     When the content item version has been updated.
        public DateTime? ModifiedUtc { get; set; }
        //
        // Summary:
        //     When the content item has been published.
        public DateTime? PublishedUtc { get; set; }
        //
        // Summary:
        //     When the content item has been created or first published.
        public DateTime? CreatedUtc { get; set; }
        //
        // Summary:
        //     The user id of the user who first created this content item version.
        public string Owner { get; set; }
        //
        // Summary:
        //     The name of the user who last modified this content item version.
        public string Author { get; set; }
        //
        // Summary:
        //     The text representing this content item.
        public string DisplayText { get; set; }

        public List<Field> Fields { get; set; }

        public ApplicationStatus ApplicationStatus { get; set; }
        public string ApplicationNumber { get; set; }
        public DateTime? SubmittedUtc { get; set; }

    }

    public enum FieldStatus
    {
        NotStarted,
        InProgress,
        Completed,
        NotApplicable
    }

    public class Field
    {
        public string Name { get; set; }
        public string Data { get; set; }
        public string OtherOption { get; set; }
        public string AdditionalInformation { get; set; }
        public bool? MarkAsComplete { get; set; }
        public FieldStatus? FieldStatus { get; set; }
    }

    public class UploadedFile
    {
        public string Name { get; set; }
        public string FileLocation { get; set; }
        public string Size { get; set; }
        public ParsedExcelData ParsedExcelData { get; set; }
    }

    public class ParsedExcelData
    {
        public string? ParsedTotalProjectCost { get; set; }
        public string? ParsedTotalGrantFunding { get; set; }
        public string? ParsedTotalGrantFundingPercentage { get; set; }
        public string? ParsedTotalMatchFunding { get; set; }
        public string? ParsedTotalMatchFundingPercentage { get; set; }
    }
    public class Action
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string PageName { get; set; }
    }
    public class PreviousPage
    {
        public string link { get; set; }
        public string text { get; set; }
    }

}
