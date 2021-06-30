using INZFS.MVC;
using INZFS.MVC.Controllers;
using INZFS.MVC.Forms;
using INZFS.MVC.Models;
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

    private CompanyDetailsPart _companyDetailsPart;

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
        
        //foreach(var contentItem in contents)
        //{
        //    foreach(var iterable in contentItem.Content)
        //    {
        //        var inner = iterable.Value;
        //        Debug.WriteLine("Breakpoint");
        //    }
        //    dynamic content = contentItem.Content;
        //   Debug.WriteLine("Loop over");
        //}

        var html = $@"
           <!DOCTYPE html>
           <html lang=""en"">
           <head>
           </head>
          <body>
          <h1>{ _companyDetailsPart.CompanyName }</h1>
          <p>{ applicationId }</p>
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
                    _companyDetailsPart = contentItem.ContentItem.As<CompanyDetailsPart>();
                    break;

                default:
                    Debug.WriteLine("Switch statement unable to find content type");
                    break;
            }
        }
    }
}
