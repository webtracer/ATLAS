using System.Text;
using System.Text.Json;
using Amdocs.Atlas.Core.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Amdocs.Atlas.Api.Services;

/// <summary>
/// AI Analysis service using CoPilot (Azure OpenAI) for vSphere data analysis
/// </summary>
public class AIAnalysisService : IAIAnalysisService
{
    private readonly ILogger<AIAnalysisService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public AIAnalysisService(
        ILogger<AIAnalysisService> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient("CoPilot");
    }

    public async Task<VSphereAnalysisResult> AnalyzeInfrastructureAsync(
        Dictionary<string, object> infrastructureData,
        string analysisType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting AI analysis of type {AnalysisType}", analysisType);

            var prompt = BuildAnalysisPrompt(infrastructureData, analysisType);
            var analysis = await CallCoPilotAsync(prompt, cancellationToken);

            var result = new VSphereAnalysisResult
            {
                Success = true,
                AnalysisType = analysisType,
                AiAnalysis = analysis,
                Metrics = infrastructureData,
                AnalysisTimestamp = DateTime.UtcNow
            };

            result.Recommendations = ExtractRecommendations(analysis);
            result.Issues = ExtractIssues(analysis);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform AI analysis");
            return new VSphereAnalysisResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                AnalysisTimestamp = DateTime.UtcNow
            };
        }
    }

    public async Task<List<string>> GenerateRecommendationsAsync(
        Dictionary<string, object> metrics,
        string analysisType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = $"Based on the following vSphere {analysisType} metrics, provide specific, actionable recommendations:\n\n" +
                        $"{JsonSerializer.Serialize(metrics, new JsonSerializerOptions { WriteIndented = true })}\n\n" +
                        "Provide recommendations as a numbered list.";

            var response = await CallCoPilotAsync(prompt, cancellationToken);
            return ExtractRecommendations(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate recommendations");
            return new List<string> { "Unable to generate recommendations at this time." };
        }
    }

    private string BuildAnalysisPrompt(Dictionary<string, object> infrastructureData, string analysisType)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("You are a vSphere infrastructure expert. Analyze the following vSphere infrastructure data and provide insights.");
        sb.AppendLine($"Analysis Type: {analysisType}");
        sb.AppendLine();
        sb.AppendLine("Infrastructure Data:");
        sb.AppendLine(JsonSerializer.Serialize(infrastructureData, new JsonSerializerOptions { WriteIndented = true }));
        sb.AppendLine();

        switch (analysisType.ToLower())
        {
            case "performance":
                sb.AppendLine("Focus on:");
                sb.AppendLine("- CPU and memory utilization patterns");
                sb.AppendLine("- Performance bottlenecks");
                sb.AppendLine("- Resource contention issues");
                sb.AppendLine("- Optimization opportunities");
                break;

            case "capacity":
                sb.AppendLine("Focus on:");
                sb.AppendLine("- Current capacity utilization");
                sb.AppendLine("- Growth trends and projections");
                sb.AppendLine("- Capacity planning recommendations");
                sb.AppendLine("- Right-sizing opportunities");
                break;

            case "logs":
                sb.AppendLine("Focus on:");
                sb.AppendLine("- Error patterns and anomalies");
                sb.AppendLine("- Security concerns");
                sb.AppendLine("- System health indicators");
                sb.AppendLine("- Troubleshooting recommendations");
                break;

            default:
                sb.AppendLine("Provide a comprehensive analysis covering:");
                sb.AppendLine("- Overall infrastructure health");
                sb.AppendLine("- Key metrics and trends");
                sb.AppendLine("- Potential issues and concerns");
                sb.AppendLine("- Optimization recommendations");
                break;
        }

        sb.AppendLine();
        sb.AppendLine("Provide your analysis in a clear, structured format with specific recommendations.");

        return sb.ToString();
    }

    private async Task<string> CallCoPilotAsync(string prompt, CancellationToken cancellationToken)
    {
        var apiKey = _configuration["CoPilot:ApiKey"];
        var endpoint = _configuration["CoPilot:Endpoint"] ?? "https://api.openai.com/v1/chat/completions";
        var model = _configuration["CoPilot:Model"] ?? "gpt-4";

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("CoPilot API key not configured. Using mock response.");
            return GenerateMockAnalysis(prompt);
        }

        try
        {
            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = "You are a vSphere infrastructure expert." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 2000
            };

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json")
            };

            request.Headers.Add("api-key", apiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (jsonResponse.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                try
                {
                    var firstChoice = choices[0];
                    if (firstChoice.TryGetProperty("message", out var message) &&
                        message.TryGetProperty("content", out var content))
                    {
                        return content.GetString() ?? "No analysis available.";
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    _logger.LogWarning("Choices array was empty or invalid despite length check");
                }
            }

            return "Unable to parse AI response.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call CoPilot API. Using mock response.");
            return GenerateMockAnalysis(prompt);
        }
    }

    private string GenerateMockAnalysis(string prompt)
    {
        return $"""
            AI Analysis (Mock - CoPilot not configured):
            
            Based on the provided vSphere infrastructure data, here are the key findings:
            
            1. **Performance Overview**: The infrastructure shows moderate utilization with some optimization opportunities.
            
            2. **Key Metrics**:
               - CPU utilization is within acceptable ranges
               - Memory usage indicates potential for consolidation
               - Storage capacity is adequate but should be monitored
            
            3. **Recommendations**:
               - Consider right-sizing underutilized VMs
               - Implement resource pools for better resource management
               - Monitor capacity trends for future planning
            
            4. **Potential Issues**:
               - No critical issues detected in the current data
            
            Note: This is a mock response. Configure CoPilot API settings for real AI analysis.
            """;
    }

    private List<string> ExtractRecommendations(string analysis)
    {
        try
        {
            var recommendations = new List<string>();
            if (string.IsNullOrWhiteSpace(analysis))
                return recommendations;

            var lines = analysis.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            bool inRecommendations = false;
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                if (trimmed.Contains("Recommendation", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.Contains("Action", StringComparison.OrdinalIgnoreCase))
                {
                    inRecommendations = true;
                    continue;
                }

                if (inRecommendations)
                {
                    bool isRecommendation = trimmed.StartsWith("-") || trimmed.StartsWith("*");
                    
                    // Check if it starts with a digit followed by a period (e.g., "1. Recommendation")
                    if (!isRecommendation && trimmed.Length > 0 && char.IsDigit(trimmed[0]) && trimmed.Contains("."))
                    {
                        isRecommendation = true;
                    }
                    
                    if (isRecommendation)
                    {
                        var rec = trimmed.TrimStart('-', '*', ' ', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.');
                        if (!string.IsNullOrWhiteSpace(rec))
                        {
                            recommendations.Add(rec);
                        }
                    }
                }
            }

            return recommendations.Any() ? recommendations : new List<string> { "Review the full analysis for detailed recommendations." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting recommendations from analysis");
            return new List<string> { "Unable to extract recommendations from analysis." };
        }
    }

    private List<string> ExtractIssues(string analysis)
    {
        try
        {
            var issues = new List<string>();
            if (string.IsNullOrWhiteSpace(analysis))
                return issues;

            var lines = analysis.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            bool inIssues = false;
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                if (trimmed.Contains("Issue", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.Contains("Problem", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.Contains("Warning", StringComparison.OrdinalIgnoreCase))
                {
                    inIssues = true;
                    continue;
                }

                if (inIssues && (trimmed.StartsWith("-") || trimmed.StartsWith("*")))
                {
                    var issue = trimmed.TrimStart('-', '*', ' ');
                    if (!string.IsNullOrWhiteSpace(issue))
                    {
                        issues.Add(issue);
                    }
                }
            }

            return issues;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting issues from analysis");
            return new List<string>();
        }
    }
}
