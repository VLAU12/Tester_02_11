using System.Windows;
using Microsoft.Win32;
using Tester2_01_GUI.Services;
using Tester2_01_GUI.Models;
using System;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input; 

namespace Tester2_01_GUI
{
    public partial class MainWindow : Window
    {
        private TestFileService fileService = new TestFileService();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenTestButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Файлы тестов Tester2_01 (*.tft)|*.tft|Все файлы (*.*)|*.*";
            openFileDialog.Title = "Выберите файл теста для прохождения";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string selectedFilePath = openFileDialog.FileName;
                    var test = fileService.LoadTest(selectedFilePath);

                    if (test != null)
                    {
                        var studentNameWindow = new StudentNameWindow(test);
                        studentNameWindow.Owner = this;
                        
                        if (studentNameWindow.ShowDialog() == true)
                        {
                            var testRunnerWindow = new TestRunnerWindow(test, studentNameWindow.StudentName);
                            testRunnerWindow.Owner = this;
                            testRunnerWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                            
                            this.Hide();
                            testRunnerWindow.Closed += (s, args) =>
                            {
                                fileService.CleanupTempFiles(test);
                                this.Show();
                            };
                            
                            testRunnerWindow.Show();
                        }
                        else
                        {
                            fileService.CleanupTempFiles(test);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Не удалось загрузить тест из файла", 
                                      "Ошибка", 
                                      MessageBoxButton.OK, 
                                      MessageBoxImage.Error);
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

        private void ConstructorButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TestConstructorWindow constructorWindow = new TestConstructorWindow();
                constructorWindow.Owner = this;
                constructorWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                
                this.Hide();
                constructorWindow.ShowDialog();
                this.Show();
            }
            catch (Exception ex)
            {
                string errorDetails = $"Ошибка при открытии конструктора: {ex.Message}\n\n" +
                                    $"Тип ошибки: {ex.GetType().Name}\n" +
                                    $"Место ошибки: {ex.StackTrace}";
                
                MessageBox.Show(errorDetails, 
                              "Критическая ошибка", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
                this.Show();
            }
        }
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {

            var helpWindow = new Window
            {
                Title = "Справка по системе TuToR",
                Width = 600,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = Brushes.Black,
                Foreground = Brushes.White,
                ResizeMode = ResizeMode.NoResize
            };


            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(20)
            };

            var stackPanel = new StackPanel();

            var title = new TextBlock
            {
                Text = "📖 Справка по системе TuToR",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stackPanel.Children.Add(title);

            string[] helpSections = new[]
            {
                "🔹 Открыть тест",
                "   • Выберите файл теста (.tft) для прохождения",
                "   • Введите ваше имя перед началом теста",
                "   • Следуйте инструкциям во время прохождения",
                "",
                "🔹 Конструктор тестов",
                "   • Создавайте новые тесты",
                "   • Добавляйте вопросы разных типов:",
                "     - Множественный выбор",
                "     - Текстовый ответ",
                "     - Сопоставление",
                "     - Медиа-вопросы",
                "     - Множественный выбор (несколько ответов)",
                "   • Настраивайте время и баллы",
                "   • Сохраняйте тесты в файлы",
                "",
                "🔹 Типы вопросов",
                "   1. Множественный выбор - выберите один правильный ответ",
                "   2. Множественный выбор (несколько) - отметьте все правильные",
                "   3. Текстовый ответ - введите ответ вручную",
                "   4. Сопоставление - соедините элементы левого и правого столбцов",
                "   5. Медиа-вопросы - вопросы с изображениями, видео или аудио",
                "",
                "🔹 Навигация во время теста",
                "   • Используйте кнопки для перехода между вопросами",
                "   • Пропустите вопрос, если не знаете ответ",
                "   • Следите за временем и прогрессом",
                "   • Завершите тест, когда будете готовы",
                "",
                "🔹 Формат файлов",
                "   • Тесты сохраняются в формате .tft",
                "   • Для переноса теста скопируйте файл .tft",
                "   • Не изменяйте расширение файла вручную",
                "",
                "📞 Поддержка",
                "   При возникновении проблем:",
                "   • Проверьте, что файл теста не поврежден",
                "   • Убедитесь, что медиафайлы доступны",
                "   • Перезапустите приложение"
            };

            foreach (var line in helpSections)
            {
                var textBlock = new TextBlock
                {
                    Text = line,
                    FontSize = line.StartsWith("🔹") ? 16 : 14,
                    FontWeight = line.StartsWith("🔹") ? FontWeights.Bold : FontWeights.Normal,
                    Foreground = line.StartsWith("🔹") ? Brushes.Orange : Brushes.White,
                    Margin = new Thickness(line.StartsWith("   ") ? 20 : 0, 5, 0, 5),
                    TextWrapping = TextWrapping.Wrap
                };
                stackPanel.Children.Add(textBlock);
            }

            var closeButton = new Button
            {
                Content = "Закрыть",
                Width = 100,
                Height = 35,
                Background = Brushes.Orange,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 20, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                Cursor = Cursors.Hand
            };
            closeButton.Click += (s, args) => helpWindow.Close();
            
            stackPanel.Children.Add(closeButton);
            scrollViewer.Content = stackPanel;
            helpWindow.Content = scrollViewer;
            helpWindow.ShowDialog();
        }
    }
}