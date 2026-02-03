using System.Net.Http.Json;
using System.Text.Json;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.VnptEkyc;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Infrastructure.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Beyond8.Integration.Infrastructure.ExternalServices
{
    public class VnptEkycService(ILogger<VnptEkycService> logger, IHttpClientFactory httpClientFactory, IOptions<VnptEkycSettings> options) : IVnptEkycService
    {
        private const string UploadEndpoint = "file-service/v1/addFile";
        private const string LivenessCheckEndpoint = "ai/v1/card/liveness";
        private const string ClassifyEndpoint = "ai/v1/classify/id";
        private const string OcrFrontEndpoint = "ai/v1/ocr/id/front";
        private const string OcrBackEndpoint = "ai/v1/ocr/id/back";
        private const string CompareFaceEndpoint = "ai/v1/face/compare";
        private const string HttpClientName = "VnptEkycClient";

        private readonly VnptEkycSettings _settings = options.Value;
        private readonly ILogger<VnptEkycService> _logger = logger;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        public async Task<UploadResponse> UploadAsync(IFormFile file)
        {
            try
            {
                _logger.LogInformation("Starting VNPT eKYC file upload: {FileName}", file.FileName);

                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Upload failed: File is empty");
                    throw new ArgumentException("File cannot be empty", nameof(file));
                }

                var httpClient = CreateHttpClient();

                using var formData = new MultipartFormDataContent();
                using var fileStream = file.OpenReadStream();
                using var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                formData.Add(streamContent, "file", file.FileName);
                formData.Add(new StringContent("file"), "title");
                formData.Add(new StringContent("file"), "description");

                _logger.LogDebug("Sending upload request to VNPT eKYC API");

                var response = await httpClient.PostAsync(UploadEndpoint, formData);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("VNPT eKYC upload failed with status {StatusCode}: {ErrorContent}",
                        response.StatusCode, errorContent);
                    throw new HttpRequestException($"VNPT eKYC upload failed: {response.StatusCode}");
                }

                var result = await response.Content.ReadFromJsonAsync<VnptEkycResponse<UploadResponse>>();

                if (result?.Object == null)
                {
                    _logger.LogError("VNPT eKYC upload response is null or invalid");
                    throw new InvalidOperationException("Invalid response from VNPT eKYC API");
                }

                _logger.LogInformation("VNPT eKYC file uploaded successfully: Hash={Hash}, FileName={FileName}",
                    result.Object.Hash, result.Object.FileName);

                return result.Object;
            }
            catch (Exception ex) when (ex is not ArgumentException and not HttpRequestException and not InvalidOperationException)
            {
                _logger.LogError(ex, "Unexpected error during VNPT eKYC file upload: {FileName}", file.FileName);
                throw new InvalidOperationException("Failed to upload file to VNPT eKYC", ex);
            }
        }

        public async Task<LivenessResponse> CheckLivenessAsync(LivenessRequest request)
        {
            try
            {
                _logger.LogInformation("Starting VNPT eKYC liveness check for session: {ClientSession}", request.ClientSession);

                if (string.IsNullOrWhiteSpace(request.Img))
                {
                    _logger.LogWarning("Liveness check failed: Image hash is empty");
                    throw new ArgumentException("Image hash cannot be empty", nameof(request.Img));
                }

                if (string.IsNullOrWhiteSpace(request.ClientSession))
                {
                    _logger.LogWarning("Liveness check failed: Client session is empty");
                    throw new ArgumentException("Client session cannot be empty", nameof(request.ClientSession));
                }

                var httpClient = CreateHttpClient();

                // Add mac-address header as required by VNPT API
                httpClient.DefaultRequestHeaders.Remove("mac-address");
                httpClient.DefaultRequestHeaders.Add("mac-address", "TEST1");

                _logger.LogDebug("Sending liveness check request to VNPT eKYC API for session: {ClientSession}", request.ClientSession);

                var response = await httpClient.PostAsJsonAsync(LivenessCheckEndpoint, request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("VNPT eKYC liveness check failed with status {StatusCode}: {ErrorContent}",
                        response.StatusCode, errorContent);
                    throw new HttpRequestException($"VNPT eKYC liveness check failed: {response.StatusCode}");
                }

                // VNPT API returns response with "object" field
                var result = await response.Content.ReadFromJsonAsync<VnptEkycResponse<LivenessResponse>>();

                if (result?.Object == null)
                {
                    _logger.LogError("VNPT eKYC liveness check response is null or invalid");
                    throw new InvalidOperationException("Invalid response from VNPT eKYC API");
                }

                _logger.LogInformation("VNPT eKYC liveness check completed: Liveness={Liveness}, Message={Message}, FaceSwapping={FaceSwapping}, FakeLiveness={FakeLiveness}",
                    result.Object.Liveness, result.Object.LivenessMsg, result.Object.FaceSwapping, result.Object.FakeLiveness);

                return result.Object;
            }
            catch (Exception ex) when (ex is not ArgumentException and not HttpRequestException and not InvalidOperationException)
            {
                _logger.LogError(ex, "Unexpected error during VNPT eKYC liveness check for session: {ClientSession}", request.ClientSession);
                throw new InvalidOperationException("Failed to check liveness with VNPT eKYC", ex);
            }
        }

        public async Task<ApiResponse<LivenessResponse>> UploadAndCheckLivenessAsync(IFormFile file)
        {
            try
            {
                _logger.LogInformation("Starting combined upload and liveness check for file: {FileName}", file.FileName);

                // Step 1: Upload file
                var uploadResult = await UploadAsync(file);

                // Step 2: Check liveness using the uploaded file hash
                var livenessRequest = new LivenessRequest
                {
                    Img = uploadResult.Hash,
                    ClientSession = Guid.NewGuid().ToString()
                };

                var livenessResult = await CheckLivenessAsync(livenessRequest);

                if (livenessResult.Liveness != "success")
                {
                    _logger.LogError("Liveness check failed: {LivenessMsg}", livenessResult.LivenessMsg);
                    return ApiResponse<LivenessResponse>.FailureResponse(livenessResult.LivenessMsg);
                }

                livenessResult.Hash = uploadResult.Hash;
                livenessResult.FileName = uploadResult.FileName;

                _logger.LogInformation("Combined upload and liveness check completed successfully");

                return ApiResponse<LivenessResponse>.SuccessResponse(livenessResult, "Kiểm tra liveness thành công");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed during upload and liveness check");
                return ApiResponse<LivenessResponse>.FailureResponse(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during upload and liveness check");
                return ApiResponse<LivenessResponse>.FailureResponse("Không thể kết nối đến dịch vụ VNPT eKYC");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during upload and liveness check");
                return ApiResponse<LivenessResponse>.FailureResponse("Đã xảy ra lỗi khi kiểm tra liveness");
            }
        }

        public async Task<ApiResponse<ClassifyWithOcrResponse>> ClassifyAsync(ClassifyRequest request)
        {
            try
            {
                _logger.LogInformation("Starting VNPT eKYC classification: IsFront={IsFront}", request.IsFront);

                var httpClient = CreateHttpClient();

                httpClient.DefaultRequestHeaders.Remove("mac-address");
                httpClient.DefaultRequestHeaders.Add("mac-address", "TEST1");

                var payload = new
                {
                    img_card = request.Img,
                    client_session = GenerateClientSession(),
                    token = Guid.NewGuid().ToString("N")
                };

                var response = await httpClient.PostAsJsonAsync(ClassifyEndpoint, payload);
                var jsonContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    try
                    {
                        var errorResult = JsonSerializer.Deserialize<ClassifyErrorResponse>(jsonContent);
                        var errorMsg = errorResult?.Errors?.FirstOrDefault() ?? errorResult?.Message ?? "Unknown Error";

                        _logger.LogError("VNPT eKYC failed. Code: {Code}, Msg: {Msg}, Errors: {Errors}",
                            errorResult?.StatusCode, errorResult?.Message, string.Join(", ", errorResult?.Errors ?? []));

                        return ApiResponse<ClassifyWithOcrResponse>.FailureResponse($"Lỗi từ VNPT: {errorMsg}");
                    }
                    catch
                    {
                        _logger.LogError("VNPT eKYC failed with status {StatusCode}: {Content}", response.StatusCode, jsonContent);
                        return ApiResponse<ClassifyWithOcrResponse>.FailureResponse($"Lỗi HTTP {response.StatusCode}");
                    }
                }

                var successResult = JsonSerializer.Deserialize<ClassifySuccessResponse>(jsonContent);

                if (successResult?.Object == null)
                {
                    _logger.LogError("VNPT eKYC returned 200 but body is invalid or null: {Content}", jsonContent);
                    return ApiResponse<ClassifyWithOcrResponse>.FailureResponse("Phản hồi từ VNPT không hợp lệ");
                }

                var cardType = request.IsFront ? 2 : 3;

                if (successResult.Object.Type != cardType)
                {
                    _logger.LogWarning("Card type mismatch. Request: {Req}, VNPT Response: {ResType} ({ResName})",
                       cardType, successResult.Object.Type, successResult.Object.Name);

                    return ApiResponse<ClassifyWithOcrResponse>.FailureResponse("Loại giấy tờ không đúng với yêu cầu");
                }

                _logger.LogInformation("VNPT eKYC classification success: Type={Type}, Name={Name}",
                    successResult.Object.Type, successResult.Object.Name);

                // Gọi OCR endpoint tương ứng với type từ classify result
                var ocrResult = request.IsFront
                    ? await OcrFrontAsync(request.Img, successResult.Object.Type)
                    : await OcrBackAsync(request.Img, successResult.Object.Type);

                if (!ocrResult.IsSuccess)
                {
                    _logger.LogWarning("OCR failed but classification succeeded: {Error}", ocrResult.Message);
                    return ApiResponse<ClassifyWithOcrResponse>.SuccessResponse(
                        new ClassifyWithOcrResponse
                        {
                            TypeName = successResult.Object.Type == 2 ? "mặt trước" : "mặt sau",
                            CardName = "Căn cước công dân"
                        },
                        "Phân loại thành công nhưng OCR thất bại");
                }

                var ocrData = ocrResult.Data!;
                var result = new ClassifyWithOcrResponse
                {
                    TypeName = successResult.Object.Type == 2 ? "mặt trước" : "mặt sau",
                    CardName = "Căn cước công dân"
                };

                if (request.IsFront)
                {
                    // Mặt trước: trả về loại giấy tờ và số giấy tờ
                    // Có thể có ở cả id_number hoặc id field
                    result.IdNumber = ocrData.IdNumber ?? ocrData.Id;
                    _logger.LogInformation("VNPT eKYC OCR front success: IdNumber={IdNumber}", result.IdNumber);
                }
                else
                {
                    // Mặt sau: trả về ngày hết hạn
                    result.IssueDate = ocrData.IssueDate;
                    _logger.LogInformation("VNPT eKYC OCR back success: IssueDate={IssueDate}", ocrData.IssueDate);
                }

                return ApiResponse<ClassifyWithOcrResponse>.SuccessResponse(result, "Phân loại và OCR thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during VNPT classification");
                return ApiResponse<ClassifyWithOcrResponse>.FailureResponse("Lỗi hệ thống khi xử lý phân loại");
            }
        }

        private async Task<ApiResponse<OcrResponse>> OcrFrontAsync(string imgHash, int cardType)
        {
            try
            {
                _logger.LogInformation("Starting VNPT eKYC OCR front: ImgHash={ImgHash}, CardType={CardType}", imgHash, cardType);

                var httpClient = CreateHttpClient();
                httpClient.DefaultRequestHeaders.Remove("mac-address");
                httpClient.DefaultRequestHeaders.Add("mac-address", "TEST1");

                var payload = new OcrRequestFront
                {
                    ImgFront = imgHash,
                    Type = cardType,
                    ValidatePostcode = null,
                    ClientSession = GenerateClientSession(),
                    Token = Guid.NewGuid().ToString("N")
                };

                var response = await httpClient.PostAsJsonAsync(OcrFrontEndpoint, payload);
                var jsonContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("VNPT eKYC OCR front failed with status {StatusCode}: {Content}",
                        response.StatusCode, jsonContent);
                    return ApiResponse<OcrResponse>.FailureResponse($"Lỗi OCR mặt trước: HTTP {response.StatusCode}");
                }

                var result = JsonSerializer.Deserialize<VnptEkycResponse<OcrResponse>>(jsonContent);

                if (result?.Object == null)
                {
                    _logger.LogError("VNPT eKYC OCR front returned 200 but body is invalid: {Content}", jsonContent);
                    return ApiResponse<OcrResponse>.FailureResponse("Phản hồi OCR mặt trước không hợp lệ");
                }

                return ApiResponse<OcrResponse>.SuccessResponse(result.Object, "OCR mặt trước thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during VNPT OCR front");
                return ApiResponse<OcrResponse>.FailureResponse("Lỗi hệ thống khi OCR mặt trước");
            }
        }

        private async Task<ApiResponse<OcrResponse>> OcrBackAsync(string imgHash, int cardType)
        {
            try
            {
                _logger.LogInformation("Starting VNPT eKYC OCR back: ImgHash={ImgHash}, CardType={CardType}", imgHash, cardType);

                var httpClient = CreateHttpClient();
                httpClient.DefaultRequestHeaders.Remove("mac-address");
                httpClient.DefaultRequestHeaders.Add("mac-address", "TEST1");

                var payload = new OcrRequestBack
                {
                    ImgBack = imgHash,
                    Type = cardType,
                    ClientSession = GenerateClientSession(),
                    Token = Guid.NewGuid().ToString("N")
                };

                var response = await httpClient.PostAsJsonAsync(OcrBackEndpoint, payload);
                var jsonContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("VNPT eKYC OCR back failed with status {StatusCode}: {Content}",
                        response.StatusCode, jsonContent);
                    return ApiResponse<OcrResponse>.FailureResponse($"Lỗi OCR mặt sau: HTTP {response.StatusCode}");
                }

                var result = JsonSerializer.Deserialize<VnptEkycResponse<OcrResponse>>(jsonContent);

                if (result?.Object == null)
                {
                    _logger.LogError("VNPT eKYC OCR back returned 200 but body is invalid: {Content}", jsonContent);
                    return ApiResponse<OcrResponse>.FailureResponse("Phản hồi OCR mặt sau không hợp lệ");
                }

                _logger.LogInformation("VNPT eKYC OCR back success: IssueDate={IssueDate}, ExpiryDate={ExpiryDate}",
                    result.Object.IssueDate, result.Object.ExpiryDate);
                return ApiResponse<OcrResponse>.SuccessResponse(result.Object, "OCR mặt sau thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during VNPT OCR back");
                return ApiResponse<OcrResponse>.FailureResponse("Lỗi hệ thống khi OCR mặt sau");
            }
        }


        private string GenerateClientSession()
        {
            var random = new Random();
            return $"ANDROID_nokia7.2_28_Simulator_2.4.2_08d2d8686ee5fa0e_{random.Next(int.MinValue, int.MaxValue).ToString()}";
        }

        private HttpClient CreateHttpClient()
        {
            return _httpClientFactory.CreateClient(HttpClientName);
        }

        public async Task<CompareFaceResponse> CompareFaceAsync(CompareFaceRequest request)
        {
            try
            {
                _logger.LogInformation("Starting VNPT eKYC face comparison: ImgFront={ImgFront}, ImgFace={ImgFace}, ClientSession={ClientSession}",
           request.ImgFront, request.ImgFace, request.ClientSession);

                // Validation
                if (string.IsNullOrWhiteSpace(request.ImgFront))
                {
                    _logger.LogWarning("Face comparison failed: ImgFront is empty");
                    throw new ArgumentException("ImgFront cannot be empty", nameof(request.ImgFront));
                }
                if (string.IsNullOrWhiteSpace(request.ImgFace))
                {
                    _logger.LogWarning("Face comparison failed: ImgFace is empty");
                    throw new ArgumentException("ImgFace cannot be empty", nameof(request.ImgFace));
                }
                if (string.IsNullOrWhiteSpace(request.ClientSession))
                {
                    _logger.LogWarning("Face comparison failed: ClientSession is empty");
                    throw new ArgumentException("ClientSession cannot be empty", nameof(request.ClientSession));
                }
                if (string.IsNullOrWhiteSpace(request.Token))
                {
                    _logger.LogWarning("Face comparison failed: Token is empty");
                    throw new ArgumentException("Token cannot be empty", nameof(request.Token));
                }

                var httpClient = CreateHttpClient();

                httpClient.DefaultRequestHeaders.Remove("mac-address");
                httpClient.DefaultRequestHeaders.Add("mac-address", "TEST1");

                var payload = new
                {
                    img_front = request.ImgFront,
                    img_face = request.ImgFace,
                    client_session = request.ClientSession,
                    token = request.Token
                };

                _logger.LogDebug("Sending face comparison request to VNPT eKYC API");

                var response = await httpClient.PostAsJsonAsync(CompareFaceEndpoint, payload);
                var jsonContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("VNPT eKYC face comparison failed with status {StatusCode}: {ErrorContent}",
                        response.StatusCode, errorContent);
                    throw new HttpRequestException($"VNPT eKYC face comparison failed: {response.StatusCode}");
                }

                var result = JsonSerializer.Deserialize<VnptEkycResponse<CompareFaceResponse>>(jsonContent);

                if (result?.Object == null)
                {
                    _logger.LogError("VNPT eKYC face comparison response is null or invalid");
                    throw new InvalidOperationException("Invalid response from VNPT eKYC API");
                }

                _logger.LogInformation("VNPT eKYC face comparison completed: Result={Result}, Msg={Msg}, Prob={Prob}",
                    result.Object.Result, result.Object.Msg, result.Object.Prob);

                return result.Object;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during VNPT face comparison");
                throw new InvalidOperationException("Failed to compare faces with VNPT eKYC", ex);
            }
        }

        public async Task<ApiResponse<CompareFaceResponse>> UploadFaceAndCompareAsync(IFormFile faceFile, string imgFrontHash)
        {
            try
            {
                _logger.LogInformation("Starting upload face and compare: FrontHash={ImgFrontHash}, FaceFile={FaceFileName}",
                imgFrontHash, faceFile.FileName);

                var uploadImgFaceResult = await UploadAsync(faceFile);

                var compareFaceRequest = new CompareFaceRequest
                {
                    ImgFront = imgFrontHash,
                    ImgFace = uploadImgFaceResult.Hash,
                    ClientSession = GenerateClientSession(),
                    Token = Guid.NewGuid().ToString("N")
                };

                var compareFaceResult = await CompareFaceAsync(compareFaceRequest);

                if (compareFaceResult.Msg != "MATCH")
                {
                    _logger.LogWarning("Face comparison not matched: {Result}, Msg={Msg}, Prob={Prob}",
                    compareFaceResult.Result, compareFaceResult.Msg, compareFaceResult.Prob);
                    return ApiResponse<CompareFaceResponse>.FailureResponse(compareFaceResult.Result ?? "Khuôn mặt không khớp");
                }

                _logger.LogInformation("Upload face and compare completed successfully");
                return ApiResponse<CompareFaceResponse>.SuccessResponse(compareFaceResult, "So sánh khuôn mặt thành công");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed during face upload and comparison");
                return ApiResponse<CompareFaceResponse>.FailureResponse(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during face upload and comparison");
                return ApiResponse<CompareFaceResponse>.FailureResponse("Không thể kết nối đến dịch vụ VNPT eKYC");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during VNPT face upload and comparison");
                return ApiResponse<CompareFaceResponse>.FailureResponse("Lỗi hệ thống khi tải lên và so sánh khuôn mặt");
            }
        }
    }
}
