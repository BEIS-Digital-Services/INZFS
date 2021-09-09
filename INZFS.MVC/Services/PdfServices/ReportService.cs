using INZFS.MVC;
using INZFS.MVC.Models;
using iText.Html2pdf;
using iText.Kernel.Pdf;
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

        OpenHtmlString();
        PopulateHtmlSections();
        CloseHtmlString();

        using (MemoryStream stream = new())
        using (PdfWriter writer = new(stream))
        {
            HtmlConverter.ConvertToPdf(html, writer);
            return stream.ToArray();
        }
    }

    private void OpenHtmlString()
    {
        html = $@"
           <!DOCTYPE html>
           <html lang=""en"">
           <head>
           </head>
          <body>
            <h1 style=""text-align:center;"">EEF 8A Application Form</h1>
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
                <h2 style=""color:rgb(28, 28, 28)"" >{ section.Title }</h2>
            ";
            html = html + sectionHtml;

            PopulateHtmlQuestions(section);
        }
    }

    private void PopulateHtmlQuestions(Section section)
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
