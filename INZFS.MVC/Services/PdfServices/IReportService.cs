public interface IReportService
{
    public byte[] GeneratePdfReport(
        string title,
        string id
        );
}