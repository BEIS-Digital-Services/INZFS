using System;
using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using FluentAssertions;
using DiffMatchPatch;

namespace NetZero.Automated.UI.Tests.Utils
{
    public class PdfHelpers
    {
        public  string ReadFile(string pdfPath)
        {
            var pageText = new StringBuilder();
            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(pdfPath)))
            {
                var pageNumbers = pdfDocument.GetNumberOfPages();
                for (int i = 1; i <= pageNumbers; i++)
                {
                    LocationTextExtractionStrategy strategy = new LocationTextExtractionStrategy();
                    PdfCanvasProcessor parser = new PdfCanvasProcessor(strategy);
                    parser.ProcessPageContent(pdfDocument.GetPage(i));
                    pageText.Append(strategy.GetResultantText());
                }
            }
            return pageText.ToString();
        }

        public  bool CompareContentOfPdf(string firstFileText, string secondFileText)
        {
            StringBuilder compareResult = new StringBuilder();
            diff_match_patch dmp = new diff_match_patch();
            var diff = dmp.diff_main(firstFileText, secondFileText);
            if (diff.Count == 1 && diff[0].operation == Operation.EQUAL)
                return true;
            foreach (var d in diff)
            {
                if(d.operation == Operation.EQUAL)
                    compareResult.Append($"{d.text}");
                else if (d.operation == Operation.DELETE)
                    compareResult.Append($"[----{d.text}----] ");
                else
                    compareResult.Append($"[++++{d.text}++++] ");
            }
            Console.WriteLine(compareResult.ToString());
            return false;
        }
        public  void CompareTwoPDF(string firstPdf, string secondPdf)
        {
            CompareContentOfPdf(ReadFile(firstPdf), ReadFile(secondPdf)).Should().BeTrue();
        }
    }
}
