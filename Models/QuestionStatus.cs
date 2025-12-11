namespace Tester2_01_GUI.Models
{
    public enum QuestionStatus
    {
        NotAnswered,
        Answered,
        Skipped,
        Current
    }

    public class QuestionNavigationInfo
    {
        public int QuestionIndex { get; set; }
        public QuestionStatus Status { get; set; }
        public int Points { get; set; }
        public string QuestionType { get; set; } = string.Empty;
    }
}