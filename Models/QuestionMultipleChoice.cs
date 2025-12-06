namespace Tester2_01_GUI.Models
{
    public class QuestionMultipleChoice : QuestionBase
    {
        public List<string> Options { get; set; } = new List<string>();
        public int CorrectOptionIndex { get; set; }
        
        public string CorrectAnswerText => 
            Options.Count > CorrectOptionIndex && CorrectOptionIndex >= 0 
                ? Options[CorrectOptionIndex] 
                : "Нет правильного ответа";
        
        public override QuestionBase Clone()
        {
            return new QuestionMultipleChoice
            {
                Id = this.Id,
                QuestionText = this.QuestionText,
                Points = this.Points,
                Options = new List<string>(this.Options),
                CorrectOptionIndex = this.CorrectOptionIndex
            };
        }
    }
}