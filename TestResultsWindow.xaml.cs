using System.Windows;
using Tester2_01_GUI.Models;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Tester2_01_GUI
{
    public partial class TestResultsWindow : Window
{
    private TestResult testResult;
    private Test originalTest;
    private Dictionary<int, string> studentAnswers = new Dictionary<int, string>(); // Инициализируем
    private Dictionary<int, string>? correctAnswers; // nullable

    public TestResultsWindow(TestResult result, Test test)
    {
        InitializeComponent();
        testResult = result;
        originalTest = test;
        InitializeAnswers();
        DisplayResults();
    }

    private void InitializeAnswers()
    {
        studentAnswers = testResult.StudentAnswers; // Перезаписываем
        correctAnswers = new Dictionary<int, string>();
            
            // Собираем правильные ответы
            foreach (var question in originalTest.Questions)
            {
                switch (question)
                {
                    case QuestionMultipleChoice mcQuestion:
                        correctAnswers![question.Id] = mcQuestion.CorrectAnswerText; // Добавили !
                        break;
                    case QuestionTextInput tiQuestion:
                        correctAnswers![question.Id] = tiQuestion.CorrectAnswer; // Добавили !
                        break;
                    case QuestionMatching mQuestion:
                        var matches = mQuestion.GetCorrectMatchesList();
                        var matchText = new StringBuilder();
                        foreach (var match in matches)
                        {
                            string leftValue = mQuestion.LeftColumn[match.Key];
                            var rightValues = match.Value.Select(r => mQuestion.RightColumn[r]);
                            matchText.AppendLine($"  {match.Key}. {leftValue} → {string.Join(", ", rightValues)}");
                        }
                        correctAnswers![question.Id] = matchText.ToString(); // Добавили !
                        break;
                }
            }
        }

        private void DisplayResults()
        {
            ResultsTextBlock.Text = $@"Студент: {testResult.StudentName}
Тест: {originalTest.Title}
Автор: {originalTest.Author}
Дата прохождения: {testResult.CompletionDate:dd.MM.yyyy HH:mm}

Набрано баллов: {testResult.Score} из {testResult.MaxScore}
Процент выполнения: {testResult.Percentage:F1}%";

            ResultsProgressBar.Value = testResult.Percentage;
            ProgressTextBlock.Text = $"{testResult.Percentage:F1}%";

            string grade = GetGrade(testResult.Percentage);
            string gradeColor = GetGradeColor(testResult.Percentage);
            GradeTextBlock.Text = $"Оценка: {grade}";
            GradeTextBlock.Foreground = GetGradeBrush(gradeColor);

            DetailsTextBlock.Text = $@"Всего вопросов: {originalTest.Questions.Count}
Отвечено вопросов: {testResult.StudentAnswers.Count}
Пропущено вопросов: {originalTest.Questions.Count - testResult.StudentAnswers.Count}

Статистика по типам вопросов:
{GetQuestionTypeStatistics()}";
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

        private string GetGradeColor(double percentage)
        {
            return percentage switch
            {
                >= 90 => "Green",
                >= 75 => "Blue",
                >= 60 => "Orange",
                >= 40 => "Red",
                _ => "DarkRed"
            };
        }

        private System.Windows.Media.Brush GetGradeBrush(string color)
        {
            return color switch
            {
                "Green" => System.Windows.Media.Brushes.Green,
                "Blue" => System.Windows.Media.Brushes.Blue,
                "Orange" => System.Windows.Media.Brushes.Orange,
                "Red" => System.Windows.Media.Brushes.Red,
                _ => System.Windows.Media.Brushes.DarkRed
            };
        }

        private string GetQuestionTypeStatistics()
        {
            int multipleChoiceCount = originalTest.Questions.OfType<QuestionMultipleChoice>().Count();
            int textInputCount = originalTest.Questions.OfType<QuestionTextInput>().Count();
            int matchingCount = originalTest.Questions.OfType<QuestionMatching>().Count();

            return $"- Вопросы с выбором ответа: {multipleChoiceCount}\n- Вопросы с вводом текста: {textInputCount}\n- Вопросы на сопоставление: {matchingCount}";
        }

        private void SaveResultsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt";
            saveFileDialog.FileName = $"Результаты_{testResult.StudentName}_{originalTest.Title}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            saveFileDialog.Title = "Сохранить результаты как...";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string resultsText = GenerateDetailedResultsText();
                    File.WriteAllText(saveFileDialog.FileName, resultsText, Encoding.UTF8);
                    
                    MessageBox.Show($"Результаты успешно сохранены в файл:\n{saveFileDialog.FileName}", 
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

        private string GenerateDetailedResultsText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== РЕЗУЛЬТАТЫ ТЕСТИРОВАНИЯ ===");
            sb.AppendLine();
            sb.AppendLine($"Студент: {testResult.StudentName}");
            sb.AppendLine($"Тест: {originalTest.Title}");
            sb.AppendLine($"Автор: {originalTest.Author}");
            sb.AppendLine($"Дата прохождения: {testResult.CompletionDate:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine($"Длительность теста: {GetTestDuration()}");
            sb.AppendLine();
            sb.AppendLine($"Набрано баллов: {testResult.Score} из {testResult.MaxScore}");
            sb.AppendLine($"Процент выполнения: {testResult.Percentage:F1}%");
            sb.AppendLine($"Оценка: {GetGrade(testResult.Percentage)}");
            sb.AppendLine();
            sb.AppendLine("=== ОБЩАЯ СТАТИСТИКА ===");
            sb.AppendLine($"Всего вопросов: {originalTest.Questions.Count}");
            sb.AppendLine($"Отвечено: {testResult.StudentAnswers.Count}");
            sb.AppendLine($"Пропущено: {originalTest.Questions.Count - testResult.StudentAnswers.Count}");
            sb.AppendLine();
            sb.AppendLine(GetQuestionTypeStatistics());
            sb.AppendLine();
            sb.AppendLine("=== ДЕТАЛЬНЫЕ РЕЗУЛЬТАТЫ ПО ВОПРОСАМ ===");
            sb.AppendLine();

            int questionNumber = 1;
            foreach (var question in originalTest.Questions)
            {
                sb.AppendLine($"Вопрос #{questionNumber}: {question.QuestionText}");
                sb.AppendLine($"Тип: {question.TypeDisplay}");
                sb.AppendLine($"Баллы: {question.Points}");
                sb.AppendLine();

                bool hasStudentAnswer = studentAnswers.ContainsKey(question.Id);
                bool isCorrect = false;
                string studentAnswerText = hasStudentAnswer ? studentAnswers[question.Id] : "ОТВЕТ НЕ ДАН";
                string correctAnswerText = correctAnswers!.ContainsKey(question.Id) ? correctAnswers[question.Id] : "Нет правильного ответа"; // Добавили !

                // Проверяем правильность ответа
                if (hasStudentAnswer)
                {
                    switch (question)
                    {
                        case QuestionMultipleChoice mcQuestion:
                            if (int.TryParse(studentAnswerText, out int selectedIndex))
                            {
                                isCorrect = selectedIndex == mcQuestion.CorrectOptionIndex;
                                studentAnswerText = mcQuestion.Options[selectedIndex];
                            }
                            break;
                        case QuestionTextInput tiQuestion:
                            isCorrect = tiQuestion.IsAnswerCorrect(studentAnswerText);
                            break;
                        case QuestionMatching:
                            // Для сопоставления пока не проверяем
                            isCorrect = false;
                            break;
                    }
                }

                sb.AppendLine($"Ответ студента: {studentAnswerText}");
                sb.AppendLine($"Правильный ответ: {correctAnswerText}");
                sb.AppendLine($"Результат: {(isCorrect ? "ПРАВИЛЬНО ✓" : "НЕПРАВИЛЬНО ✗")}");
                sb.AppendLine($"Начислено баллов: {(isCorrect ? question.Points : 0)}");
                sb.AppendLine(new string('-', 60));
                sb.AppendLine();

                questionNumber++;
            }

            sb.AppendLine();
            sb.AppendLine("=== ВЫВОД ===");
            sb.AppendLine(GetFinalComment(testResult.Percentage));

            return sb.ToString();
        }

        private string GetTestDuration()
        {
            return "Не указано";
        }

        private string GetFinalComment(double percentage)
        {
            return percentage switch
            {
                >= 90 => "Отличный результат! Студент продемонстрировал прекрасное владение материалом.",
                >= 75 => "Хороший результат. Студент хорошо усвоил основные темы.",
                >= 60 => "Удовлетворительный результат. Рекомендуется повторить некоторые темы.",
                >= 40 => "Неудовлетворительный результат. Требуется дополнительное изучение материала.",
                _ => "Низкий результат. Необходимо серьезно заняться изучением предмета."
            };
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    
    private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
    {
        var detailsWindow = new DetailedResultsWindow(testResult, originalTest);
        detailsWindow.Owner = this;
        detailsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        detailsWindow.ShowDialog();
    }
    }
}