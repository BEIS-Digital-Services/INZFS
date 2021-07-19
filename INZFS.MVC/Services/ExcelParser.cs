using System;
using ClosedXML.Excel;

namespace INZFS.MVC.Services
{
    public class ExcelParser
    {
        public ExcelParser(string filepath)
        {
            var wb = new XLWorkbook();
            var ws = wb.Worksheet("summary");
            var cells = ws.Range("a17:g31").CellsUsed();
        }
    }
}
