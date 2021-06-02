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

        using (MemoryStream stream = new())
        using (PdfWriter writer = new(stream))
        {
            HtmlConverter.ConvertToPdf(html, writer);
            return stream.ToArray();
        }
    }
}
