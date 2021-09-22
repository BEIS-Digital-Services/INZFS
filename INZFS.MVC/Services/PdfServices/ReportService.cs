using Aspose.Words;
using INZFS.MVC;
using System;
using System.IO;
using System.Threading.Tasks;


namespace INZFS.MVC.Services.PdfServices
{
    public class ReportService : IReportService
    {
        private string html;

        private string tableStyle = @"style=""margin-bottom:2rem; width:100%; border:none;""";
        private string questionTableStyle = @"style=""background-color:rgb(18,31,54); width:100%;""";
        private string questionHeaderStyle = @"style=""color:white; text-align:left; padding:10px;""";
        private string answerCellStyle = @"style=""border:1px solid grey; padding:10px;""";

        private string coverPageTextColour = @"style=""rgb(28,28,28)""";
        private string sectionTitleTextColour = @"style=""rgb(20,40,99)""";

        private readonly ApplicationDefinition _applicationDefinition;
        private readonly IContentRepository _contentRepository;

        public ReportService(IContentRepository contentRepository, ApplicationDefinition applicationDefinition)
        {
            _contentRepository = contentRepository;
            _applicationDefinition = applicationDefinition;

            try
            {
                License license = new();
                license.SetLicense("Aspose.Words.NET.lic");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while applying Aspose.Words Licence: " + e.Message);
            }
        }

        public async Task<ReportContent> GeneratePdfReport(string applicationAuthor)
        {
            var applicationContent = await _contentRepository.GetApplicationContent(applicationAuthor);
            var reportContent = new ReportContent
            {
                ApplicationNumber = applicationContent.ApplicationNumber
            };

            BuildHtmlString(applicationContent);

            using (MemoryStream stream = new())
            {
                Document doc = new();
                DocumentBuilder builder = new(doc);

                builder.InsertHtml(html);

                doc.Save(stream, SaveFormat.Pdf);
                reportContent.FileContents = stream.ToArray();
                return reportContent;
            }
        }

        public async Task<ReportContent> GenerateOdtReport(string applicationAuthor)
        {
            var applicationContent = await _contentRepository.GetApplicationContent(applicationAuthor);
            var reportContent = new ReportContent
            {
                ApplicationNumber = applicationContent.ApplicationNumber
            };
            
            BuildHtmlString(applicationContent);

            using (MemoryStream stream = new())
            {
                Document doc = new();
                DocumentBuilder builder = new(doc);

                builder.InsertHtml(html);

                doc.Save(stream, SaveFormat.Odt);
                reportContent.FileContents = stream.ToArray();
                return reportContent;
            }
        }

        //To reimplement this method, Nuget packages DocumentFormat.OpenXml and NS.HtmlToOpenXml are required
        //public async Task<byte[]> GenerateDocXReport(string applicationAuthor)
        //{
        //    _applicationContent = await _contentRepository.GetApplicationContent(applicationAuthor);

        //    BuildHtmlString();

        //    using (MemoryStream stream = new())
        //    {
        //        using (WordprocessingDocument package = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        //        {
        //            MainDocumentPart mainPart = package.MainDocumentPart;
        //            if (mainPart == null)
        //            {
        //                mainPart = package.AddMainDocumentPart();
        //                new Document(new Body()).Save(mainPart);
        //            }

        //            HtmlToOpenXml.HtmlConverter converter = new HtmlToOpenXml.HtmlConverter(mainPart);
        //            converter.ParseHtml(html);
        //            mainPart.Document.Save();
        //        }
        //        return stream.ToArray();
        //    }
        //}
        private void BuildHtmlString(ApplicationContent applicationContent)
        {
            OpenHtmlString(applicationContent.ApplicationNumber);
            PopulateHtmlSections(applicationContent);
            CloseHtmlString();
        }

        private void OpenHtmlString(string applicationNumber)
        {
            html = $@"
           <!DOCTYPE html>
           <html lang=""en"">
           <head>
           </head>
          <body style=""font-family: Helvetica, sans-serif;"">

            <div style=""height:265mm;"">
                <p style=""float: left; color:rgb(28,28,28);"">BEIS</p>
                <p style=""width: 45mm; float:right; text-align: right; color:rgb(28,28,28);"">This document was downloaded on:<br><strong>{ DateTime.Now.ToString("dd MMMM yyyy HH:mm") }</strong></p>
                <div style=""padding-left: 15mm; padding-right:15mm; margin-top: 65mm;"">
                    <h1 style=""font-size:5rem; color:rgb(28,28,28);"">The Energy Entrepreneurs Fund (EEF)</h1>
                    <h2 style=""color:rgb(28,28,28);"">Phase 9 Application Form</h2>
                    <p style=""color:rgb(28,28,28);"">This is a copy of your online application for the Energy Entrepreneurs Fund for your records</p>
                    <p style=""color:rgb(28,28,28);"">Your Application Reference is <strong>{ applicationNumber }</strong></p> 
                </div>
            </div>

            <div style=""height:265mm;"">
                <h2>Contents</h2>

                <h3>Your information</h3>
                <a href=""#your-details-and-eligibility"" style=""display:block; margin-bottom: 1rem;"">1. Your details and eligibility</a>
                <a href=""#summary-of-finances"" style=""display:block; margin-bottom: 1rem;"">2. Summary of finances</a>
                <a href=""#your-organisation"" style=""display:block; margin-bottom: 1rem;"">3. About your organisation (and partners)</a>

                <h3>Your proposal</h3>
                <a href=""#proposal-summary"" style=""display:block; margin-bottom: 1rem;"">4. Proposal summary</a>
                <a href=""#business-proposal"" style=""display:block; margin-bottom: 1rem;"">5. Business proposal, project plans and risk</a>
                <a href=""#finance-proposal"" style=""display:block; margin-bottom: 1rem;"">6. Financial proposal and costs</a>

                <h3>Un-assessed data</h3>
                <a href=""#performance-data"" style=""display:block; margin-bottom: 1rem;"">7. Performance Data</a>

            </div>
          ";
        }

        private void CloseHtmlString()
        {
            html = html + "</body></html>";
        }

        private void PopulateHtmlSections(ApplicationContent applicationContent)
        {
            foreach (var section in _applicationDefinition.Application.Sections)
            {
                string sectionHtml = $@"
                <h2 style=""color:rgb(18,31,54);"", id=""{section.Tag}"" >{ section.OverviewTitle }</h2>
            ";
                html = html + sectionHtml;

                PopulateHtmlQuestions(section, applicationContent);
            }
        }

        private void PopulateHtmlQuestions(INZFS.MVC.Section section, ApplicationContent applicationContent)
        {
            foreach (var page in section.Pages) if (!page.HideFromSummary)
                {
                    String questionHtml = $@"
                <table { tableStyle }>
                  <tr { questionTableStyle }>
                    <th { questionHeaderStyle }>{ page.SectionTitle }</th>
                  </tr>
                  <tr>
                    <td { answerCellStyle }>{ GetAnswer(page, applicationContent) }</td>
                  </tr>
                </table>
                ";
                    html = html + questionHtml;
                }
        }

        private string GetAnswer(Page page, ApplicationContent applicationContent)
        {
            var answer = applicationContent?.Fields.Find(question => question.Name == page.Name);

            if (answer == null)
            {
                return @"<span style=""color:red;"">No response</span>";
            }
            else
            {
                return answer.Data;
            }
        }
    }
}
    
