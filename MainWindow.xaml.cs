using System.Windows;
using Microsoft.Win32;
using Tester2_01_GUI.Services;
using Tester2_01_GUI.Models;
using System;

namespace Tester2_01_GUI
{
    public partial class MainWindow : Window
    {
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
                    
                    var fileService = new TestFileService();
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
                            testRunnerWindow.ShowDialog();
                            this.Show();
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
    }
}