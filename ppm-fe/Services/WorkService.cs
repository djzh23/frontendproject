using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using ppm_fe.Constants;
using ppm_fe.Helpers;
using ppm_fe.Models;

namespace ppm_fe.Services
{
    public class WorkService : IWorkService
    {
        private readonly HttpClient _client = new()
        {
            BaseAddress = new Uri(RouteHelper.BaseUrl),
        };

        public async Task<ApiResponse<int>> GetNumberOfWorks()
        {
            string route = RouteHelper.GetFullUrl("/works/count/created");
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
                    LoggerHelper.LogError(this.GetType().Name, nameof(GetNumberOfWorks), $"API request failed: {response.StatusCode}", new {}, null);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(GetNumberOfWorks), $"An error occurred: {ex.Message}", new {}, ex.StackTrace);
            }

            return new ApiResponse<int>
            {
                Success = true,
                Data = -1
            };
        }

        public async Task<ApiResponse<int>> GetNumberOfIncompleteWorks()
        {
            string route = RouteHelper.GetFullUrl("/works/count/incomplete");
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
                    LoggerHelper.LogError(this.GetType().Name, nameof(GetNumberOfWorks), $"API request failed: {response.StatusCode}", new { }, null);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(GetNumberOfWorks), $"An error occurred: {ex.Message}", new { }, ex.StackTrace);
            }

            return new ApiResponse<int>
            {
                Success = true,
                Data = -1
            };
        }

        public async Task<ApiResponse<Work>> CreateWorkWithoutAgeGroups(Work work)
        {
            string route = RouteHelper.GetFullUrl("/work");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{route}"),
                Content = new StringContent(JsonConvert.SerializeObject(work), Encoding.UTF8, "application/json")
            };

            try
            {
                var token = await SecureStorage.GetAsync("token");
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _client.SendAsync(request);
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var responseResult = JsonConvert.DeserializeObject<ApiResponse<Work>>(jsonResponse);

                if (responseResult != null)
                {
                    return responseResult;
                }
            }
            catch (Exception ex)
            {
                // Log the error
                LoggerHelper.LogError(GetType().Name, nameof(CreateWorkWithoutAgeGroups), ex.Message, new { work }, ex.StackTrace);
            }

            return new ApiResponse<Work>
            {
                Success = false,
                Message = Properties.UnexpectedError,
                Data = null
            };
        }

        public async Task SavePdfToDatabaseAsync(string fileName, byte[] pdfBytes, int wordId)
        {
            try
            {
                // Build the route
                string route = RouteHelper.GetFullUrl($"/works/{wordId}/pdf");
                var request = new HttpRequestMessage(HttpMethod.Post, $"{route}");

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
                        LoggerHelper.LogError(this.GetType().Name, nameof(SavePdfToDatabaseAsync), $"Failed to upload PDF. Status Code: {response.StatusCode} Response: {responseBody}", new { App.UserDetails }, null);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(SavePdfToDatabaseAsync), $"An error occurred while uploading the PDF: {ex.Message}", new { FileName = fileName, PdfBytes = pdfBytes, WorkdId = wordId }, ex.StackTrace);
            }
        }

        public async Task<ApiResponse<Work>> UpdateWork(Work work)
        {
            string route = RouteHelper.GetFullUrl($"/works/{work.Id}");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri($"{route}"),
                Content = new StringContent(JsonConvert.SerializeObject(work), Encoding.UTF8, "application/json")
            };

            try
            {
                var token = await SecureStorage.GetAsync("token");
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _client.SendAsync(request);
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var responseResult = JsonConvert.DeserializeObject<ApiResponse<Work>>(jsonResponse);

                if (responseResult != null)
                {
                    return responseResult;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(GetType().Name, nameof(UpdateWork), ex.Message, new { Work = work }, ex.StackTrace);
            }

            return new ApiResponse<Work>
            {
                Success = false,
                Message = Properties.UnexpectedError,
                Data = null
            };
        }

        public async Task<ApiResponse<Work>> CompleteWork(Work work, string fileName, byte[] pdfBytes)
        {
            string route = RouteHelper.GetFullUrl($"/works/{work.Id}/complete");
            
            try
            {
                Debug.WriteLine($"PDF bytes length: {pdfBytes.Length}");
                var token = await SecureStorage.GetAsync("token");
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using var content = new MultipartFormDataContent();

                // Add the work data
                var data = new StringContent(JsonConvert.SerializeObject(work), Encoding.UTF8, "application/json");
                content.Add(data, "data");

                // Add the PDF file
                var pdfContent = new ByteArrayContent(pdfBytes);
                content.Add(pdfContent, "pdf", fileName);

                var response = await _client.PostAsync(route, content);
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var responseResult = JsonConvert.DeserializeObject<ApiResponse<Work>>(jsonResponse);

                if (responseResult != null)
                {
                    return responseResult;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(CompleteWork), 
                    $"An error occurred: {ex.Message}", 
                    new { Work = work, FileName = fileName, PdfBytes = pdfBytes }, ex.StackTrace);
            }

            return new ApiResponse<Work>
            {
                Success = false,
                Message = Properties.UnexpectedError,
                Data = null
            };
        }

        public async Task<ApiResponse<List<Work>>> GetAllUsersWorks(int page = 1, int perPage = 10)
        {
            string route = RouteHelper.GetFullUrl($"/works/allusers?page={page}&per_page={perPage}");
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
                var responseResult = JsonConvert.DeserializeObject<ApiResponse<List<Work>>>(jsonResponse);

                if (responseResult != null)
                {
                    return responseResult;
                }
                else
                {
                    LoggerHelper.LogError(this.GetType().Name, nameof(GetAllUsersWorks), $"API request failed: {response.StatusCode}", new { Page = page, PerPage = perPage }, null);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(GetAllUsersWorks), $"An error occurred: {ex.Message}", new { Page = page, PerPage = perPage }, ex.StackTrace);
            }

            return new ApiResponse<List<Work>>
            {
                Success = false,
                Message = Properties.UnexpectedError,
                Data = null
            };
        }

        public async Task<ApiResponse<List<Work>>> FetchWorksPerTeam(string selectedTeam, int page = 1, int perPage = 10)
        {
            string route = RouteHelper.GetFullUrl($"/works/{selectedTeam}?page={page}&per_page={perPage}");
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
                var responseResult = JsonConvert.DeserializeObject<ApiResponse<List<Work>>>(jsonResponse);

                if (responseResult != null)
                {
                    return responseResult;
                }
                else
                {
                    LoggerHelper.LogError(this.GetType().Name, nameof(FetchWorksPerTeam), $"API request failed: {response.StatusCode}", new { SelectedTeam = selectedTeam, Page = page, PerPage = perPage }, null);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(FetchWorksPerTeam), $"An error occurred: {ex.Message}", new { SelectedTeam = selectedTeam, Page = page, PerPage = perPage }, ex.StackTrace);
            }

            return new ApiResponse<List<Work>>
            {
                Success = false,
                Message = Properties.UnexpectedError,
                Data = null
            };
        }

        public async Task<string> DownloadAndOpenPdfAsync(int workId, string url, string path)
        {
            string pdfName = Path.GetFileName(url);

            // Check if the file already exists
            string filePath = Path.Combine(path, pdfName);
            if (File.Exists(filePath))
            {
                return filePath;
            }

            string route = RouteHelper.GetFullUrl($"/works/download/{workId}");
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
                if (response.IsSuccessStatusCode)
                {
                    var pdfData = await response.Content.ReadAsByteArrayAsync();

                    // Save the file
                    await File.WriteAllBytesAsync(filePath, pdfData);

                    return filePath;
                }
                else
                {
                    throw new HttpRequestException($"Error downloading PDF: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(DownloadAndOpenPdfAsync), $"An error occurred: {ex.Message}", new { WorkId = workId, Url = url, Path = path }, ex.StackTrace);
                throw; // Rethrow the exception to be handled by the viewmodel
            }
        }

        public async Task<ApiResponse<List<Work>>> GetAllWorks(int page = 1, int perPage = 10)
        {
            string route = RouteHelper.GetFullUrl($"/works?page={page}&per_page={perPage}");
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
                var responseResult = JsonConvert.DeserializeObject<ApiResponse<List<Work>>>(jsonResponse);

                if (responseResult != null)
                {
                    return responseResult;
                }
                else
                {
                    LoggerHelper.LogError(this.GetType().Name, nameof(GetAllWorks), $"API request failed: {response.StatusCode}", new { Page = page, PerPage = perPage }, null);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(GetAllWorks), $"An error occurred: {ex.Message}", new { Page = page, PerPage = perPage }, ex.StackTrace);
            }

            return new ApiResponse<List<Work>>
            {
                Success = false,
                Message = Properties.UnexpectedError,
                Data = null
            };
        }
    }
}
    