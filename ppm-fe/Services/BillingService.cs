using ppm_fe.Constants;
using ppm_fe.Helpers;
using ppm_fe.Models;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using iText.Kernel.Pdf;

namespace ppm_fe.Services
{
    public class BillingService : IBillingService
    {

        private readonly HttpClient _client = new()
        {
            BaseAddress = new Uri(RouteHelper.BaseUrl),
        };


        //Service function to create a preview of a billing and validate data
        public async Task<ApiResponse<Billing>> PreviewCreateBilling(Billing billing)
        {
            string route = RouteHelper.GetFullUrl("/billings");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{route}/preview"),
                Content = new StringContent(JsonConvert.SerializeObject(billing), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await SecureStorage.GetAsync("token"));

            try
            {
                var token = await SecureStorage.GetAsync("token");
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _client.SendAsync(request);
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var responseResult = JsonConvert.DeserializeObject<ApiResponse<Billing>>(jsonResponse);

                if (responseResult != null)
                {
                    return responseResult;
                }
                {
                    LoggerHelper.LogError(this.GetType().Name, nameof(PreviewCreateBilling), $"API request failed: {response.StatusCode}", billing, null);
                    return new ApiResponse<Billing>
                    {
                        Success = false,
                        Message = ErrorMessage.UnexpectedError,
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(PreviewCreateBilling), $"An error occurred: {ex.Message}", billing, null);
                return new ApiResponse<Billing>
                {
                    Success = false,
                    Message = ErrorMessage.UnexpectedError,
                    Data = null
                };
            }
        }

        //Service function to create the billing after the validation
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
                var token = await SecureStorage.GetAsync("token");
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _client.SendAsync(request);
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var responseResult = JsonConvert.DeserializeObject<ApiResponse<Billing>>(jsonResponse);

                if (responseResult != null && responseResult.Success)
                {
                    return responseResult;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(PreviewCreateBilling), $"API request failed: {ex.Message}", billing, null);
            }

            return new ApiResponse<Billing>
            {
                Success = false,
                Message = ErrorMessage.UnexpectedError,
                Data = null
            };
        }

        //Service function to download and pen billing pdf file
        public async Task<string> DownloadAndOpenPdfAsync(int billingNumber, string url, string path)
        {
            string pdfName = Path.GetFileName(url);
            string route = RouteHelper.GetFullUrl("/billings");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{route}/download/{billingNumber}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await SecureStorage.GetAsync("token"));

            try
            {
                // Check if the file already exists
                string filePath = Path.Combine(path, pdfName);
                if (File.Exists(filePath))
                {
                    return filePath; // Return the existing file path
                }

                var response = await _client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var pdfData = await response.Content.ReadAsByteArrayAsync();

                    // Save the file
                    await File.WriteAllBytesAsync(filePath, pdfData);

                    return filePath; // Return the path of the newly downloaded file
                }
                else
                {
                    throw new HttpRequestException($"Error downloading PDF: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(DownloadAndOpenPdfAsync), $"Error in DownloadAndOpenPdf: {ex.Message}", new { url }, ex.StackTrace);
                throw; // Rethrow the exception to be handled by the caller
            }
        }

        //Service function to get billings for the logged in user
        public async Task<ApiResponse<List<Billing>>> GetBillings(int page = 1, int perPage = 10)
        {
            string route = RouteHelper.GetFullUrl("/billings");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{route}?page={page}&per_page={perPage}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await SecureStorage.GetAsync("token"));
            try
            {
                // Read the response content
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

                LoggerHelper.LogError(this.GetType().Name, nameof(GetBillings), $"Error while getting last 10 bills: {ex.Message}", new { App.UserDetails }, ex.StackTrace);
            }

            return new ApiResponse<List<Billing>>
            {
                Success = false,
                Data = null
            };
        }

        // Service function to get list of  billings filtered by month 
        public async Task<ApiResponse<List<Billing>>> FetchBillsPerMonth(string month, int page = 1, int perPage = 10)
        {
            string route = RouteHelper.GetFullUrl("/billings");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{route}/{month}?page={page}&per_page={perPage}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await SecureStorage.GetAsync("token"));
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
                LoggerHelper.LogError(this.GetType().Name, nameof(FetchBillsPerMonth), $"Error while getting billing per month: {ex.Message}", new { App.UserDetails, month }, ex.StackTrace);
            }

            // If the response was not successful, return a failed response
            return new ApiResponse<List<Billing>>
            {
                Success = false,
                Data = null // No data to return on failure
            };
        }

        // Service functions to get billings pdf files for loggend in user
        public async Task<ApiResponse<List<string>>> GetPdfUrls()
        {

            string route = RouteHelper.GetFullUrl("/billings");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{route}/pdfs");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await SecureStorage.GetAsync("token"));

            try
            {
                var response = await _client.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var responseResult = JsonConvert.DeserializeObject<ApiResponse<List<string>>>(jsonResponse);

                    return responseResult;
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new ApiResponse<List<string>>
                    {
                        Success = false,
                        Message = ErrorMessage.NotFoundErrorHelper("billings"),
                        Data = null
                    };
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return new ApiResponse<List<string>>
                    {
                        Success = false,
                        Message = ErrorMessage.NotAuthorizedError,
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(GetPdfUrls), $"Error while fetching pdf urls: {ex.Message}", new { App.UserDetails }, ex.StackTrace);
            }

            return new ApiResponse<List<string>>
            {
                Success = false,
                Message = ErrorMessage.UnexpectedError,
                Data = null
            };
        }

        // Service function to send the PDF to the API and store it the database
        public async Task UploadBillingPdfAsync(string fileName, byte[] pdfBytes, int billId)
        {
            try
            {
                // Build the route
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

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("PDF uploaded successfully.");
                    }
                    else
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        LoggerHelper.LogError(this.GetType().Name, nameof(UploadBillingPdfAsync), $"Failed to upload PDF. Status Code: {response.StatusCode} Response: {responseBody}", new { App.UserDetails }, null);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(UploadBillingPdfAsync), $"An error occurred while uploading the PDF: {ex.Message}", new { fileName, pdfBytes, billId }, null);
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
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await SecureStorage.GetAsync("token"));

            try
            {
                var token = await SecureStorage.GetAsync("token");
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
                    LoggerHelper.LogError(this.GetType().Name, nameof(GetNumberOfBills), $"API request failed: {response.StatusCode}", new { UserId = App.UserDetails.Id }, null);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(GetNumberOfBills), $"An error occurred: {ex.Message}", new { UserId = App.UserDetails.Id }, ex.StackTrace);
            }

            return new ApiResponse<int>
            {
                Success = true,
                Data = -1
            };
        }
    }
}
