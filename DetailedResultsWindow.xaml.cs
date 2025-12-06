using System.Windows;
using Tester2_01_GUI.Models;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Tester2_01_GUI
{
    public partial class DetailedResultsWindow : Window
    {
        private TestResult testResult;
        private Test originalTest;

        public DetailedResultsWindow(TestResult result, Test test)
        {
            InitializeComponent();
            testResult = result;
            originalTest = test;
            DisplayDetailedResults();
        }

        private void DisplayDetailedResults()
        {
            HeaderTextBlock.Text = $"{testResult.StudentName} - {originalTest.Title}";
            
            var sb = new StringBuilder();
            sb.AppendLine($"Тест: {originalTest.Title}");
            sb.AppendLine($"Студент: {testResult.StudentName}");
            sb.AppendLine($"Дата: {testResult.CompletionDate:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine($"Результат: {testResult.Score} из {testResult.MaxScore} ({testResult.Percentage:F1}%)");
            sb.AppendLine();
            sb.AppendLine(new string('=', 80));
            sb.AppendLine();

            int questionNumber = 1;
            foreach (var question in originalTest.Questions)
            {
                sb.AppendLine($"ВОПРОС #{questionNumber}");
                sb.AppendLine(new string('-', 40));
                sb.AppendLine($"Текст: {question.QuestionText}");
                sb.AppendLine($"Тип: {question.TypeDisplay}");
                sb.AppendLine($"Баллы: {question.Points}");
                sb.AppendLine();

                bool hasStudentAnswer = testResult.StudentAnswers.ContainsKey(question.Id);
                string studentAnswer = hasStudentAnswer ? testResult.StudentAnswers[question.Id] : "ОТВЕТ НЕ ДАН";
                bool isCorrect = false;

                switch (question)
                {
                    case QuestionMultipleChoice mcQuestion:
                        sb.AppendLine("Варианты ответа:");
                        for (int i = 0; i < mcQuestion.Options.Count; i++)
                        {
                            string prefix = i == mcQuestion.CorrectOptionIndex ? "[✓] " : "[ ] ";
                            sb.AppendLine($"  {prefix}{mcQuestion.Options[i]}");
                        }
                        
                        if (int.TryParse(studentAnswer, out int selectedIndex) && 
                            selectedIndex >= 0 && selectedIndex < mcQuestion.Options.Count)
                        {
                            studentAnswer = mcQuestion.Options[selectedIndex];
                            isCorrect = selectedIndex == mcQuestion.CorrectOptionIndex;
                        }
                        break;

                    case QuestionTextInput tiQuestion:
                        sb.AppendLine($"Правильный ответ: {tiQuestion.CorrectAnswer}");
                        sb.AppendLine($"Регистрозависимость: {(tiQuestion.IsCaseSensitive ? "Да" : "Нет")}");
                        isCorrect = tiQuestion.IsAnswerCorrect(studentAnswer);
                        break;

                    case QuestionMatching mQuestion:
                        sb.AppendLine("Левый столбец:");
                        foreach (var left in mQuestion.LeftColumn)
                        {
                            sb.AppendLine($"  {left.Key}. {left.Value}");
                        }
                        
                        sb.AppendLine("Правый столбец:");
                        foreach (var right in mQuestion.RightColumn)
                        {
                            sb.AppendLine($"  {right.Key}. {right.Value}");
                        }
                        
                        sb.AppendLine("Правильные соответствия:");
                        var correctMatches = mQuestion.GetCorrectMatchesList();
                        foreach (var match in correctMatches)
                        {
                            string leftValue = mQuestion.LeftColumn[match.Key];
                            var rightValues = match.Value.Select(r => mQuestion.RightColumn[r]);
                            sb.AppendLine($"  {match.Key}. {leftValue} → {string.Join(", ", rightValues)}");
                        }
                        break;
                }

                sb.AppendLine();
                sb.AppendLine($"Ответ студента: {studentAnswer}");
                sb.AppendLine($"Статус: {(isCorrect ? "ПРАВИЛЬНО" : "НЕПРАВИЛЬНО")}");
                sb.AppendLine($"Начислено баллов: {(isCorrect ? question.Points : 0)}");
                sb.AppendLine();
                sb.AppendLine(new string('=', 80));
                sb.AppendLine();

                questionNumber++;
            }

            // Итоговая статистика
            sb.AppendLine("ИТОГОВАЯ СТАТИСТИКА");
            sb.AppendLine(new string('-', 40));
            sb.AppendLine($"Всего вопросов: {originalTest.Questions.Count}");
            sb.AppendLine($"Правильных ответов: {CountCorrectAnswers()}");
            sb.AppendLine($"Неправильных ответов: {CountIncorrectAnswers()}");
            sb.AppendLine($"Пропущенных вопросов: {originalTest.Questions.Count - testResult.StudentAnswers.Count}");
            sb.AppendLine();
            sb.AppendLine($"Оценка: {GetGrade(testResult.Percentage)}");

            ResultsDetailsTextBlock.Text = sb.ToString();
        }

        private int CountCorrectAnswers()
        {
            int correctCount = 0;
            foreach (var question in originalTest.Questions)
            {
                if (testResult.StudentAnswers.ContainsKey(question.Id))
                {
                    string studentAnswer = testResult.StudentAnswers[question.Id];
                    
                    switch (question)
                    {
                        case QuestionMultipleChoice mcQuestion:
                            if (int.TryParse(studentAnswer, out int selectedIndex))
                            {
                                if (selectedIndex == mcQuestion.CorrectOptionIndex)
                                    correctCount++;
                            }
                            break;
                        case QuestionTextInput tiQuestion:
                            if (tiQuestion.IsAnswerCorrect(studentAnswer))
                                correctCount++;
                            break;
                    }
                }
            }
            return correctCount;
        }

        private int CountIncorrectAnswers()
        {
            int incorrectCount = 0;
            foreach (var question in originalTest.Questions)
            {
                if (testResult.StudentAnswers.ContainsKey(question.Id))
                {
                    string studentAnswer = testResult.StudentAnswers[question.Id];
                    
                    switch (question)
                    {
                        case QuestionMultipleChoice mcQuestion:
                            if (int.TryParse(studentAnswer, out int selectedIndex))
                            {
                                if (selectedIndex != mcQuestion.CorrectOptionIndex)
                                    incorrectCount++;
                            }
                            else
                            {
                                incorrectCount++;
                            }
                            break;
                        case QuestionTextInput tiQuestion:
                            if (!tiQuestion.IsAnswerCorrect(studentAnswer))
                                incorrectCount++;
                            break;
                    }
                }
            }
            return incorrectCount;
        }

        private string GetGrade(double percentage)
        {
            return percentage switch
            {
                >= 90 => "Отлично (5)",
                >= 75 => "Хорошо (4)",
                >= 60 => "Удовлетворительно (3)",
                >= 40 => "Неудовлетворительно (2)",
                _ => "Плохо (1)"
            };
        }

        private void SaveDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt";
            saveFileDialog.FileName = $"Детальные_результаты_{testResult.StudentName}_{originalTest.Title}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            saveFileDialog.Title = "Сохранить детальные результаты...";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, ResultsDetailsTextBlock.Text, Encoding.UTF8);
                    
                    MessageBox.Show($"Детальные результаты сохранены в файл:\n{saveFileDialog.FileName}", 
                                  "Сохранение", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", 
                                  "Ошибка", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}