using System.Windows;
using System.Windows.Controls;
using Tester2_01_GUI.Models;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Win32;
using Tester2_01_GUI.Services;
using System;

namespace Tester2_01_GUI
{
    public partial class TestConstructorWindow : Window
    {
        private Test currentTest = new Test();
        private ObservableCollection<QuestionBase> questions = new ObservableCollection<QuestionBase>();
        private TestFileService fileService = new TestFileService();
        private string? currentFilePath = null; // Путь к текущему файлу теста

        public TestConstructorWindow()
        {
            try
            {
                InitializeComponent();
                InitializeTest();
                ConnectEvents();
                UpdateTimeEstimate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в конструкторе TestConstructorWindow: {ex.Message}\n\n" +
                              $"Тип: {ex.GetType().Name}\n\n" +
                              $"Stack Trace:\n{ex.StackTrace}", 
                              "Ошибка инициализации", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
                throw;
            }
        }

        private void InitializeTest()
        {
            currentTest = new Test();
            questions = new ObservableCollection<QuestionBase>();
            QuestionsListView.ItemsSource = questions;
            currentFilePath = null;
            LoadedTestInfoTextBlock.Text = "";
        }

        private void ConnectEvents()
        {
            AddQuestionButton.Click += AddQuestionButton_Click;
            EditQuestionButton.Click += EditQuestionButton_Click;
            DeleteQuestionButton.Click += DeleteQuestionButton_Click;
            SaveTestButton.Click += SaveTestButton_Click;
            LoadTestButton.Click += LoadTestButton_Click;
            ResetTestButton.Click += ResetTestButton_Click;
            MoveUpButton.Click += MoveUpButton_Click;
            MoveDownButton.Click += MoveDownButton_Click;
            CloseButton.Click += CloseButton_Click;
            QuestionsListView.SelectionChanged += QuestionsListView_SelectionChanged;
            
            PerQuestionTimeTextBox.TextChanged += TimeTextBox_TextChanged;
            WholeTestTimeTextBox.TextChanged += TimeTextBox_TextChanged;
            
            NoTimeLimitRadio.Checked += TimeLimitRadio_Checked;
            PerQuestionTimeRadio.Checked += TimeLimitRadio_Checked;
            WholeTestTimeRadio.Checked += TimeLimitRadio_Checked;
        }

        private void LoadTestButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Файлы тестов Tester2_01 (*.tft)|*.tft|Все файлы (*.*)|*.*";
            openFileDialog.Title = "Выберите тест для редактирования";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var loadedTest = fileService.LoadTest(openFileDialog.FileName);
                    if (loadedTest != null)
                    {
                        LoadTestIntoEditor(loadedTest, openFileDialog.FileName);
                        MessageBox.Show($"Тест '{loadedTest.Title}' успешно загружен для редактирования!\n\n" +
                                      $"Вопросов: {loadedTest.Questions.Count}\n" +
                                      $"Автор: {loadedTest.Author}\n" +
                                      $"Максимальный балл: {loadedTest.MaxScore}",
                                      "Загрузка завершена", 
                                      MessageBoxButton.OK, 
                                      MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке теста: {ex.Message}", 
                                  "Ошибка", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Error);
                }
            }
        }

        private void LoadTestIntoEditor(Test test, string filePath)
        {
            currentTest = test;
            currentFilePath = filePath;
            
            // Обновляем поля формы
            TestTitleTextBox.Text = test.Title;
            TeacherTextBox.Text = test.Author;
            GroupTextBox.Text = test.Description.Replace("Тест для группы: ", "");
            
            // Обновляем настройки времени - ИСПРАВЛЯЕМ
            switch (test.TimeLimitType)
            {
                case TestTimeLimitType.None:  // Изменено
                    NoTimeLimitRadio.IsChecked = true;
                    break;
                case TestTimeLimitType.PerQuestion:  // Изменено
                    PerQuestionTimeRadio.IsChecked = true;
                    PerQuestionTimeTextBox.Text = test.TimeLimitPerQuestion.ToString();
                    break;
                case TestTimeLimitType.WholeTest:  // Изменено
                    WholeTestTimeRadio.IsChecked = true;
                    WholeTestTimeTextBox.Text = test.TimeLimitForWholeTest.ToString();
                    break;
            }
            
            // Загружаем вопросы
            questions.Clear();
            foreach (var question in test.Questions)
            {
                questions.Add(question);
            }
            
            // Обновляем UI
            QuestionsListView.Items.Refresh();
            UpdateTimeEstimate();
            
            LoadedTestInfoTextBlock.Text = $"Загружен: {test.Title} (вопросов: {questions.Count})";
        }

        private void ResetTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (questions.Count > 0)
            {
                var result = MessageBox.Show("Сбросить текущий тест? Все несохраненные изменения будут потеряны.", 
                                           "Подтверждение", 
                                           MessageBoxButton.YesNo, 
                                           MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                    return;
            }
            
            InitializeTest();
            UpdateTimeEstimate();
            MessageBox.Show("Тест сброшен. Можно создать новый.", 
                          "Сброс", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Information);
        }

        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (QuestionsListView.SelectedItem is QuestionBase selectedQuestion)
            {
                int currentIndex = questions.IndexOf(selectedQuestion);
                if (currentIndex > 0)
                {
                    questions.Move(currentIndex, currentIndex - 1);
                    UpdateQuestionNumbers();
                    QuestionsListView.SelectedItem = questions[currentIndex - 1];
                    QuestionsListView.Items.Refresh();
                }
            }
        }

        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (QuestionsListView.SelectedItem is QuestionBase selectedQuestion)
            {
                int currentIndex = questions.IndexOf(selectedQuestion);
                if (currentIndex < questions.Count - 1)
                {
                    questions.Move(currentIndex, currentIndex + 1);
                    UpdateQuestionNumbers();
                    QuestionsListView.SelectedItem = questions[currentIndex + 1];
                    QuestionsListView.Items.Refresh();
                }
            }
        }

        private void TimeLimitRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (PerQuestionTimeTextBox == null || WholeTestTimeTextBox == null)
                return;

            if (sender is RadioButton radio)
            {
                PerQuestionTimeTextBox.IsEnabled = radio == PerQuestionTimeRadio;
                WholeTestTimeTextBox.IsEnabled = radio == WholeTestTimeRadio;
                UpdateTimeEstimate();
            }
        }

        private void TimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTimeEstimate();
        }

        private void UpdateTimeEstimate()
        {
            if (questions.Count == 0)
            {
                TimeEstimateTextBlock.Text = "Добавьте вопросы для расчета времени";
                return;
            }

            if (PerQuestionTimeRadio.IsChecked == true && int.TryParse(PerQuestionTimeTextBox.Text, out int perQuestionTime))
            {
                int totalSeconds = perQuestionTime * questions.Count;
                TimeSpan totalTime = TimeSpan.FromSeconds(totalSeconds);
                TimeEstimateTextBlock.Text = $"Примерное время теста: {totalTime:mm\\:ss}";
            }
            else if (WholeTestTimeRadio.IsChecked == true && int.TryParse(WholeTestTimeTextBox.Text, out int wholeTestTime))
            {
                int secondsPerQuestion = wholeTestTime / Math.Max(questions.Count, 1);
                TimeSpan timePerQuestion = TimeSpan.FromSeconds(secondsPerQuestion);
                TimeEstimateTextBlock.Text = $"Примерно {timePerQuestion:mm\\:ss} на вопрос";
            }
            else
            {
                TimeEstimateTextBlock.Text = "Без ограничения времени";
            }
        }

        private void AddQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            var typeWindow = new QuestionTypeWindow();
            typeWindow.Owner = this;
            
            if (typeWindow.ShowDialog() == true)
            {
                switch (typeWindow.SelectedQuestionType)
                {
                    case "Media":
                        CreateMediaQuestion();
                        break;
                    case "MultipleChoice":
                        CreateMultipleChoiceQuestion();
                        break;
                    case "Matching":
                        CreateMatchingQuestion();
                        break;
                    case "TextInput":
                        CreateTextInputQuestion();
                        break;
                    case "MultipleChoiceMulti":
                        CreateMultipleChoiceMultiQuestion();
                        break;
                }
                UpdateTimeEstimate();
            }
        }

        private void CreateMediaQuestion()
        {
            var mediaWindow = new MediaWindow();
            mediaWindow.Owner = this;
            
            if (mediaWindow.ShowDialog() == true && mediaWindow.ResultQuestion != null)
            {
                mediaWindow.ResultQuestion.Id = questions.Count + 1;
                questions.Add(mediaWindow.ResultQuestion);
                QuestionsListView.Items.Refresh();
                
                string mediaType = mediaWindow.ResultQuestion.MediaType switch
                {
                    MediaType.Image => "изображение",
                    MediaType.Video => "видео",
                    MediaType.Audio => "аудио",
                    _ => "медиа"
                };
                
                MessageBox.Show($"Медиа-вопрос добавлен!\n\nТип: {mediaType}\nФайл: {mediaWindow.ResultQuestion.MediaFileName}",
                            "Вопрос добавлен", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
            }
        }
        private void CreateMultipleChoiceMultiQuestion()
        {
            var mcWindow = new MultipleChoiceMultiWindow();
            mcWindow.Owner = this;
            
            if (mcWindow.ShowDialog() == true && mcWindow.ResultQuestion != null)
            {
                mcWindow.ResultQuestion.Id = questions.Count + 1;
                questions.Add(mcWindow.ResultQuestion);
                QuestionsListView.Items.Refresh();
                
                MessageBox.Show($"Вопрос с множественным выбором добавлен!\n\nВариантов: {mcWindow.ResultQuestion.Options.Count}\nПравильных ответов: {mcWindow.ResultQuestion.CorrectOptionIndices.Count}",
                            "Вопрос добавлен", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
            }
        }
        private void DeleteQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            if (QuestionsListView.SelectedItem is QuestionBase selectedQuestion)
            {
                var result = MessageBox.Show($"Удалить вопрос: '{selectedQuestion.QuestionText}'?", 
                                           "Подтверждение удаления", 
                                           MessageBoxButton.YesNo, 
                                           MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    questions.Remove(selectedQuestion);
                    UpdateQuestionNumbers();
                    UpdateTimeEstimate();
                    
                    MessageBox.Show("Вопрос удален", 
                                  "Удаление завершено", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите вопрос для удаления", 
                              "Внимание", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
            }
        }

        private void EditQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            if (QuestionsListView.SelectedItem is QuestionBase selectedQuestion)
            {
                // Клонируем вопрос для редактирования
                var questionToEdit = selectedQuestion.Clone();
                int originalIndex = questions.IndexOf(selectedQuestion);
                
                bool isEdited = false;
                
                // Открываем окно редактирования в зависимости от типа вопроса
                switch (questionToEdit)
                {
                    case QuestionMedia mediaQuestion:
                        isEdited = EditMediaQuestion(mediaQuestion);
                        break;
                    case QuestionMultipleChoice mcQuestion:
                        isEdited = EditMultipleChoiceQuestion(mcQuestion);
                        break;
                    case QuestionTextInput tiQuestion:
                        isEdited = EditTextInputQuestion(tiQuestion);
                        break;
                    case QuestionMatching mQuestion:
                        isEdited = EditMatchingQuestion(mQuestion);
                        break;
                    case QuestionMultipleChoiceMulti mcMultiQuestion:
                        EditMultipleChoiceMultiQuestion(mcMultiQuestion);
                        break;
                    default:
                        MessageBox.Show("Редактирование данного типа вопроса пока не реализовано", 
                                      "Информация", 
                                      MessageBoxButton.OK, 
                                      MessageBoxImage.Information);
                        break;
                }
                
                // Если редактирование прошло успешно, заменяем вопрос
                if (isEdited)
                {
                    questions[originalIndex] = questionToEdit;
                    QuestionsListView.Items.Refresh();
                    UpdateTimeEstimate();
                    
                    MessageBox.Show("Вопрос успешно отредактирован!", 
                                  "Редактирование", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите вопрос для редактирования", 
                              "Внимание", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
            }
        }

        private bool EditMediaQuestion(QuestionMedia question)
        {
            var editWindow = new MediaWindow();
            
            editWindow.QuestionTextTextBox.Text = question.QuestionText;
            editWindow.PointsTextBox.Text = question.Points.ToString();
            editWindow.MediaTypeComboBox.SelectedIndex = (int)question.MediaType;
            editWindow.MediaPathTextBox.Text = question.MediaPath;
            editWindow.CaptionTextBox.Text = question.Caption;
            editWindow.AutoPlayCheckBox.IsChecked = question.AutoPlay;
            editWindow.LoopCheckBox.IsChecked = question.Loop;
            
            editWindow.UpdateMediaPreview();
            editWindow.Owner = this;
            
            if (editWindow.ShowDialog() == true && editWindow.ResultQuestion != null)
            {
                question.QuestionText = editWindow.ResultQuestion.QuestionText;
                question.Points = editWindow.ResultQuestion.Points;
                question.MediaType = editWindow.ResultQuestion.MediaType;
                question.MediaPath = editWindow.ResultQuestion.MediaPath;
                question.Caption = editWindow.ResultQuestion.Caption;
                question.AutoPlay = editWindow.ResultQuestion.AutoPlay;
                question.Loop = editWindow.ResultQuestion.Loop;
                return true;
            }
            
            return false;
        }
        private bool EditMultipleChoiceQuestion(QuestionMultipleChoice question)
        {
            var editWindow = new MultipleChoiceWindow();
            
            // Заполняем поля данными вопроса
            editWindow.QuestionTextTextBox.Text = question.QuestionText;
            editWindow.PointsTextBox.Text = question.Points.ToString();
            
            // Очищаем и добавляем варианты ответов
            editWindow.InitializeOptions();
            editWindow.options.Clear();
            
            for (int i = 0; i < question.Options.Count; i++)
            {
                editWindow.options.Add(new OptionItem 
                { 
                    Text = question.Options[i], 
                    IsCorrect = (i == question.CorrectOptionIndex) 
                });
            }
            
            editWindow.OptionsListView.Items.Refresh();
            editWindow.Owner = this;
            
            if (editWindow.ShowDialog() == true && editWindow.ResultQuestion != null)
            {
                // Обновляем вопрос данными из окна редактирования
                question.QuestionText = editWindow.ResultQuestion.QuestionText;
                question.Points = editWindow.ResultQuestion.Points;
                question.Options = new List<string>(editWindow.ResultQuestion.Options);
                question.CorrectOptionIndex = editWindow.ResultQuestion.CorrectOptionIndex;
                return true;
            }
            
            return false;
        }

        private bool EditMultipleChoiceMultiQuestion(QuestionMultipleChoiceMulti question)
        {
            var editWindow = new MultipleChoiceMultiWindow();
            
            editWindow.QuestionTextTextBox.Text = question.QuestionText;
            editWindow.PointsTextBox.Text = question.Points.ToString();
            
            editWindow.InitializeOptions();
            editWindow.options.Clear();
            
            for (int i = 0; i < question.Options.Count; i++)
            {
                editWindow.options.Add(new OptionItem 
                { 
                    Text = question.Options[i], 
                    IsCorrect = question.CorrectOptionIndices.Contains(i)
                });
            }
            
            editWindow.OptionsListView.Items.Refresh();
            editWindow.Owner = this;
            
            if (editWindow.ShowDialog() == true && editWindow.ResultQuestion != null)
            {
                question.QuestionText = editWindow.ResultQuestion.QuestionText;
                question.Points = editWindow.ResultQuestion.Points;
                question.Options = new List<string>(editWindow.ResultQuestion.Options);
                question.CorrectOptionIndices = new List<int>(editWindow.ResultQuestion.CorrectOptionIndices);
                return true;
            }
            
            return false;
        }

        private bool EditTextInputQuestion(QuestionTextInput question)
        {
            var editWindow = new TextInputWindow();
            
            // Заполняем поля данными вопроса
            editWindow.QuestionTextTextBox.Text = question.QuestionText;
            editWindow.CorrectAnswerTextBox.Text = question.CorrectAnswer;
            editWindow.PointsTextBox.Text = question.Points.ToString();
            editWindow.CaseSensitiveCheckBox.IsChecked = question.IsCaseSensitive;
            
            editWindow.Owner = this;
            
            if (editWindow.ShowDialog() == true && editWindow.ResultQuestion != null)
            {
                // Обновляем вопрос данными из окна редактирования
                question.QuestionText = editWindow.ResultQuestion.QuestionText;
                question.Points = editWindow.ResultQuestion.Points;
                question.CorrectAnswer = editWindow.ResultQuestion.CorrectAnswer;
                question.IsCaseSensitive = editWindow.ResultQuestion.IsCaseSensitive;
                return true;
            }
            
            return false;
        }

        private bool EditMatchingQuestion(QuestionMatching question)
        {
            var editWindow = new MatchingWindow();
            
            // Заполняем поля данными вопроса
            editWindow.QuestionTextTextBox.Text = question.QuestionText;
            editWindow.PointsTextBox.Text = question.Points.ToString();
            
            // Загружаем элементы столбцов
            editWindow.leftItems.Clear();
            editWindow.rightItems.Clear();
            
            foreach (var leftItem in question.LeftColumn)
            {
                editWindow.leftItems.Add(new KeyValueItem { Key = leftItem.Key, Value = leftItem.Value });
            }
            
            foreach (var rightItem in question.RightColumn)
            {
                editWindow.rightItems.Add(new KeyValueItem { Key = rightItem.Key, Value = rightItem.Value });
            }
            
            // Загружаем соответствия
            editWindow.currentMatches = question.GetCorrectMatchesList();
            editWindow.UpdateMatchesInfo();
            
            editWindow.LeftItemsListView.Items.Refresh();
            editWindow.RightItemsListView.Items.Refresh();
            editWindow.Owner = this;
            
            if (editWindow.ShowDialog() == true && editWindow.ResultQuestion != null)
            {
                // Обновляем вопрос данными из окна редактирования
                question.QuestionText = editWindow.ResultQuestion.QuestionText;
                question.Points = editWindow.ResultQuestion.Points;
                question.LeftColumn = new Dictionary<string, string>(editWindow.ResultQuestion.LeftColumn);
                question.RightColumn = new Dictionary<string, string>(editWindow.ResultQuestion.RightColumn);
                question.CorrectMatches = new Dictionary<string, string>(editWindow.ResultQuestion.CorrectMatches);
                return true;
            }
            
            return false;
        }

        private void SaveTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentFilePath == null)
            {
                // Если файл не загружен, предлагаем сохранить как
                SaveAsTest();
            }
            else
            {
                // Сохраняем в текущий файл
                SaveTestToFile(currentFilePath);
            }
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveAsTest();
        }

        private void SaveAsTest()
        {
            if (!ValidateTest())
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Файлы тестов Tester2_01 (*.tft)|*.tft";
            saveFileDialog.FileName = $"{TestTitleTextBox.Text}.tft";
            saveFileDialog.Title = "Сохранить тест как...";

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveTestToFile(saveFileDialog.FileName);
                currentFilePath = saveFileDialog.FileName;
            }
        }

        private bool ValidateTest()
        {
            if (questions.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один вопрос в тест", 
                              "Ошибка", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(TestTitleTextBox.Text))
            {
                MessageBox.Show("Введите название теста", 
                              "Ошибка", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
                return false;
            }

            if (NoTimeLimitRadio.IsChecked == true)
            {
                currentTest.TimeLimitType = TestTimeLimitType.None;
            }
            else if (PerQuestionTimeRadio.IsChecked == true)
            {
                currentTest.TimeLimitType = TestTimeLimitType.PerQuestion;
                if (!int.TryParse(PerQuestionTimeTextBox.Text, out int perQuestionTime) || perQuestionTime <= 0)
                {
                    MessageBox.Show("Введите корректное время на вопрос (положительное число)", 
                                  "Ошибка", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Error);
                    return false;
                }
                currentTest.TimeLimitPerQuestion = perQuestionTime;
            }
            else if (WholeTestTimeRadio.IsChecked == true)
            {
                currentTest.TimeLimitType = TestTimeLimitType.WholeTest;
                if (!int.TryParse(WholeTestTimeTextBox.Text, out int wholeTestTime) || wholeTestTime <= 0)
                {
                    MessageBox.Show("Введите корректное время на тест (положительное число)", 
                                  "Ошибка", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Error);
                    return false;
                }
                currentTest.TimeLimitForWholeTest = wholeTestTime;
            }

            return true;
        }

        private void SaveTestToFile(string filePath)
        {
            try
            {
                currentTest.Title = TestTitleTextBox.Text.Trim();
                currentTest.Author = string.IsNullOrWhiteSpace(TeacherTextBox.Text) 
                    ? "Неизвестный преподаватель" 
                    : TeacherTextBox.Text.Trim();
                currentTest.Description = $"Тест для группы: {(string.IsNullOrWhiteSpace(GroupTextBox.Text) 
                    ? "Не указана" 
                    : GroupTextBox.Text.Trim())}";
                currentTest.CreatedDate = DateTime.Now;
                
                // Настройки времени
                if (NoTimeLimitRadio.IsChecked == true)
                {
                    currentTest.TimeLimitType = TestTimeLimitType.None;
                }
                else if (PerQuestionTimeRadio.IsChecked == true)
                {
                    currentTest.TimeLimitType = TestTimeLimitType.PerQuestion;
                    if (int.TryParse(PerQuestionTimeTextBox.Text, out int perQuestionTime) && perQuestionTime > 0)
                    {
                        currentTest.TimeLimitPerQuestion = perQuestionTime;
                    }
                }
                else if (WholeTestTimeRadio.IsChecked == true)
                {
                    currentTest.TimeLimitType = TestTimeLimitType.WholeTest;
                    if (int.TryParse(WholeTestTimeTextBox.Text, out int wholeTestTime) && wholeTestTime > 0)
                    {
                        currentTest.TimeLimitForWholeTest = wholeTestTime;
                    }
                }
                currentTest.Questions.Clear();
                
                foreach (var question in questions)
                {
                    currentTest.Questions.Add(question.Clone());
                }
                fileService.SaveTest(currentTest, filePath);
                
                string timeInfo = GetTimeLimitInfo();
                
                MessageBox.Show($"Тест успешно сохранен!\n\n" +
                            $"Файл: {filePath}\n" +
                            $"Название: {currentTest.Title}\n" +
                            $"Вопросов: {currentTest.Questions.Count}\n" +
                            $"Автор: {currentTest.Author}\n" +
                            $"Общий балл: {currentTest.MaxScore}{timeInfo}",
                            "Сохранение завершено", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
                
                LoadedTestInfoTextBlock.Text = $"Сохранен: {currentTest.Title} (вопросов: {questions.Count})";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении теста: {ex.Message}\n\n" +
                            $"Подробности: {ex.InnerException?.Message}", 
                            "Ошибка", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);
            }
        }

        private string GetTimeLimitInfo()
        {
            return currentTest.TimeLimitType switch
            {
                TestTimeLimitType.PerQuestion => $"\nВремя на вопрос: {currentTest.TimeLimitPerQuestion} сек.",
                TestTimeLimitType.WholeTest => $"\nВремя на тест: {TimeSpan.FromSeconds(currentTest.TimeLimitForWholeTest):mm\\:ss}",
                _ => "\nОграничение времени: нет"
            };
        }

        private void CreateMultipleChoiceQuestion()
        {
            var mcWindow = new MultipleChoiceWindow();
            mcWindow.Owner = this;
            
            if (mcWindow.ShowDialog() == true && mcWindow.ResultQuestion != null)
            {
                mcWindow.ResultQuestion.Id = questions.Count + 1;
                questions.Add(mcWindow.ResultQuestion);
                QuestionsListView.Items.Refresh();
                
                MessageBox.Show($"Вопрос с выбором ответа добавлен!\n\nВариантов: {mcWindow.ResultQuestion.Options.Count}\nПравильный ответ: {mcWindow.ResultQuestion.CorrectAnswerText}",
                              "Вопрос добавлен", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
            }
        }

        private void CreateTextInputQuestion()
        {
            var tiWindow = new TextInputWindow();
            tiWindow.Owner = this;
            
            if (tiWindow.ShowDialog() == true && tiWindow.ResultQuestion != null)
            {
                tiWindow.ResultQuestion.Id = questions.Count + 1;
                questions.Add(tiWindow.ResultQuestion);
                QuestionsListView.Items.Refresh();
                
                string caseSensitiveInfo = tiWindow.ResultQuestion.IsCaseSensitive ? "с учетом регистра" : "без учета регистра";
                MessageBox.Show($"Вопрос с вводом текста добавлен!\n\nПравильный ответ: '{tiWindow.ResultQuestion.CorrectAnswer}'\nПроверка: {caseSensitiveInfo}",
                              "Вопрос добавлен", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
            }
        }

        private void CreateMatchingQuestion()
        {
            var matchingWindow = new MatchingWindow();
            matchingWindow.Owner = this;
            
            if (matchingWindow.ShowDialog() == true && matchingWindow.ResultQuestion != null)
            {
                matchingWindow.ResultQuestion.Id = questions.Count + 1;
                questions.Add(matchingWindow.ResultQuestion);
                QuestionsListView.Items.Refresh();
                
                MessageBox.Show($"Вопрос на сопоставление добавлен!\n\nЭлементов в левом столбце: {matchingWindow.ResultQuestion.LeftColumn.Count}\nЭлементов в правом столбце: {matchingWindow.ResultQuestion.RightColumn.Count}",
                              "Вопрос добавлен", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (questions.Count > 0)
            {
                var result = MessageBox.Show("Есть несохраненные изменения. Вы уверены, что хотите закрыть конструктор?", 
                                        "Подтверждение", 
                                        MessageBoxButton.YesNo, 
                                        MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }
            
            // Очищаем временные файлы, если тест был загружен
            if (currentTest != null && !string.IsNullOrEmpty(currentTest.TempDirectory))
            {
                fileService.CleanupTempFiles(currentTest);
            }
            
            this.Close();
        }

        private void QuestionsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = QuestionsListView.SelectedItem != null;
            EditQuestionButton.IsEnabled = hasSelection;
            DeleteQuestionButton.IsEnabled = hasSelection;
            MoveUpButton.IsEnabled = hasSelection;
            MoveDownButton.IsEnabled = hasSelection;
        }

        private void UpdateQuestionNumbers()
        {
            for (int i = 0; i < questions.Count; i++)
            {
                questions[i].Id = i + 1;
            }
            QuestionsListView.Items.Refresh();
        }

        private string GetTestStatistics()
        {
            int multipleChoiceCount = questions.OfType<QuestionMultipleChoice>().Count();
            int textInputCount = questions.OfType<QuestionTextInput>().Count();
            int matchingCount = questions.OfType<QuestionMatching>().Count();
            
            return $"Всего вопросов: {questions.Count}\n" +
                   $"• С выбором ответа: {multipleChoiceCount}\n" +
                   $"• С вводом текста: {textInputCount}\n" +
                   $"• На сопоставление: {matchingCount}\n" +
                   $"Общий балл: {currentTest.MaxScore}";
        }
    }
}