namespace Tester2_01_GUI.Models
{
    public class QuestionMatching : QuestionBase
    {
        public Dictionary<string, string> LeftColumn { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> RightColumn { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> CorrectMatches { get; set; } = new Dictionary<string, string>();
        
        public Dictionary<string, List<string>> GetCorrectMatchesList()
        {
            var result = new Dictionary<string, List<string>>();
            foreach (var match in CorrectMatches)
            {
                result[match.Key] = match.Value.Split(';').ToList();
            }
            return result;
        }
        
        public override QuestionBase Clone()
        {
            return new QuestionMatching
            {
                Id = this.Id,
                QuestionText = this.QuestionText,
                Points = this.Points,
                LeftColumn = new Dictionary<string, string>(this.LeftColumn),
                RightColumn = new Dictionary<string, string>(this.RightColumn),
                CorrectMatches = new Dictionary<string, string>(this.CorrectMatches)
            };
        }
    }
}