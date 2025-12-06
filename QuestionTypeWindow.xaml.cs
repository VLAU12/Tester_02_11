using System.Windows;

namespace Tester2_01_GUI
{
    public partial class QuestionTypeWindow : Window
    {
        public string SelectedQuestionType { get; private set; } = string.Empty;

        public QuestionTypeWindow()
        {
            InitializeComponent();
        }

        private void MultipleChoiceButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedQuestionType = "MultipleChoice";
            this.DialogResult = true;
            this.Close();
        }

        private void MatchingButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedQuestionType = "Matching";
            this.DialogResult = true;
            this.Close();
        }

        private void TextInputButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedQuestionType = "TextInput";
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        private void MultipleChoiceMultiButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedQuestionType = "MultipleChoiceMulti";
            this.DialogResult = true;
            this.Close();
        }
    }
}