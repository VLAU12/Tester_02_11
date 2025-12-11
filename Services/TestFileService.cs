using System.Text.Json;
using Tester2_01_GUI.Models;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Compression;

namespace Tester2_01_GUI.Services
{
    public class TestFileService
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public TestFileService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public void SaveTest(Test test, string filePath)
        {
            try
            {
                string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDirectory);
                string jsonFilePath = Path.Combine(tempDirectory, "test.json");
                string jsonString = JsonSerializer.Serialize(test, _jsonOptions);
                File.WriteAllText(jsonFilePath, jsonString);
                string mediaDirectory = Path.Combine(tempDirectory, "media");
                Directory.CreateDirectory(mediaDirectory);
                var mediaPathMapping = new Dictionary<string, string>();
                
                foreach (var question in test.Questions)
                {
                    if (question is QuestionMedia mediaQuestion && 
                        !string.IsNullOrEmpty(mediaQuestion.MediaPath) &&
                        File.Exists(mediaQuestion.MediaPath))
                    {
                        string originalPath = mediaQuestion.MediaPath;
                        string fileName = Path.GetFileName(originalPath);
                        string newFileName = GetUniqueFileName(mediaDirectory, fileName);
                        string newFilePath = Path.Combine(mediaDirectory, newFileName);
                        File.Copy(originalPath, newFilePath, true);
                        mediaPathMapping[originalPath] = $"media/{newFileName}";
                    }
                }
                var testWithUpdatedPaths = UpdateMediaPaths(test, mediaPathMapping);
                jsonString = JsonSerializer.Serialize(testWithUpdatedPaths, _jsonOptions);
                File.WriteAllText(jsonFilePath, jsonString);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                
                ZipFile.CreateFromDirectory(tempDirectory, filePath);
                Directory.Delete(tempDirectory, true);
                
                Console.WriteLine($"Тест успешно сохранен в архив: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении теста: {ex.Message}");
                throw;
            }
        }

        public Test? LoadTest(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"Файл теста не найден: {filePath}");
                string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDirectory);
                ZipFile.ExtractToDirectory(filePath, tempDirectory);
                string jsonFilePath = Path.Combine(tempDirectory, "test.json");
                if (!File.Exists(jsonFilePath))
                {
                    throw new FileNotFoundException("В архиве не найден файл test.json");
                }
                
                string jsonString = File.ReadAllText(jsonFilePath);
                Test? test = JsonSerializer.Deserialize<Test>(jsonString, _jsonOptions);
                
                if (test != null)
                {
                    string mediaDirectory = Path.Combine(tempDirectory, "media");
                    if (Directory.Exists(mediaDirectory))
                    {
                        foreach (var question in test.Questions)
                        {
                            if (question is QuestionMedia mediaQuestion && 
                                !string.IsNullOrEmpty(mediaQuestion.MediaPath))
                            {
                                string fileName = Path.GetFileName(mediaQuestion.MediaPath);
                                string tempMediaPath = Path.Combine(mediaDirectory, fileName);
                                
                                if (File.Exists(tempMediaPath))
                                {
                                    mediaQuestion.MediaPath = tempMediaPath;
                                }
                            }
                        }
                    }
                    test.TempDirectory = tempDirectory;
                    
                    Console.WriteLine($"Тест '{test.Title}' успешно загружен. Вопросов: {test.Questions.Count}");
                }
                else
                {
                    Directory.Delete(tempDirectory, true);
                }
                
                return test;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке теста: {ex.Message}");
                throw;
            }
        }

        public void CleanupTempFiles(Test test)
        {
            if (test == null || string.IsNullOrEmpty(test.TempDirectory))
                return;
            
            try
            {
                if (Directory.Exists(test.TempDirectory))
                {
                    Directory.Delete(test.TempDirectory, true);
                    Console.WriteLine($"Очищена временная директория: {test.TempDirectory}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при очистке временных файлов: {ex.Message}");
            }
        }

        private Test UpdateMediaPaths(Test originalTest, Dictionary<string, string> pathMapping)
        {
            var clonedTest = new Test
            {
                Title = originalTest.Title,
                Description = originalTest.Description,
                Author = originalTest.Author,
                CreatedDate = originalTest.CreatedDate,
                TimeLimitType = originalTest.TimeLimitType,
                TimeLimitPerQuestion = originalTest.TimeLimitPerQuestion,
                TimeLimitForWholeTest = originalTest.TimeLimitForWholeTest
            };

            foreach (var question in originalTest.Questions)
            {
                var clonedQuestion = question.Clone();
                if (clonedQuestion is QuestionMedia mediaQuestion && 
                    !string.IsNullOrEmpty(mediaQuestion.MediaPath))
                {
                    string originalPath = mediaQuestion.MediaPath;
                    
                    if (pathMapping.TryGetValue(originalPath, out string newRelativePath))
                    {
                        mediaQuestion.MediaPath = newRelativePath; 
                    }
                    else if (File.Exists(originalPath))
                    {
                        string fileName = Path.GetFileName(originalPath);
                        mediaQuestion.MediaPath = $"media/{fileName}";
                    }
                }
                
                clonedTest.Questions.Add(clonedQuestion);
            }

            return clonedTest;
        }

        private string GetUniqueFileName(string directory, string baseFileName)
        {
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(baseFileName);
            string extension = Path.GetExtension(baseFileName);
            string newFileName = baseFileName;
            
            int counter = 1;
            while (File.Exists(Path.Combine(directory, newFileName)))
            {
                newFileName = $"{fileNameWithoutExt}_{counter}{extension}";
                counter++;
            }
            
            return newFileName;
        }
    }
}