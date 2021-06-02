using iText.Html2pdf;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

public class ReportService : IReportService
{

    public ReportService()
    {

    }

    public byte[] GeneratePdfReport(string companyName, string applicationId)
    {
        var html = $@"
           <!DOCTYPE html>
           <html lang=""en"">
           <head>
           </head>
          <body>
          <h1>{ companyName }</h1>
          <p>{ applicationId }</p>
          </body>
          </html>
          ";

        using (var workstream = new MemoryStream())
        using (var pdfWriter = new PdfWriter(workstream))
        {
            HtmlConverter.ConvertToPdf(html, pdfWriter);
            return workstream.ToArray();
        }
    }
}
