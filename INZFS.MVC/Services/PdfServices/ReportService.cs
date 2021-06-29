using INZFS.MVC;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.Flows.Models;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

public class ReportService : IReportService
{

    //private readonly IContentRepository _contentRepository;

    public ReportService()
    {

    }

    public async Task<byte[]> GeneratePdfReport(string applicationId)
    {
        //var application = await _contentRepository.GetContentItemById(applicationId);
        //var bagPart = application?.ContentItem?.As<BagPart>();
        //var contents = bagPart?.ContentItems;

        var html = $@"
           <!DOCTYPE html>
           <html lang=""en"">
           <head>
           </head>
          <body>
          <h1>Placeholder</h1>
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
}
