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
                    case QuestionMedia mediaQuestion:
                        sb.AppendLine($"Тип медиа: {mediaQuestion.MediaType}");
                        if (!string.IsNullOrEmpty(mediaQuestion.MediaPath))
                            sb.AppendLine($"Файл: {mediaQuestion.MediaFileName}");
                        if (!string.IsNullOrEmpty(mediaQuestion.Caption))
                            sb.AppendLine($"Подпись: {mediaQuestion.Caption}");
                        break;
                    
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
                        

                        if (hasStudentAnswer)
                        {
                            isCorrect = IsMatchingAnswerCorrect(studentAnswer, mQuestion);
                        }
                        break;

                    case QuestionMultipleChoiceMulti mcMultiQuestion:
                        sb.AppendLine("Варианты ответа:");
                        for (int i = 0; i < mcMultiQuestion.Options.Count; i++)
                        {
                            string prefix = mcMultiQuestion.CorrectOptionIndices.Contains(i) ? "[✓] " : "[ ] ";
                            sb.AppendLine($"  {prefix}{mcMultiQuestion.Options[i]}");
                        }
                        
                        sb.AppendLine($"Правильные индексы: {string.Join(", ", mcMultiQuestion.CorrectOptionIndices)}");
                        
                        if (hasStudentAnswer)
                        {
                            if (studentAnswer.Contains(','))
                            {
                                var selectedIndices = studentAnswer.Split(',')
                                    .Where(s => !string.IsNullOrEmpty(s))
                                    .Select(s => int.Parse(s))
                                    .ToList();
                                
                                isCorrect = mcMultiQuestion.IsAnswerCorrect(selectedIndices);
                                studentAnswer = string.Join(", ", selectedIndices.Select(i => 
                                    i >= 0 && i < mcMultiQuestion.Options.Count ? 
                                    mcMultiQuestion.Options[i] : $"Неверный индекс: {i}"));
                            }
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

            sb.AppendLine("ИТОГОВАЯ СТАТИСТИКА");
            sb.AppendLine(new string('-', 40));
            
            int correctCount = 0;
            int incorrectCount = 0;
            
            foreach (var question in originalTest.Questions)
            {
                if (testResult.StudentAnswers.ContainsKey(question.Id))
                {
                    string studentAnswer = testResult.StudentAnswers[question.Id];
                    bool isQuestionCorrect = false;
                    
                    switch (question)
                    {
                        case QuestionMultipleChoice mcQuestion:
                            if (int.TryParse(studentAnswer, out int selectedIndex))
                            {
                                isQuestionCorrect = selectedIndex == mcQuestion.CorrectOptionIndex;
                            }
                            break;
                        case QuestionTextInput tiQuestion:
                            isQuestionCorrect = tiQuestion.IsAnswerCorrect(studentAnswer);
                            break;
                        case QuestionMatching mQuestion:
                            isQuestionCorrect = IsMatchingAnswerCorrect(studentAnswer, mQuestion);
                            break;
                        case QuestionMultipleChoiceMulti mcMultiQuestion:
                            if (studentAnswer.Contains(','))
                            {
                                var selectedIndices = studentAnswer.Split(',')
                                    .Where(s => !string.IsNullOrEmpty(s))
                                    .Select(s => int.Parse(s))
                                    .ToList();
                                isQuestionCorrect = mcMultiQuestion.IsAnswerCorrect(selectedIndices);
                            }
                            break;
                    }
                    
                    if (isQuestionCorrect)
                        correctCount++;
                    else
                        incorrectCount++;
                }
            }
            
            sb.AppendLine($"Всего вопросов: {originalTest.Questions.Count}");
            sb.AppendLine($"Правильных ответов: {correctCount}");
            sb.AppendLine($"Неправильных ответов: {incorrectCount}");
            sb.AppendLine($"Пропущенных вопросов: {originalTest.Questions.Count - testResult.StudentAnswers.Count}");
            sb.AppendLine();
            sb.AppendLine($"Оценка: {GetGrade(testResult.Percentage)}");

            ResultsDetailsTextBlock.Text = sb.ToString();
        }
        private bool IsMatchingAnswerCorrect(string studentAnswer, QuestionMatching question)
        {
            try
            {
                var studentMatches = new Dictionary<string, List<string>>();
                
                if (string.IsNullOrEmpty(studentAnswer))
                    return false;
                    
                var pairs = studentAnswer.Split(';');
                foreach (var pair in pairs)
                {
                    var parts = pair.Split(':');
                    if (parts.Length == 2)
                    {
                        string leftKey = parts[0];
                        string rightKey = parts[1];
                        
                        if (!studentMatches.ContainsKey(leftKey))
                        {
                            studentMatches[leftKey] = new List<string>();
                        }
                        
                        if (!studentMatches[leftKey].Contains(rightKey))
                        {
                            studentMatches[leftKey].Add(rightKey);
                        }
                    }
                }
                
                var correctMatches = question.GetCorrectMatchesList();
                
                if (studentMatches.Count != correctMatches.Count)
                    return false;
                    
                foreach (var correctMatch in correctMatches)
                {
                    if (!studentMatches.ContainsKey(correctMatch.Key))
                        return false;
                        
                    var studentValues = studentMatches[correctMatch.Key];
                    var correctValues = correctMatch.Value;
                    
                    if (studentValues.Count != correctValues.Count)
                        return false;
                        
                    foreach (var correctValue in correctValues)
                    {
                        if (!studentValues.Contains(correctValue))
                            return false;
                    }
                }
                
                return true;
            }
            catch
            {
                return false;
            }
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
                        case QuestionMatching mQuestion:
                            if (IsMatchingAnswerCorrect(studentAnswer, mQuestion))
                                correctCount++;
                            break;
                        case QuestionMultipleChoiceMulti mcMultiQuestion:
                            if (studentAnswer.Contains(','))
                            {
                                var selectedIndices = studentAnswer.Split(',')
                                    .Where(s => !string.IsNullOrEmpty(s))
                                    .Select(s => int.Parse(s))
                                    .ToList();
                                if (mcMultiQuestion.IsAnswerCorrect(selectedIndices))
                                    correctCount++;
                            }
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
                        case QuestionMatching mQuestion:
                            if (!IsMatchingAnswerCorrect(studentAnswer, mQuestion))
                                incorrectCount++;
                            break;
                        case QuestionMultipleChoiceMulti mcMultiQuestion:
                            if (studentAnswer.Contains(','))
                            {
                                var selectedIndices = studentAnswer.Split(',')
                                    .Where(s => !string.IsNullOrEmpty(s))
                                    .Select(s => int.Parse(s))
                                    .ToList();
                                if (!mcMultiQuestion.IsAnswerCorrect(selectedIndices))
                                    incorrectCount++;
                            }
                            else
                            {
                                incorrectCount++;
                            }
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