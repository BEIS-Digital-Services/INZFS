using Microsoft.AspNetCore.Mvc;

public interface IReportService
{
    public byte[] GeneratePdfReport(
        string companyName,
        string applicationId
        );
}