using System.Windows;
using System.Collections.Generic;
using System.Linq;
using Tester2_01_GUI.Models;

namespace Tester2_01_GUI
{
    public partial class MatchingPairsWindow : Window
    {
        public Dictionary<string, List<string>> CorrectMatches { get; private set; }
        private Dictionary<string, string> leftColumn;
        private Dictionary<string, string> rightColumn;
        private Dictionary<string, List<string>> tempMatches;

        public MatchingPairsWindow(Dictionary<string, string> left, 
                                 Dictionary<string, string> right,
                                 Dictionary<string, List<string>>? existingMatches = null)
        {
            InitializeComponent();
            leftColumn = left;
            rightColumn = right;
            tempMatches = existingMatches != null ? 
                new Dictionary<string, List<string>>(existingMatches) : 
                new Dictionary<string, List<string>>();
            CorrectMatches = new Dictionary<string, List<string>>();
            
            InitializeUI();
        }

        private void InitializeUI()
        {
            LeftItemsListView.ItemsSource = leftColumn.Select(kv => new KeyValueItem { Key = kv.Key, Value = kv.Value });
            RightItemsListView.ItemsSource = rightColumn.Select(kv => new KeyValueItem { Key = kv.Key, Value = kv.Value });
            
            UpdateMatchesDisplay();
        }

        private void AddMatchButton_Click(object sender, RoutedEventArgs e)
        {
            var leftSelected = LeftItemsListView.SelectedItem as KeyValueItem;
            var rightSelected = RightItemsListView.SelectedItem as KeyValueItem;

            if (leftSelected == null || rightSelected == null)
            {
                MessageBox.Show("Выберите элемент из левого и правого столбцов", 
                              "Внимание", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
                return;
            }

            // Добавляем соответствие (теперь разрешены дубликаты)
            if (!tempMatches.ContainsKey(leftSelected.Key))
            {
                tempMatches[leftSelected.Key] = new List<string>();
            }

            // Проверяем, не добавлено ли уже это соответствие
            if (!tempMatches[leftSelected.Key].Contains(rightSelected.Key))
            {
                tempMatches[leftSelected.Key].Add(rightSelected.Key);
                UpdateMatchesDisplay();
                
                MessageBox.Show($"Добавлено соответствие: {leftSelected.Key}. {leftSelected.Value} → {rightSelected.Key}. {rightSelected.Value}",
                              "Сохранено", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Это соответствие уже установлено", 
                              "Внимание", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
            }
        }

        private void RemoveSelectedMatchButton_Click(object sender, RoutedEventArgs e)
        {
            var leftSelected = LeftItemsListView.SelectedItem as KeyValueItem;
            var rightSelected = RightItemsListView.SelectedItem as KeyValueItem;

            if (leftSelected == null || rightSelected == null)
            {
                MessageBox.Show("Выберите соответствие для удаления", 
                              "Внимание", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
                return;
            }

            if (tempMatches.ContainsKey(leftSelected.Key) && 
                tempMatches[leftSelected.Key].Contains(rightSelected.Key))
            {
                tempMatches[leftSelected.Key].Remove(rightSelected.Key);
                
                // Если для левого элемента не осталось соответствий, удаляем запись
                if (tempMatches[leftSelected.Key].Count == 0)
                {
                    tempMatches.Remove(leftSelected.Key);
                }
                
                UpdateMatchesDisplay();
                
                MessageBox.Show("Соответствие удалено", 
                              "Удалено", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Выбранное соответствие не найдено", 
                              "Внимание", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
            }
        }

        private void ClearMatchesButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Очистить все установленные соответствия?", 
                                       "Подтверждение", 
                                       MessageBoxButton.YesNo, 
                                       MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                tempMatches.Clear();
                UpdateMatchesDisplay();
            }
        }

        private void UpdateMatchesDisplay()
        {
            var matchesText = new System.Text.StringBuilder();
            matchesText.AppendLine("Установленные соответствия:");

            if (tempMatches.Count == 0)
            {
                matchesText.AppendLine("Нет установленных соответствий");
            }
            else
            {
                int totalMatches = tempMatches.Sum(m => m.Value.Count);
                matchesText.AppendLine($"Всего соответствий: {totalMatches}");
                matchesText.AppendLine();

                foreach (var match in tempMatches)
                {
                    string leftValue = leftColumn[match.Key];
                    var rightValues = match.Value.Select(r => $"{r}. {rightColumn[r]}");
                    matchesText.AppendLine($"{match.Key}. {leftValue} → {string.Join(", ", rightValues)}");
                }
            }

            CurrentMatchesTextBlock.Text = matchesText.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Убрали проверку на количество соответствий - теперь можно любое количество
            if (tempMatches.Count == 0)
            {
                var result = MessageBox.Show("Не установлено ни одного соответствия. Продолжить сохранение?", 
                                           "Подтверждение", 
                                           MessageBoxButton.YesNo, 
                                           MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                    return;
            }

            CorrectMatches = new Dictionary<string, List<string>>(tempMatches);
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