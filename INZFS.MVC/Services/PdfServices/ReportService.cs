using iText.Html2pdf;
using iText.Kernel.Pdf;
using INZFS.MVC;
using System;
using System.IO;
using System.Threading.Tasks;

public class ReportService : IReportService
{
    private string html;
    private string tableStyle = @"style=""margin-bottom:2rem; width:100%; border:none;""";
    private string questionTableStyle = @"style=""background-color:rgb(18,31,54); width:100%;""";
    private string questionHeaderStyle = @"style=""color:white; text-align:left;""";
    private string answerCellStyle = @"style=""border:1px solid grey""";

    private string coverPageTextColour = "rgb(28,28,28)";
    private string sectionTitleTextColour = "rgb(20,40,99)";

    private readonly ApplicationDefinition _applicationDefinition;
    private readonly IContentRepository _contentRepository;
    private ApplicationContent _applicationContent;
    
    public ReportService(IContentRepository contentRepository, ApplicationDefinition applicationDefinition)
    {
        _contentRepository = contentRepository;
        _applicationDefinition = applicationDefinition;
    }

    public async Task<byte[]> GeneratePdfReport(string applicationAuthor)
    {
        _applicationContent = await _contentRepository.GetApplicationContent(applicationAuthor);

        BuildHtmlString();

        using (MemoryStream stream = new())
        using (PdfWriter writer = new(stream))
        {
            HtmlConverter.ConvertToPdf(html, writer);
            return stream.ToArray();
        }
    }

    private void BuildHtmlString()
    {
        OpenHtmlString();
        PopulateHtmlSections();
        CloseHtmlString();
    }

    private void OpenHtmlString()
    {
        html = $@"
           <!DOCTYPE html>
           <html lang=""en"">
           <head>
           </head>
          <body style=""font-family: Helvetica, sans-serif;"">

            <div style=""height:265mm;"">
                <div style=""display:flex; justify-content:space-between;"">
                    <p style=""float: left;"">BEIS</p>
                    <p style=""width: 45mm; float:right;"">This document was downloaded on:<br><strong>{ DateTime.Now.ToString("dd MMMM yyyy HH:mm") }</strong></p>
                </div>
                <div style=""padding-left: 15mm; padding-right:15mm; padding-top: 15%;"">
                    <h1 style=""font-size:5rem;"">The Energy Entrepreneurs Fund (EEF)</h1>
                    <h2>Phase 9 Application Form</h2>
                    <p>This is a copy of your online application for the Energy Entrepreneurs Fund for your records</p>
                    <p>Your Application Reference is <strong>{ _applicationContent.ApplicationNumber }</strong></p> 
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

    private void PopulateHtmlSections()
    {
        foreach (var section in _applicationDefinition.Application.Sections)
        {
            string sectionHtml = $@"
                <h2 style=""color:rgb(28, 28, 28)"", id=""{section.Tag}"" >{ section.Title }</h2>
            ";
            html = html + sectionHtml;

            PopulateHtmlQuestions(section);
        }
    }

    private void PopulateHtmlQuestions(INZFS.MVC.Section section)
    {
        foreach (var page in section.Pages) if (!page.HideFromSummary)
        {
            String questionHtml = $@"
                <table { tableStyle }>
                  <tr { questionTableStyle }>
                    <th { questionHeaderStyle }>{ page.Question }</th>
                  </tr>
                  <tr>
                    <td { answerCellStyle }>{ GetAnswer(page) }</td>
                  </tr>
                </table>
                ";
            html = html + questionHtml;
        }
    }

    private string GetAnswer(Page page)
    {
        var answer = _applicationContent?.Fields.Find(question => question.Name == page.Name);

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
