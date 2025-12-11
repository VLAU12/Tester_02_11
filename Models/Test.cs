using System.Text.Json.Serialization;
using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;

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
        
        public TestTimeLimitType TimeLimitType { get; set; } = TestTimeLimitType.None;
        public int TimeLimitPerQuestion { get; set; } = 0;
        public int TimeLimitForWholeTest { get; set; } = 0;

        [JsonIgnore]
        public string TempDirectory { get; set; } = string.Empty;

        public bool HasMediaFiles()
        {
            return Questions.Any(q => q is QuestionMedia mediaQuestion && 
                                    !string.IsNullOrEmpty(mediaQuestion.MediaPath));
        }

        public List<string> GetMediaFilePaths()
        {
            var paths = new List<string>();
            foreach (var question in Questions)
            {
                if (question is QuestionMedia mediaQuestion && 
                    !string.IsNullOrEmpty(mediaQuestion.MediaPath))
                {
                    paths.Add(mediaQuestion.MediaPath);
                }
            }
            return paths;
        }
    }

    public enum TestTimeLimitType
    {
        None,
        PerQuestion,
        WholeTest
    }
}