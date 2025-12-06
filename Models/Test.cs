using System.Text.Json.Serialization;

namespace Tester2_01_GUI.Models
{
    public class Test
    {
        public string Title { get; set; } = "Без названия";
        public string Description { get; set; } = string.Empty;
        public string Author { get; set; } = "Неизвестный автор";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public List<QuestionBase> Questions { get; set; } = new List<QuestionBase>();
        
        [JsonIgnore]
        public int MaxScore => Questions.Sum(q => q.Points);
        
        public TimeLimitType TimeLimitType { get; set; } = TimeLimitType.None;
        public int TimeLimitPerQuestion { get; set; } = 0;
        public int TimeLimitForWholeTest { get; set; } = 0;
    }

    public enum TimeLimitType
    {
        None,
        PerQuestion,
        WholeTest
    }
}