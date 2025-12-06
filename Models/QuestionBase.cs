using System.Text.Json.Serialization;

namespace Tester2_01_GUI.Models
{
    [JsonDerivedType(typeof(QuestionMultipleChoice), typeDiscriminator: "multiple_choice")]
    [JsonDerivedType(typeof(QuestionMultipleChoiceMulti), typeDiscriminator: "multiple_choice_multi")] // Добавили
    [JsonDerivedType(typeof(QuestionMatching), typeDiscriminator: "matching")]
    [JsonDerivedType(typeof(QuestionTextInput), typeDiscriminator: "text_input")]
    public abstract class QuestionBase
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int Points { get; set; } = 1;
        public string Type => this.GetType().Name;
    
        public string TypeDisplay
        {
            get
            {
                return Type switch
                {
                    "QuestionMultipleChoice" => "Выбор ответа",
                    "QuestionMultipleChoiceMulti" => "Множественный выбор", // Добавили
                    "QuestionMatching" => "Сопоставление",
                    "QuestionTextInput" => "Ввод текста",
                    _ => Type
                };
            }
        }
        
        public abstract QuestionBase Clone();
    }
}