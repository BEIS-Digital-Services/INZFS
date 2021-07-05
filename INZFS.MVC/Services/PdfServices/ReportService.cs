using INZFS.MVC;
using INZFS.MVC.Controllers;
using INZFS.MVC.Forms;
using INZFS.MVC.Models;
using INZFS.MVC.Models.ProposalWritten;
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

    private CompanyDetailsPart companyDetails;
    private ProjectSummaryPart projectSummary;
    private ProjectDetailsPart projectDetails;
    private OrgFundingPart orgFunding;
    private ProjectProposalDetailsPart projectProposalDetails;

    public ReportService(IContentRepository contentRepository)
    {
        _contentRepository = contentRepository;
    }

    public async Task<byte[]> GeneratePdfReport(string applicationId)
    {
        var application = await _contentRepository.GetContentItemById(applicationId);
        var bagPart = application?.ContentItem?.As<BagPart>();
        var contents = bagPart?.ContentItems;

        PopulateData(contents);

        var html = $@"
           <!DOCTYPE html>
           <html lang=""en"">
           <head>
           </head>
          <body>
            <h1>EEF 8A Application Form </h1>

            <h2>Proposal Summary</h2>

            <table>
              <tr>
                <th>Q1. 1. Name of Bidder (this should be the lead organisation/co-ordinator for the proposed project)</th>
              </tr>
              <tr>
                <td>{ companyDetails?.CompanyName }</td>
              </tr>
            </table>

            <table>
              <tr>
                <th>Q2. 2. Project Name</th>
              </tr>
              <tr>
                <td>{ projectSummary?.ProjectName }</td>
              </tr>
            </table>

            <table>
              <tr>
                <th>Q3. 3. Estimated Start Date</th>
              </tr>
              <tr>
                <td>{ projectSummary?.Day }/{ projectSummary?.Month }/{ projectSummary?.Year }</td>
              </tr>
            </table>

            <table>
              <tr>
                <th>Q4. 4. Project Duration (months)</th>
              </tr>
              <tr>
                <td></td>
              </tr>
            </table>

            <table>
              <tr>
                <th>Q5. 5. Estimated End Date</th>
              </tr>
              <tr>
                <td>{ projectProposalDetails?.Day }/{ projectProposalDetails?.Month }/{ projectProposalDetails?.Year }</td>
              </tr>
            </table>


          </body>
          </html>
          ";

        using (MemoryStream stream = new())
        using (PdfWriter writer = new(stream))
        {
            HtmlConverter.ConvertToPdf(html, writer);
            return stream.ToArray();
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

                case ContentTypes.ProjectSummary:
                    projectSummary = contentItem.ContentItem.As<ProjectSummaryPart>();
                    break;

                case ContentTypes.ProjectDetails:
                    projectDetails = contentItem.ContentItem.As<ProjectDetailsPart>();
                    break;

                case ContentTypes.OrgFunding:
                    orgFunding = contentItem.ContentItem.As<OrgFundingPart>();
                    break;

                case ContentTypes.ProjectProposalDetails:
                    projectProposalDetails = contentItem.ContentItem.As<ProjectProposalDetailsPart>();
                    break;


                default:
                    Debug.WriteLine("Switch statement unable to find content type");
                    break;
            }
        }
    }
}
