using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using ppm_fe.Constants;
using ppm_fe.Helpers;
using ppm_fe.Models;

namespace ppm_fe.Services
{
    public class BillingService : IBillingService
    {

        private readonly HttpClient _client = new()
        {
            BaseAddress = new Uri(RouteHelper.BaseUrl),
        };

        public async Task<ApiResponse<Billing>> CreateBilling(Billing billing)
        {
            string route = RouteHelper.GetFullUrl("/billings");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{route}/create"),
                Content = new StringContent(JsonConvert.SerializeObject(billing), Encoding.UTF8, "application/json")
            };

            try
            {
                var token = await SecureStorage.GetAsync(Properties.TokenKey);
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _client.SendAsync(request);
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var responseResult = JsonConvert.DeserializeObject<ApiResponse<Billing>>(jsonResponse);

                if (responseResult != null)
                {
                    return responseResult;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(CreateBilling), $"API request failed: {ex.Message}", new { Billing = billing }, null);
            }

            return new ApiResponse<Billing>
            {
                Success = false,
                Message = Properties.UnexpectedError,
                Data = null
            };
        }

        // Download billing pdf file
        public async Task<string> DownloadAndOpenPdfAsync(int billingId, string url, string path)
        {
            string pdfName = Path.GetFileName(url);
            string route = RouteHelper.GetFullUrl("/billings");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{route}/download/{billingId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await SecureStorage.GetAsync(Properties.TokenKey));

            try
            {
                // Check if the file already exists
                string filePath = Path.Combine(path, pdfName);
                if (File.Exists(filePath))
                {
                    return filePath;
                }

                var response = await _client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var pdfData = await response.Content.ReadAsByteArrayAsync();

                    // Write data to the file
                    await File.WriteAllBytesAsync(filePath, pdfData);

                    return filePath; // Return the path of the newly downloaded file to open it from the viewmodel
                }
                else
                {
                    throw new HttpRequestException($"Error downloading PDF: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(DownloadAndOpenPdfAsync), $"Error in DownloadAndOpenPdf: {ex.Message}", new { BillingId = billingId,  Url = url , Path = path}, ex.StackTrace);
                throw; // Rethrow the exception to be handled by the caller in the viewmodel
            }
        }

        // Fetch billings for the logged in user
        public async Task<ApiResponse<List<Billing>>> GetBillings(int page = 1, int perPage = 10)
        {
            string route = RouteHelper.GetFullUrl("/billings");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{route}?page={page}&per_page={perPage}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await SecureStorage.GetAsync(Properties.TokenKey));
            
            try
            {
                var response = await _client.SendAsync(request);
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var responseResult = JsonConvert.DeserializeObject<ApiResponse<List<Billing>>>(jsonResponse);

                if (responseResult != null && responseResult.Success)
                {
                    return responseResult;
                }
            }
            catch (Exception ex)
            {

                LoggerHelper.LogError(this.GetType().Name, nameof(GetBillings), $"Error while getting last 10 bills: {ex.Message}", new { Page = page, PerPage = perPage }, ex.StackTrace);
            }

            return new ApiResponse<List<Billing>>
            {
                Success = false,
                Data = null
            };
        }

        // Fetch list of billings filtered by month 
        public async Task<ApiResponse<List<Billing>>> FetchBillsPerMonth(string month, int page = 1, int perPage = 10)
        {
            string route = RouteHelper.GetFullUrl("/billings");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{route}/{month}?page={page}&per_page={perPage}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await SecureStorage.GetAsync(Properties.TokenKey));
            try
            {
                var response = await _client.SendAsync(request);
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var responseResult = JsonConvert.DeserializeObject<ApiResponse<List<Billing>>>(jsonResponse);

                if (responseResult != null && responseResult.Success)
                {
                    return responseResult;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(FetchBillsPerMonth), $"Error while getting billing per month: {ex.Message}", new { Month = month, Page = page, PerPage = perPage }, ex.StackTrace);
            }
            return new ApiResponse<List<Billing>>
            {
                Success = false,
                Data = null 
            };
        }

        // Fetch all billings of all users for admin 
        public async Task<ApiResponse<List<Billing>>> GetAdminBillings(int page = 1, int perPage = 10)
        {
            string route = RouteHelper.GetFullUrl("/billings/allusers");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{route}?page={page}&per_page={perPage}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await SecureStorage.GetAsync(Properties.TokenKey));
            try
            {
                var response = await _client.SendAsync(request);
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var responseResult = JsonConvert.DeserializeObject<ApiResponse<List<Billing>>>(jsonResponse);

                if (responseResult != null && responseResult.Success)
                {
                    return responseResult;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(GetBillings), $"Error while getting last 10 bills: {ex.Message}", new { Page = page, PerPage = perPage }, ex.StackTrace);
            }

            return new ApiResponse<List<Billing>>
            {
                Success = false,
                Data = null
            };
        }

        // Fetch list of billings filtered by month 
        public async Task<ApiResponse<List<Billing>>> FetchAdminBillsPerMonth(string month, int page = 1, int perPage = 10)
        {
            string route = RouteHelper.GetFullUrl("/billings/allusers");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{route}/{month}?page={page}&per_page={perPage}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await SecureStorage.GetAsync(Properties.TokenKey));
            try
            {
                var response = await _client.SendAsync(request);
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var responseResult = JsonConvert.DeserializeObject<ApiResponse<List<Billing>>>(jsonResponse);

                if (responseResult != null && responseResult.Success)
                {
                    return responseResult;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(FetchBillsPerMonth), $"Error while getting billing per month: {ex.Message}", new { Month = month, Page = page, PerPage = perPage }, ex.StackTrace);
            }

            return new ApiResponse<List<Billing>>
            {
                Success = false,
                Data = null
            };
        }

        // Send the PDF to the API and store it in the database
        public async Task StoreBillingPdfAsync(string fileName, byte[] pdfBytes, int billId)
        {
            try
            {
                string route = RouteHelper.GetFullUrl("/billings");
                var request = new HttpRequestMessage(HttpMethod.Post, $"{route}/{billId}/pdf");

                // Create the multipart form content
                using (var content = new MultipartFormDataContent())
                {
                    var fileContent = new ByteArrayContent(pdfBytes);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                    content.Add(fileContent, "pdf", fileName);

                    request.Content = content;
                    HttpResponseMessage response = await _client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        LoggerHelper.LogError(this.GetType().Name, nameof(StoreBillingPdfAsync), $"Failed to upload PDF. Status Code: {response.StatusCode} Response: {responseBody}", new { FileName = fileName, BillId = billId }, null);

                    }                
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(StoreBillingPdfAsync), $"An error occurred while uploading the PDF: {ex.Message}", new { FileName = fileName, BillId = billId }, null);
            }
        }

        public async Task<ApiResponse<int>> GetNumberOfBills()
        {
            string route = RouteHelper.GetFullUrl("/billings/count/created");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{route}"),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await SecureStorage.GetAsync(Properties.TokenKey));

            try
            {
                var token = await SecureStorage.GetAsync(Properties.TokenKey);
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _client.SendAsync(request);
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var responseResult = JsonConvert.DeserializeObject<ApiResponse<int>>(jsonResponse);

                if (responseResult != null)
                {
                    return responseResult;
                }
                else
                {
                    LoggerHelper.LogError(this.GetType().Name, nameof(GetNumberOfBills), $"API request failed: {response.StatusCode}", new { }, null);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(GetNumberOfBills), $"An error occurred: {ex.Message}", new { }, ex.StackTrace);
            }

            return new ApiResponse<int>
            {
                Success = true,
                Data = -1
            };
        }
    }
}
