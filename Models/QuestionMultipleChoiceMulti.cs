namespace Tester2_01_GUI.Models
{
    public class QuestionMultipleChoiceMulti : QuestionBase
    {
        public List<string> Options { get; set; } = new List<string>();
        public List<int> CorrectOptionIndices { get; set; } = new List<int>();
        
        public List<string> CorrectAnswersText => 
            CorrectOptionIndices
                .Where(i => i >= 0 && i < Options.Count)
                .Select(i => Options[i])
                .ToList();
        
        public override QuestionBase Clone()
        {
            return new QuestionMultipleChoiceMulti
            {
                Id = this.Id,
                QuestionText = this.QuestionText,
                Points = this.Points,
                Options = new List<string>(this.Options),
                CorrectOptionIndices = new List<int>(this.CorrectOptionIndices)
            };
        }
        
        public bool IsAnswerCorrect(List<int> selectedIndices)
        {
            if (selectedIndices.Count != CorrectOptionIndices.Count)
                return false;
                
            foreach (var correctIndex in CorrectOptionIndices)
            {
                if (!selectedIndices.Contains(correctIndex))
                    return false;
            }
            
            return true;
        }
    }
}