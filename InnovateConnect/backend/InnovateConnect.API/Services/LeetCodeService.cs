using System.Text;
using System.Text.Json;
using InnovateConnect.API.DTOs;

namespace InnovateConnect.API.Services
{
    public interface ILeetCodeService
    {
        Task<LeetCodeStatsDto> GetStatsAsync(string username);
    }

    public class LeetCodeService : ILeetCodeService
    {
        private readonly HttpClient _httpClient;

        public LeetCodeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://leetcode.com/");
        }

        public async Task<LeetCodeStatsDto> GetStatsAsync(string username)
        {
            var query = new
            {
                query = @"
                query userProblemsSolved($username: String!) {
                    allQuestionsCount {
                        difficulty
                        count
                    }
                    matchedUser(username: $username) {
                        username
                        submitStats: submitStatsGlobal {
                            acSubmissionNum {
                                difficulty
                                count
                                submissions
                            }
                        }
                        profile {
                            ranking
                        }
                    }
                }",
                variables = new { username }
            };

            var content = new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json");
            
            try 
            {
                // LeetCode requires User-Agent
                var request = new HttpRequestMessage(HttpMethod.Post, "graphql");
                request.Content = content;
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                request.Headers.Add("Referer", $"https://leetcode.com/{username}");

                var response = await _httpClient.SendAsync(request);
                Console.WriteLine($"LeetCode API Response Status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"LeetCode API Error Content: {errorContent}");
                    return new LeetCodeStatsDto { Status = "error", Message = $"LeetCode API returned {response.StatusCode}" };
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"LeetCode API Response JSON: {jsonString.Substring(0, Math.Min(jsonString.Length, 500))}...");
                
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;
                
                if (root.TryGetProperty("errors", out _))
                {
                     return new LeetCodeStatsDto { Status = "error", Message = "User not found or API error" };
                }

                var data = root.GetProperty("data");
                var matchedUser = data.GetProperty("matchedUser");

                if (matchedUser.ValueKind == JsonValueKind.Null)
                {
                    return new LeetCodeStatsDto { Status = "error", Message = "User not found" };
                }

                var submitStats = matchedUser.GetProperty("submitStats").GetProperty("acSubmissionNum");
                var profile = matchedUser.GetProperty("profile");

                int total = 0, easy = 0, medium = 0, hard = 0;

                foreach (var item in submitStats.EnumerateArray())
                {
                    var diff = item.GetProperty("difficulty").GetString();
                    var count = item.GetProperty("count").GetInt32();

                    if (diff == "All") total = count;
                    else if (diff == "Easy") easy = count;
                    else if (diff == "Medium") medium = count;
                    else if (diff == "Hard") hard = count;
                }

                return new LeetCodeStatsDto
                {
                    Status = "success",
                    TotalSolved = total,
                    EasySolved = easy,
                    MediumSolved = medium,
                    HardSolved = hard,
                    Ranking = profile.GetProperty("ranking").GetInt32()
                };
            }
            catch (Exception ex)
            {
                return new LeetCodeStatsDto { Status = "error", Message = ex.Message };
            }
        }
    }
}
