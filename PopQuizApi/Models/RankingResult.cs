namespace PopQuizApi.Models
{
    public class RankingResult
    {
        public int ranking { get; set; }
        public string ParticipantName { get; set; }
        public int Progress { get; set; }
        public int CorrectCount { get; set; }
        public double Accuracy { get; set; }
        public bool Completed { get; set; }
        public double Time { get; set; }
    }
}
