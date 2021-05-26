using iText.Html2pdf;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

public class ReportService
{
    public FileStreamResult GeneratePdfReport(string companyName, string applicationId)
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

        var workstream = new MemoryStream();
        using (var pdfWriter = new PdfWriter(workstream))
        {
            pdfWriter.SetCloseStream(false);
            using (var document = HtmlConverter.ConvertToDocument(html, pdfWriter))
            {

            }
        }
        workstream.Position = 0;
        return new FileStreamResult(workstream, "application/pdf");
    }
}
