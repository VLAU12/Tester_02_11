namespace Tester2_01_GUI.Models
{
    public class QuestionTextInput : QuestionBase
    {
        public string CorrectAnswer { get; set; } = string.Empty;
        public bool IsCaseSensitive { get; set; } = false;
        
        public bool IsAnswerCorrect(string studentAnswer)
        {
            if (IsCaseSensitive)
                return studentAnswer.Trim() == CorrectAnswer.Trim();
            else
                return studentAnswer.Trim().ToLower() == CorrectAnswer.Trim().ToLower();
        }
        
        public override QuestionBase Clone()
        {
            return new QuestionTextInput
            {
                Id = this.Id,
                QuestionText = this.QuestionText,
                Points = this.Points,
                CorrectAnswer = this.CorrectAnswer,
                IsCaseSensitive = this.IsCaseSensitive
            };
        }
    }
}