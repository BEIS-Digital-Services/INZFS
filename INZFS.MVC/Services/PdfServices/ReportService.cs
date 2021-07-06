using INZFS.MVC;
using INZFS.MVC.Controllers;
using INZFS.MVC.Forms;
using INZFS.MVC.Models;
using INZFS.MVC.Services;
using INZFS.MVC.Models.ProposalWritten;
using INZFS.MVC.Models.ProposalFinance;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Flows.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

public class ReportService : IReportService
{
    private IContentRepository _contentRepository;

    private String html;
    private String tableStyle = @"style=""margin-bottom:2rem; width:100%; border:1px solid grey;""";
    private String questionTableStyle = @"style=""background-color:rgb(248,241,220); width:100%; border:1px solid grey;""";
    private String questionHeaderStyle = @"style=""text-align:left;""";

    private CompanyDetailsPart companyDetails;
    private ApplicationDefinition _applicationDefinition;
    private ApplicationContent _applicationContent;

    public ReportService(IContentRepository contentRepository, ApplicationDefinition applicationDefinition)
    {
        _contentRepository = contentRepository;
        _applicationDefinition = applicationDefinition;
    }

    public async Task<byte[]> GeneratePdfReport(string applicationId)
    {
        var application = await _contentRepository.GetContentItemById(applicationId);
        var bagPart = application?.ContentItem?.As<BagPart>();
        var contents = bagPart?.ContentItems;

        _applicationContent = _contentRepository.GetApplicationContent(application.Author).Result;

        PopulateData(contents);

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

            <h2>Proposal Summary</h2>

            <table { tableStyle }>
              <tr { questionTableStyle }>
                <th { questionHeaderStyle }>Q1. 1. Name of Bidder (this should be the lead organisation/co-ordinator for the proposed project)</th>
              </tr>
              <tr>
                <td>{ companyDetails?.CompanyName } </td>
              </tr>
            </table>
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
                <h2>{ section.Title }</h2>
            ";
            html = html + sectionHtml;

            PopulateHtmlQuestions(section);
        }
    }

    private void PopulateHtmlQuestions(Section section)
    {
        foreach (var page in section.Pages)
        {
            String questionHtml = $@"
                <table { tableStyle }>
                  <tr { questionTableStyle }>
                    <th { questionHeaderStyle }>{ page.Question }</th>
                  </tr>
                  <tr>
                    <td>{ getAnswerString(page) }</td>
                  </tr>
                </table>
                ";
            html = html + questionHtml;
        }
    }

    private String getAnswerString(INZFS.MVC.Page page)
    {
        var answer = _applicationContent.Fields.Find(question => question.Name == page.Name);

        if (answer == null)
        {
            return "No response";
        }
        else 
        {
            return answer.Data;
        }
    }

    private void PopulateData(System.Collections.Generic.List<ContentItem> contents)
    {
        foreach(var contentItem in contents)
        {
            switch (contentItem.ContentType)
            {
                case ContentTypes.CompanyDetails:
                    companyDetails = contentItem.ContentItem.As<CompanyDetailsPart>();
                    break;

                default:
                    Debug.WriteLine("Switch statement unable to find content type");
                    break;
            }
        }
    }
}
