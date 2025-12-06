namespace Tester2_01_GUI.Models
{
    public class TestResult
    {
        public string StudentName { get; set; } = "Анонимный студент";
        public DateTime CompletionDate { get; set; } = DateTime.Now;
        public Dictionary<int, string> StudentAnswers { get; set; } = new Dictionary<int, string>();
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public double Percentage => MaxScore > 0 ? (double)Score / MaxScore * 100 : 0;
    }
}