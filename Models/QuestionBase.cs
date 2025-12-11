using System.Text.Json.Serialization;

namespace Tester2_01_GUI.Models
{
    [JsonDerivedType(typeof(QuestionMultipleChoice), typeDiscriminator: "multiple_choice")]
    [JsonDerivedType(typeof(QuestionMultipleChoiceMulti), typeDiscriminator: "multiple_choice_multi")]
    [JsonDerivedType(typeof(QuestionMatching), typeDiscriminator: "matching")]
    [JsonDerivedType(typeof(QuestionTextInput), typeDiscriminator: "text_input")]
    [JsonDerivedType(typeof(QuestionMedia), typeDiscriminator: "media")] // Добавили
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
                    "QuestionMultipleChoiceMulti" => "Множественный выбор",
                    "QuestionMatching" => "Сопоставление",
                    "QuestionTextInput" => "Ввод текста",
                    "QuestionMedia" => "Медиаконтент", // Добавили
                    _ => Type
                };
            }
        }
        
        public abstract QuestionBase Clone();
    }
}