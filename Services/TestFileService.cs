using System.Text.Json;
using Tester2_01_GUI.Models;
using System.IO;
using System;

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
                string jsonString = JsonSerializer.Serialize(test, _jsonOptions);
                File.WriteAllText(filePath, jsonString);
                Console.WriteLine($"Тест успешно сохранен в файл: {filePath}");
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

                string jsonString = File.ReadAllText(filePath);
                Test? test = JsonSerializer.Deserialize<Test>(jsonString, _jsonOptions);
                
                if (test != null)
                {
                    Console.WriteLine($"Тест '{test.Title}' успешно загружен. Вопросов: {test.Questions.Count}");
                }
                return test;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке теста: {ex.Message}");
                throw;
            }
        }
    }
}