using System.Windows;
using System.Windows.Controls;
using Tester2_01_GUI.Models;
using Tester2_01_GUI.Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;
using System;

namespace Tester2_01_GUI
{
    public partial class TestRunnerWindow : Window
    {
        private Test currentTest;
        private TestResult testResult = new TestResult();
        private int currentQuestionIndex = 0;
        private Dictionary<int, string> studentAnswers = new Dictionary<int, string>();
        
        private DispatcherTimer? questionTimer;
        private DispatcherTimer? testTimer;
        private int timeRemaining;
        private bool isTimerActive = false;
        private DateTime testStartTime;
        private int totalTestTimeSeconds;

        public TestRunnerWindow(Test test, string studentName)
        {
            InitializeComponent();
            currentTest = test;
            InitializeTimers();
            InitializeTest(studentName);
            LoadQuestion(0);
        }

        private void InitializeTimers()
        {
            questionTimer = new DispatcherTimer();
            questionTimer.Interval = TimeSpan.FromSeconds(1);
            questionTimer.Tick += QuestionTimer_Tick;

            testTimer = new DispatcherTimer();
            testTimer.Interval = TimeSpan.FromSeconds(1);
            testTimer.Tick += TestTimer_Tick;
        }

        private void InitializeTest(string studentName)
        {
            testResult = new TestResult
            {
                StudentName = studentName,
                MaxScore = currentTest.MaxScore
            };

            studentAnswers = new Dictionary<int, string>();
            
            TestTitleTextBlock.Text = currentTest.Title;
            TestAuthorTextBlock.Text = $"Автор: {currentTest.Author}";
            
            switch (currentTest.TimeLimitType)
            {
                case TimeLimitType.PerQuestion:
                    TimerTextBlock.Text = $"{currentTest.TimeLimitPerQuestion} сек.";
                    break;
                case TimeLimitType.WholeTest:
                    totalTestTimeSeconds = currentTest.TimeLimitForWholeTest;
                    testStartTime = DateTime.Now;
                    TimerTextBlock.Text = TimeSpan.FromSeconds(totalTestTimeSeconds).ToString(@"mm\:ss");
                    break;
                case TimeLimitType.None:
                    TimerTextBlock.Text = "∞";
                    TimerTextBlock.Foreground = Brushes.Gray;
                    break;
            }
            
            PrevQuestionButton.Click += (s, e) => NavigateToQuestion(currentQuestionIndex - 1);
            NextQuestionButton.Click += (s, e) => NavigateToQuestion(currentQuestionIndex + 1);
            FinishTestButton.Click += FinishTestButton_Click;
            
            UpdateNavigationButtons();
            UpdateProgress();
        }

        private void LoadQuestion(int questionIndex)
        {
            StopTimers();
            
            if (questionIndex < 0 || questionIndex >= currentTest.Questions.Count)
                return;

            currentQuestionIndex = questionIndex;
            var question = currentTest.Questions[questionIndex];

            QuestionCounterTextBlock.Text = $"{questionIndex + 1} / {currentTest.Questions.Count}";
            PointsTextBlock.Text = $"{question.Points}";
            QuestionTextTextBlock.Text = question.QuestionText;

            QuestionContentControl.Content = null;

            switch (question)
            {
                case QuestionMultipleChoice mcQuestion:
                    CreateMultipleChoiceUI(mcQuestion);
                    break;
                case QuestionTextInput tiQuestion:
                    CreateTextInputUI(tiQuestion);
                    break;
                case QuestionMatching mQuestion:
                    CreateMatchingUI(mQuestion);
                    break;
                case QuestionMultipleChoiceMulti mcMultiQuestion:
                    CreateMultipleChoiceMultiUI(mcMultiQuestion);
                    break;
            }

            LoadStudentAnswer(question.Id);

            switch (currentTest.TimeLimitType)
            {
                case TimeLimitType.PerQuestion:
                    StartQuestionTimer(currentTest.TimeLimitPerQuestion);
                    break;
                case TimeLimitType.WholeTest:
                    StartTestTimer();
                    break;
            }

            UpdateNavigationButtons();
            UpdateProgress();
        }
        private void CreateMultipleChoiceMultiUI(QuestionMultipleChoiceMulti question)
        {
            var stackPanel = new StackPanel();

            for (int i = 0; i < question.Options.Count; i++)
            {
                var checkBox = new CheckBox
                {
                    Content = question.Options[i],
                    FontSize = 14,
                    Margin = new Thickness(5),
                    Tag = i // Сохраняем индекс варианта
                };

                stackPanel.Children.Add(checkBox);
            }

            QuestionContentControl.Content = stackPanel;
        }

        private void StartQuestionTimer(int seconds)
        {
            timeRemaining = seconds;
            isTimerActive = true;
            UpdateTimerDisplay();
            questionTimer?.Start();
        }

        private void StartTestTimer()
        {
            isTimerActive = true;
            testTimer?.Start();
        }

        private void StopTimers()
        {
            isTimerActive = false;
            questionTimer?.Stop();
            testTimer?.Stop();
        }

        private void QuestionTimer_Tick(object? sender, EventArgs e)
        {
            if (isTimerActive)
            {
                timeRemaining--;
                UpdateTimerDisplay();

                if (timeRemaining <= 0)
                {
                    StopTimers();
                    TimeExpired();
                }
            }
        }

        private void TestTimer_Tick(object? sender, EventArgs e)
        {
            if (isTimerActive)
            {
                var elapsed = DateTime.Now - testStartTime;
                timeRemaining = totalTestTimeSeconds - (int)elapsed.TotalSeconds;
                
                if (timeRemaining <= 0)
                {
                    StopTimers();
                    TestTimeExpired();
                }
                else
                {
                    UpdateTimerDisplay();
                }
            }
        }

        private void UpdateTimerDisplay()
        {
            string timerText;
            
            if (currentTest.TimeLimitType == TimeLimitType.WholeTest)
            {
                timerText = TimeSpan.FromSeconds(timeRemaining).ToString(@"mm\:ss");
            }
            else
            {
                timerText = $"{timeRemaining} сек.";
            }
            
            TimerTextBlock.Text = timerText;
            
            if (currentTest.TimeLimitType == TimeLimitType.WholeTest)
            {
                double percentage = (double)timeRemaining / totalTestTimeSeconds;
                if (percentage <= 0.1)
                {
                    TimerTextBlock.Foreground = Brushes.Red;
                    TimerTextBlock.FontWeight = FontWeights.Bold;
                }
                else if (percentage <= 0.3)
                {
                    TimerTextBlock.Foreground = Brushes.Orange;
                    TimerTextBlock.FontWeight = FontWeights.SemiBold;
                }
                else
                {
                    TimerTextBlock.Foreground = Brushes.Green;
                    TimerTextBlock.FontWeight = FontWeights.Normal;
                }
            }
            else
            {
                if (timeRemaining <= 10)
                {
                    TimerTextBlock.Foreground = Brushes.Red;
                    TimerTextBlock.FontWeight = FontWeights.Bold;
                }
                else if (timeRemaining <= 30)
                {
                    TimerTextBlock.Foreground = Brushes.Orange;
                    TimerTextBlock.FontWeight = FontWeights.SemiBold;
                }
                else
                {
                    TimerTextBlock.Foreground = Brushes.Green;
                    TimerTextBlock.FontWeight = FontWeights.Normal;
                }
            }
        }

        private void TimeExpired()
        {
            MessageBox.Show("Время на ответ вышло! Автоматический переход к следующему вопросу.",
                          "Время истекло", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Warning);

            SaveCurrentAnswer();
            
            if (currentQuestionIndex < currentTest.Questions.Count - 1)
            {
                NavigateToQuestion(currentQuestionIndex + 1);
            }
            else
            {
                FinishTestButton_Click(null!, null!);
            }
        }

        private void TestTimeExpired()
        {
            MessageBox.Show("Время на прохождение теста истекло! Тест будет завершен автоматически.",
                          "Время вышло", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Warning);

            SaveCurrentAnswer();
            CalculateResults();
            ShowResults();
        }

        private void CreateMultipleChoiceUI(QuestionMultipleChoice question)
        {
            var stackPanel = new StackPanel();

            for (int i = 0; i < question.Options.Count; i++)
            {
                var radioButton = new RadioButton
                {
                    Content = question.Options[i],
                    FontSize = 14,
                    Margin = new Thickness(5),
                    Tag = i
                };

                stackPanel.Children.Add(radioButton);
            }

            QuestionContentControl.Content = stackPanel;
        }

        private void CreateTextInputUI(QuestionTextInput question)
        {
            var stackPanel = new StackPanel();

            var textBox = new TextBox
            {
                FontSize = 14,
                Height = 40,
                Margin = new Thickness(5),
                TextWrapping = TextWrapping.Wrap
            };

            var hintText = new TextBlock
            {
                Text = question.IsCaseSensitive ? 
                    "Внимание: ответ проверяется с учетом регистра!" : 
                    "Ответ можно вводить в любом регистре",
                FontSize = 11,
                FontStyle = FontStyles.Italic,
                Foreground = Brushes.Gray,
                Margin = new Thickness(5, 2, 5, 5)
            };

            stackPanel.Children.Add(textBox);
            stackPanel.Children.Add(hintText);

            QuestionContentControl.Content = stackPanel;
        }

        private void CreateMatchingUI(QuestionMatching question)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var leftStack = new StackPanel();
            foreach (var leftItem in question.LeftColumn)
            {
                var textBlock = new TextBlock
                {
                    Text = $"{leftItem.Key}. {leftItem.Value}",
                    FontSize = 14,
                    Margin = new Thickness(5),
                    VerticalAlignment = VerticalAlignment.Center
                };
                leftStack.Children.Add(textBlock);
            }

            var arrowText = new TextBlock
            {
                Text = "→",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var rightStack = new StackPanel();
            foreach (var leftItem in question.LeftColumn)
            {
                var comboBox = new ComboBox
                {
                    FontSize = 14,
                    Margin = new Thickness(5),
                    Tag = leftItem.Key,
                    ItemsSource = question.RightColumn.Select(r => $"{r.Key}. {r.Value}").ToList()
                };
                rightStack.Children.Add(comboBox);
            }

            Grid.SetColumn(leftStack, 0);
            Grid.SetColumn(arrowText, 1);
            Grid.SetColumn(rightStack, 2);

            grid.Children.Add(leftStack);
            grid.Children.Add(arrowText);
            grid.Children.Add(rightStack);

            QuestionContentControl.Content = grid;
        }

        private void SaveCurrentAnswer()
        {
            var question = currentTest.Questions[currentQuestionIndex];
            string answer = "";

            switch (question)
            {
                case QuestionMultipleChoice:
                    var radioStack = QuestionContentControl.Content as StackPanel;
                    if (radioStack != null)
                    {
                        var selectedRadio = radioStack.Children
                            .OfType<RadioButton>()
                            .FirstOrDefault(r => r.IsChecked == true);
                        
                        if (selectedRadio != null)
                            answer = selectedRadio.Tag?.ToString() ?? "";
                    }
                    break;

                case QuestionTextInput:
                    var textStack = QuestionContentControl.Content as StackPanel;
                    var textBox = textStack?.Children[0] as TextBox;
                    if (textBox != null)
                        answer = textBox.Text;
                    break;

                case QuestionMatching:
                    var matchingGrid = QuestionContentControl.Content as Grid;
                    if (matchingGrid != null)
                    {
                        var rightStack = matchingGrid.Children
                            .OfType<StackPanel>()
                            .FirstOrDefault(s => Grid.GetColumn(s) == 2);
                        
                        if (rightStack != null)
                        {
                            var matches = new List<string>();
                            foreach (var comboBox in rightStack.Children.OfType<ComboBox>())
                            {
                                if (comboBox.SelectedItem != null)
                                {
                                    string leftKey = comboBox.Tag?.ToString() ?? "";
                                    string rightSelection = comboBox.SelectedItem.ToString() ?? "";
                                    if (!string.IsNullOrEmpty(rightSelection))
                                    {
                                        string rightKey = rightSelection.Split('.')[0];
                                        matches.Add($"{leftKey}:{rightKey}");
                                    }
                                }
                            }
                            answer = string.Join(";", matches);
                        }
                    }
                    break;
                case QuestionMultipleChoiceMulti:
                    var checkBoxStack = QuestionContentControl.Content as StackPanel;
                    if (checkBoxStack != null)
                    {
                        var selectedIndices = checkBoxStack.Children
                            .OfType<CheckBox>()
                            .Where(cb => cb.IsChecked == true)
                            .Select(cb => cb.Tag?.ToString())
                            .Where(tag => tag != null)
                            .Select(tag => int.Parse(tag))
                            .ToList();
                        
                        answer = string.Join(",", selectedIndices);
                    }
                    break;    
            }

            if (!string.IsNullOrEmpty(answer))
                studentAnswers[question.Id] = answer;
        }

        private void LoadStudentAnswer(int questionId)
        {
            if (studentAnswers.ContainsKey(questionId))
            {
                var question = currentTest.Questions[currentQuestionIndex];
                string savedAnswer = studentAnswers[questionId];

                switch (question)
                {
                    case QuestionMultipleChoice mcQuestion:
                        var radioStack = QuestionContentControl.Content as StackPanel;
                        if (radioStack != null && int.TryParse(savedAnswer, out int optionIndex))
                        {
                            if (optionIndex >= 0 && optionIndex < radioStack.Children.Count)
                            {
                                var radioButton = radioStack.Children[optionIndex] as RadioButton;
                                if (radioButton != null)
                                    radioButton.IsChecked = true;
                            }
                        }
                        break;

                    case QuestionTextInput:
                        var textStack = QuestionContentControl.Content as StackPanel;
                        var textBox = textStack?.Children[0] as TextBox;
                        if (textBox != null)
                            textBox.Text = savedAnswer;
                        break;

                    case QuestionMatching mQuestion:
                        break;
                }
            }
        }

        private void NavigateToQuestion(int newIndex)
        {
            SaveCurrentAnswer();
            LoadQuestion(newIndex);
        }

        private void UpdateNavigationButtons()
        {
            PrevQuestionButton.IsEnabled = currentQuestionIndex > 0;
            NextQuestionButton.IsEnabled = currentQuestionIndex < currentTest.Questions.Count - 1;
            FinishTestButton.IsEnabled = true;
        }

        private void UpdateProgress()
        {
            double progress = currentTest.Questions.Count > 0 ? 
                (double)(currentQuestionIndex + 1) / currentTest.Questions.Count * 100 : 0;
            
            ProgressBar.Value = progress;
            ProgressTextBlock.Text = $"{progress:F0}%";
        }

        private void FinishTestButton_Click(object sender, RoutedEventArgs e)
        {
            StopTimers();
            SaveCurrentAnswer();

            var result = MessageBox.Show("Вы уверены, что хотите завершить тест?\n\nПосле завершения будут показаны результаты.",
                                       "Завершение теста", 
                                       MessageBoxButton.YesNo, 
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                CalculateResults();
                ShowResults();
            }
        }

        private void CalculateResults()
        {
            int totalScore = 0;

            foreach (var question in currentTest.Questions)
            {
                if (studentAnswers.ContainsKey(question.Id))
                {
                    string studentAnswer = studentAnswers[question.Id];
                    
                    switch (question)
                    {
                        case QuestionMultipleChoice mcQuestion:
                            if (int.TryParse(studentAnswer, out int selectedIndex) && 
                                selectedIndex == mcQuestion.CorrectOptionIndex)
                            {
                                totalScore += mcQuestion.Points;
                            }
                            break;

                        case QuestionTextInput tiQuestion:
                            if (tiQuestion.IsAnswerCorrect(studentAnswer))
                            {
                                totalScore += tiQuestion.Points;
                            }
                            break;

                        case QuestionMatching mQuestion:
                            // Проверка сопоставлений
                            if (IsMatchingAnswerCorrect(studentAnswer, mQuestion))
                            {
                                totalScore += mQuestion.Points;
                            }
                            break;
                        case QuestionMultipleChoiceMulti mcMultiQuestion:
                            if (studentAnswer.Contains(','))
                            {
                                var selectedIndices = studentAnswer.Split(',')
                                    .Where(s => !string.IsNullOrEmpty(s))
                                    .Select(s => int.Parse(s))
                                    .ToList();
                                
                                if (mcMultiQuestion.IsAnswerCorrect(selectedIndices))
                                {
                                    totalScore += mcMultiQuestion.Points;
                                }
                            }
                            break;
                    }
                }
            }

            testResult.Score = totalScore;
            testResult.StudentAnswers = new Dictionary<int, string>(studentAnswers);
        }

        private void ShowResults()
        {
            var resultsWindow = new TestResultsWindow(testResult, currentTest);
            resultsWindow.Owner = this;
            resultsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            
            resultsWindow.ShowDialog();
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            StopTimers();
            base.OnClosed(e);
        }
        private bool IsMatchingAnswerCorrect(string studentAnswer, QuestionMatching question)
        {
            try
            {
                // Формат ответа студента: "1:A;2:B;3:B;4:A"
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
                
                // Получаем правильные соответствия
                var correctMatches = question.GetCorrectMatchesList();
                
                // Проверяем, совпадают ли все соответствия
                if (studentMatches.Count != correctMatches.Count)
                    return false;
                    
                foreach (var correctMatch in correctMatches)
                {
                    if (!studentMatches.ContainsKey(correctMatch.Key))
                        return false;
                        
                    var studentValues = studentMatches[correctMatch.Key];
                    var correctValues = correctMatch.Value;
                    
                    // Проверяем, что все правильные значения есть в ответе студента
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
    }
}