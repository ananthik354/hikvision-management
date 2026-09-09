using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HikvisionBackend1.Services
{
    public class GroqPlateService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GroqPlateService> _logger;

        public GroqPlateService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GroqPlateService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<PlateResult?> ReadPlateAsync(
            string imagePath,
            CancellationToken cancellationToken = default)
        {
            if (!File.Exists(imagePath))
            {
                _logger.LogWarning(
                    "Image not found: {ImagePath}",
                    imagePath);

                return null;
            }

            byte[] imageBytes =
                await File.ReadAllBytesAsync(
                    imagePath,
                    cancellationToken);

            if (imageBytes.Length == 0)
            {
                _logger.LogWarning(
                    "Image is empty: {ImagePath}",
                    imagePath);

                return null;
            }

            string base64Image =
                Convert.ToBase64String(imageBytes);

            string extension =
                Path.GetExtension(imagePath)
                    .ToLowerInvariant();

            string mimeType = extension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };

            string dataUrl =
                $"data:{mimeType};base64,{base64Image}";

            string? apiKey =
                _configuration["Groq:ApiKey"];

            string model =
                _configuration["Groq:Model"]
                ?? "qwen/qwen3.6-27b";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Groq API key is not configured.");
            }

            var requestBody = new
            {
                model = model,

                messages = new object[]
                {
                    new
                    {
                        role = "user",

                        content = new object[]
                        {
                            new
{
    type = "text",

    text =
        """
        You are a vehicle license plate recognition system.

        The supplied image may be either:

        1. A dedicated close-up license plate image, OR
        2. A full vehicle image where the license plate is visible somewhere in the image.

        Carefully locate the license plate.

        Read the characters exactly as they appear in the image.

        Return ONLY valid JSON:

        {
          "plateNumber": "ABC123",
          "vehicleType": "Car"
        }

        Rules:
        - Do not add markdown.
        - Do not add explanations.
        - plateNumber must contain only license plate characters.
        - Preserve the characters exactly as visible.
        - Do not use the license plate number from any metadata or filename.
        - If the plate cannot be read with reasonable confidence, return an empty string.
        - vehicleType must be Car, Bike, Bus, Truck, Auto, or Unknown.
        """
},

                            new
                            {
                                type = "image_url",

                                image_url = new
                                {
                                    url = dataUrl
                                }
                            }
                        }
                    }
                },

                temperature = 0,

                max_completion_tokens = 200
            };

            string json =
                JsonSerializer.Serialize(requestBody);

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://api.groq.com/openai/v1/chat/completions");

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    apiKey);

            request.Content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

            _logger.LogInformation(
                "Sending image to Groq: {FileName}",
                Path.GetFileName(imagePath));

            using HttpResponseMessage response =
                await _httpClient.SendAsync(
                    request,
                    cancellationToken);

            string responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Groq API failed. Status: {Status}. Response: {Response}",
                    response.StatusCode,
                    responseBody);

                return null;
            }

            _logger.LogInformation(
                "Groq response received.");

            using JsonDocument document =
                JsonDocument.Parse(responseBody);

            string content =
                document.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString()
                    ?? "";

            _logger.LogInformation(
                "Groq raw result: {Result}",
                content);

            return ParseResult(content);
        }

        private PlateResult? ParseResult(string content)
        {
            try
            {
                content = content.Trim();

                // Remove ```json ... ``` if the model happens
                // to return markdown despite the instruction.
                if (content.StartsWith("```"))
                {
                    int firstNewLine =
                        content.IndexOf('\n');

                    if (firstNewLine >= 0)
                    {
                        content =
                            content[(firstNewLine + 1)..];
                    }

                    content =
                        content.Replace(
                            "```",
                            "")
                        .Trim();
                }

                using JsonDocument document =
                    JsonDocument.Parse(content);

                string plateNumber =
                    document.RootElement
                        .GetProperty("plateNumber")
                        .GetString()
                        ?? "";

                string vehicleType =
                    document.RootElement
                        .GetProperty("vehicleType")
                        .GetString()
                        ?? "Unknown";

                return new PlateResult
                {
                    PlateNumber = plateNumber.Trim(),
                    VehicleType = vehicleType.Trim()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to parse Groq response: {Content}",
                    content);

                return null;
            }
        }
    }

    public class PlateResult
    {
        public string PlateNumber { get; set; } = "";

        public string VehicleType { get; set; } = "Unknown";
    }
}