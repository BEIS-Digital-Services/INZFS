using INZFS.MVC;
using INZFS.MVC.Controllers;
using INZFS.MVC.Forms;
using INZFS.MVC.Models;
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

    private CompanyDetailsPart companyDetails;
    private ProjectSummaryPart projectSummary;
    private ProjectDetailsPart projectDetails;
    private OrgFundingPart orgFunding;
    private ProjectProposalDetailsPart projectProposalDetails;
    private FinanceBalanceSheetPart financeBalanceSheet;
    private FinanceBarriersPart financeBarriers;
    private FinanceRecoverVatPart financeRecoverVat;
    private FinanceTurnoverPart financeTurnover;

    public ReportService(IContentRepository contentRepository)
    {
        _contentRepository = contentRepository;
    }

    public async Task<byte[]> GeneratePdfReport(string applicationId)
    {
        var application = await _contentRepository.GetContentItemById(applicationId);
        var bagPart = application?.ContentItem?.As<BagPart>();
        var contents = bagPart?.ContentItems;

        var tableStyle = @"style=""margin-bottom:2rem; width:100%; border:1px solid grey;""";
        var questionTableStyle = @"style=""background-color:rgb(248,241,220); width:100%; border:1px solid grey;""";
        var questionHeaderStyle = @"style=""text-align:left;""";

        PopulateData(contents);

        var html = $@"
           <!DOCTYPE html>
           <html lang=""en"">
           <head>
           </head>
          <body>
            <h1 style=""text-align:center;"">EEF 8A Application Form </h1>

            <h2>Proposal Summary</h2>

            <table { tableStyle }>
              <tr { questionTableStyle }>
                <th { questionHeaderStyle }>Q1. 1. Name of Bidder (this should be the lead organisation/co-ordinator for the proposed project)</th>
              </tr>
              <tr>
                <td>{ companyDetails?.CompanyName } </td>
              </tr>
            </table>

            <table { tableStyle }>
              <tr { questionTableStyle }>
                <th { questionHeaderStyle }>Q2. 2. Project Name</th>
              </tr>
              <tr>
                <td>{ projectSummary?.ProjectName }</td>
              </tr>
            </table>

            <table { tableStyle }>
              <tr { questionTableStyle }>
                <th { questionHeaderStyle }>Q3. 3. Estimated Start Date</th>
              </tr>
              <tr>
                <td>{ projectSummary?.Day }/{ projectSummary?.Month }/{ projectSummary?.Year }</td>
              </tr>
            </table>

            <table { tableStyle }>
              <tr { questionTableStyle }>
                <th { questionHeaderStyle }>Q4. 4. Project Duration (months)</th>
              </tr>
              <tr>
                <td></td>
              </tr>
            </table>

            <table { tableStyle }>
              <tr { questionTableStyle }>
                <th { questionHeaderStyle }>Q5. 5. Estimated End Date</th>
              </tr>
              <tr>
                <td>{ projectProposalDetails?.Day }/{ projectProposalDetails?.Month }/{ projectProposalDetails?.Year }</td>
              </tr>
            </table>

            <h2>Proposal (Finance)</h2>

            <table { tableStyle }>
              <tr { questionTableStyle }>
                <th { questionHeaderStyle }>Turnover amount (in most recent annual accounts)</th>
              </tr>
              <tr>
                <td>{ financeTurnover?.TurnoverAmount }</td>
              </tr>
            </table>

            <table { tableStyle }>
              <tr { questionTableStyle }>
                <th { questionHeaderStyle }>Turnover date</th>
              </tr>
              <tr>
                <td>{ financeTurnover?.Day }/{ financeTurnover?.Month }/{ financeTurnover?.Year }</td>
              </tr>
            </table>

            <table { tableStyle }>
              <tr { questionTableStyle }>
                <th { questionHeaderStyle }>Balance sheet total</th>
              </tr>
              <tr>
                <td>{ financeBalanceSheet?.BalanceSheetTotal }</td>
              </tr>
            </table>

            <table { tableStyle }>
              <tr { questionTableStyle }>
                <th { questionHeaderStyle }>Balance sheet date</th>
              </tr>
              <tr>
                <td>{ financeBalanceSheet?.Day }/{ financeBalanceSheet?.Month }/{ financeBalanceSheet?.Year }</td>
              </tr>
            </table>

            <table { tableStyle }>
              <tr { questionTableStyle }>
                <th { questionHeaderStyle }>Is the organisation able to recover VAT?</th>
              </tr>
              <tr>
                <td>{ financeRecoverVat.AbleToRecover }</td>
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

                case ContentTypes.FinanceBalanceSheet:
                    financeBalanceSheet = contentItem.ContentItem.As<FinanceBalanceSheetPart>();
                    break;

                case ContentTypes.FinanceBarriers:
                    financeBarriers = contentItem.ContentItem.As<FinanceBarriersPart>();
                    break;

                case ContentTypes.FinanceRecoverVat:
                    financeRecoverVat = contentItem.ContentItem.As<FinanceRecoverVatPart>();
                    break;

                case ContentTypes.FinanceTurnover:
                    financeTurnover = contentItem.ContentItem.As<FinanceTurnoverPart>();
                    break;

                default:
                    Debug.WriteLine("Switch statement unable to find content type");
                    break;
            }
        }
    }
}
