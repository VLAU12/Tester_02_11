using System.Windows;
using Tester2_01_GUI.Models;

namespace Tester2_01_GUI
{
    public partial class TextInputWindow : Window
    {
        public QuestionTextInput? ResultQuestion { get; private set; }

        public TextInputWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(QuestionTextTextBox.Text))
            {
                MessageBox.Show("Введите текст вопроса", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(CorrectAnswerTextBox.Text))
            {
                MessageBox.Show("Введите правильный ответ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ResultQuestion = new QuestionTextInput
            {
                QuestionText = QuestionTextTextBox.Text.Trim(),
                CorrectAnswer = CorrectAnswerTextBox.Text.Trim(),
                Points = int.TryParse(PointsTextBox.Text, out int points) ? points : 1,
                IsCaseSensitive = CaseSensitiveCheckBox.IsChecked ?? false
            };

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