using System.Windows;
using System.Collections.ObjectModel;
using Tester2_01_GUI.Models;
using System.Linq;

namespace Tester2_01_GUI
{
    public partial class MultipleChoiceMultiWindow : Window
    {
        public QuestionMultipleChoiceMulti? ResultQuestion { get; private set; }
        public ObservableCollection<OptionItem> options;

        public MultipleChoiceMultiWindow()
        {
            InitializeComponent();
            options = new ObservableCollection<OptionItem>();
            InitializeOptions();
        }

        public void InitializeOptions()
        {
            options.Clear();
            options.Add(new OptionItem { Text = "Вариант 1", IsCorrect = true });
            options.Add(new OptionItem { Text = "Вариант 2", IsCorrect = false });
            options.Add(new OptionItem { Text = "Вариант 3", IsCorrect = true });
            options.Add(new OptionItem { Text = "Вариант 4", IsCorrect = false });
            
            OptionsListView.ItemsSource = options;
        }

        private void AddOptionButton_Click(object sender, RoutedEventArgs e)
        {
            options.Add(new OptionItem { Text = $"Вариант {options.Count + 1}", IsCorrect = false });
        }

        private void RemoveOptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (OptionsListView.SelectedItem is OptionItem selectedOption)
            {
                options.Remove(selectedOption);
            }
            else
            {
                MessageBox.Show("Выберите вариант для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(QuestionTextTextBox.Text))
            {
                MessageBox.Show("Введите текст вопроса", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (options.Count < 2)
            {
                MessageBox.Show("Добавьте хотя бы 2 варианта ответа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int correctCount = options.Count(o => o.IsCorrect);
            if (correctCount == 0)
            {
                MessageBox.Show("Выберите хотя бы один правильный вариант", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ResultQuestion = new QuestionMultipleChoiceMulti
            {
                QuestionText = QuestionTextTextBox.Text.Trim(),
                Points = int.TryParse(PointsTextBox.Text, out int points) ? points : 2,
                Options = options.Select(o => o.Text).ToList(),
                CorrectOptionIndices = options
                    .Select((o, index) => new { o.IsCorrect, index })
                    .Where(x => x.IsCorrect)
                    .Select(x => x.index)
                    .ToList()
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