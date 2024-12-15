using ppm_fe.Models;

namespace ppm_fe.Services
{
    public interface IBillingService
    {
        Task<ApiResponse<Billing>> PreviewCreateBilling(Billing billing);

        Task<ApiResponse<Billing>> CreateBilling(Billing billing);

        Task<ApiResponse<List<Billing>>> GetBillings(int page = 1, int perPage = 10);

        Task<ApiResponse<List<Billing>>> FetchBillsPerMonth(string month, int page = 1, int perPage = 10);

        Task<string> DownloadAndOpenPdfAsync(int billingNumber, string url, string path);

        Task<ApiResponse<List<string>>> GetPdfUrls();

        Task UploadBillingPdfAsync(string fileName, byte[] pdfBytes, int billId);

        Task<ApiResponse<int>> GetNumberOfBills();
    }
}
