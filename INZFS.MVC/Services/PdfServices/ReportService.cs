using DinkToPdf;
using DinkToPdf.Contracts;

public class ReportService: IReportService
{
    private readonly IConverter _converter;
    public ReportService(IConverter converter)
    {
        _converter = converter;
    }
    public byte[] GeneratePdfReport()
    {
        var html = $@"
           <!DOCTYPE html>
           <html lang=""en"">
           <head>
               This is the header of this document.
           </head>
          <body>
          <h1>This is the heading for demonstration purposes only.</h1>
          <p>This is a line of text for demonstration purposes only.</p>
          </body>
          </html>
          ";
        GlobalSettings globalSettings = new GlobalSettings();
        globalSettings.ColorMode = ColorMode.Color;
        globalSettings.Orientation = Orientation.Portrait;
        globalSettings.PaperSize = PaperKind.A4;
        globalSettings.Margins = new MarginSettings { Top = 25, Bottom = 25 };
        ObjectSettings objectSettings = new ObjectSettings();
        objectSettings.PagesCount = true;
        objectSettings.HtmlContent = html;
        WebSettings webSettings = new WebSettings();
        webSettings.DefaultEncoding = "utf-8";
        HeaderSettings headerSettings = new HeaderSettings();
        headerSettings.FontSize = 15;
        headerSettings.FontName = "Ariel";
        headerSettings.Right = "Page [page] of [toPage]";
        headerSettings.Line = true;
        FooterSettings footerSettings = new FooterSettings();
        footerSettings.FontSize = 12;
        footerSettings.FontName = "Ariel";
        footerSettings.Center = "This is for demonstration purposes only.";
        footerSettings.Line = true;
        objectSettings.HeaderSettings = headerSettings;
        objectSettings.FooterSettings = footerSettings;
        objectSettings.WebSettings = webSettings;
        HtmlToPdfDocument htmlToPdfDocument = new HtmlToPdfDocument()
        {
            GlobalSettings = globalSettings,
            Objects = { objectSettings },
        };
        return _converter.Convert(htmlToPdfDocument);
    }
}