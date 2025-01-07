using ppm_fe.Models;

namespace ppm_fe.Services
{
    public interface IWorkService
    {
        Task<ApiResponse<int>> GetNumberOfWorks();

        Task<ApiResponse<int>> GetNumberOfIncompleteWorks();

        Task<ApiResponse<Work>> CreateWorkWithoutAgeGroups(Work work);

        Task SavePdfToDatabaseAsync(string fileName, byte[] pdfBytes, int wordId);

        Task<ApiResponse<Work>> UpdateWork(Work work);
        Task<ApiResponse<Work>> CompleteWork(Work work, string fileName, byte[] pdfBytes);

        Task<ApiResponse<List<Work>>> GetAllUsersWorks(int page = 1, int perPage = 10);

        Task<ApiResponse<List<Work>>> FetchWorksPerTeam(string SelectedTeam, int page = 1, int perPage = 10);

        Task<string> DownloadAndOpenPdfAsync(int workId, string url, string path);

        Task<ApiResponse<List<Work>>> GetAllWorks(int page = 1, int perPage = 10);
    }
}
