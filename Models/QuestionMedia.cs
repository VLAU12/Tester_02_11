using System.Text.Json.Serialization;

namespace Tester2_01_GUI.Models
{
    public class QuestionMedia : QuestionBase
    {
        public MediaType MediaType { get; set; } = MediaType.Image;
        public string MediaPath { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public bool AutoPlay { get; set; } = false;
        public bool Loop { get; set; } = false;
        
        [JsonIgnore]
        public string MediaFileName => System.IO.Path.GetFileName(MediaPath);
        
        public override QuestionBase Clone()
        {
            return new QuestionMedia
            {
                Id = this.Id,
                QuestionText = this.QuestionText,
                Points = this.Points,
                MediaType = this.MediaType,
                MediaPath = this.MediaPath,
                Caption = this.Caption,
                AutoPlay = this.AutoPlay,
                Loop = this.Loop
            };
        }
    }

    public enum MediaType
    {
        Image,
        Video,
        Audio
    }
}