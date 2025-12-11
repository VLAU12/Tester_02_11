using System.Windows;
using System.Windows.Input;
using Tester2_01_GUI.Models;

namespace Tester2_01_GUI
{
    public partial class StudentNameWindow : Window
    {
        public string StudentName { get; private set; } = string.Empty;

        public StudentNameWindow(Test test)
        {
            InitializeComponent();
            InitializeTestInfo(test);
            
            StudentNameTextBox.Focus();
            StudentNameTextBox.SelectAll();
        }

        private void InitializeTestInfo(Test test)
        {
            string timeInfo = test.TimeLimitType switch
            {
                // ИСПРАВЛЯЕМ: TimeLimitType → TestTimeLimitType
                TestTimeLimitType.PerQuestion => $"\nВремя на вопрос: {test.TimeLimitPerQuestion} сек.",
                TestTimeLimitType.WholeTest => $"\nВремя на тест: {TimeSpan.FromSeconds(test.TimeLimitForWholeTest):mm\\:ss}",
                _ => "\nОграничение времени: нет"
            };
            
            TestInfoTextBlock.Text = $@"Тест: {test.Title}
        Автор: {test.Author}
        Всего вопросов: {test.Questions.Count}
        Максимальный балл: {test.MaxScore}{timeInfo}";
        }

        private void StartTestButton_Click(object sender, RoutedEventArgs e)
        {
            StartTest();
        }

        private void StudentNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                StartTest();
            }
        }

        private void StartTest()
        {
            if (string.IsNullOrWhiteSpace(StudentNameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите ваше имя", 
                              "Внимание", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
                StudentNameTextBox.Focus();
                return;
            }

            StudentName = StudentNameTextBox.Text.Trim();
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}