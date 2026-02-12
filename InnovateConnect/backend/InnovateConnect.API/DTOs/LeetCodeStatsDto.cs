namespace InnovateConnect.API.DTOs
{
    public class LeetCodeStatsDto
    {
        public string Status { get; set; } = "error";
        public string Message { get; set; } = string.Empty;
        public int TotalSolved { get; set; }
        public int EasySolved { get; set; }
        public int MediumSolved { get; set; }
        public int HardSolved { get; set; }
        public int Ranking { get; set; }
        public double AcceptanceRate { get; set; }
    }
}
