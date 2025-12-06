using System.Windows;
using System.Collections.ObjectModel;
using Tester2_01_GUI.Models;
using System.Linq;
using System.Collections.Generic;

namespace Tester2_01_GUI
{
    public partial class MatchingWindow : Window
    {
        public QuestionMatching? ResultQuestion { get; private set; }
        public ObservableCollection<KeyValueItem> leftItems;
        public ObservableCollection<KeyValueItem> rightItems;
        public Dictionary<string, List<string>> currentMatches;

        public MatchingWindow()
        {
            InitializeComponent();
            leftItems = new ObservableCollection<KeyValueItem>();
            rightItems = new ObservableCollection<KeyValueItem>();
            currentMatches = new Dictionary<string, List<string>>();
            InitializeItems();
            UpdateMatchesInfo();
        }

        private void InitializeItems()
        {
            leftItems.Clear();
            rightItems.Clear();

            leftItems.Add(new KeyValueItem { Key = "1", Value = "C#" });
            leftItems.Add(new KeyValueItem { Key = "2", Value = "Python" });

            rightItems.Add(new KeyValueItem { Key = "A", Value = "Язык программирования от Microsoft" });
            rightItems.Add(new KeyValueItem { Key = "B", Value = "Язык для машинного обучения" });

            LeftItemsListView.ItemsSource = leftItems;
            RightItemsListView.ItemsSource = rightItems;
        }

        public void UpdateMatchesInfo()
        {
            if (currentMatches.Count == 0)
            {
                MatchesInfoTextBlock.Text = "Соответствия не установлены. Нажмите 'Установить соответствия' для настройки.";
            }
            else
            {
                var matchesText = new System.Text.StringBuilder();
                int totalMatches = currentMatches.Sum(m => m.Value.Count);
                matchesText.AppendLine($"Установлено соответствий: {totalMatches}");
                matchesText.AppendLine();

                foreach (var match in currentMatches)
                {
                    string leftItem = leftItems.FirstOrDefault(l => l.Key == match.Key)?.Value ?? "Не найден";
                    var rightItemsText = match.Value.Select(r => 
                        rightItems.FirstOrDefault(ri => ri.Key == r)?.Value ?? "Не найден");
                    
                    matchesText.AppendLine($"{match.Key}. {leftItem} → {string.Join(", ", rightItemsText)}");
                }

                MatchesInfoTextBlock.Text = matchesText.ToString();
            }
        }

        private void AddLeftItemButton_Click(object sender, RoutedEventArgs e)
        {
            string newKey = (leftItems.Count + 1).ToString();
            leftItems.Add(new KeyValueItem { Key = newKey, Value = $"Понятие {newKey}" });
            UpdateMatchesInfo();
        }

        private void RemoveLeftItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (LeftItemsListView.SelectedItem is KeyValueItem selected)
            {
                if (currentMatches.ContainsKey(selected.Key))
                {
                    currentMatches.Remove(selected.Key);
                }
                
                leftItems.Remove(selected);
                UpdateMatchesInfo();
            }
        }

        private void AddRightItemButton_Click(object sender, RoutedEventArgs e)
        {
            string newKey = ((char)('A' + rightItems.Count)).ToString();
            rightItems.Add(new KeyValueItem { Key = newKey, Value = $"Определение {newKey}" });
            UpdateMatchesInfo();
        }

        private void RemoveRightItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (RightItemsListView.SelectedItem is KeyValueItem selected)
            {
                foreach (var leftKey in currentMatches.Keys.ToList())
                {
                    if (currentMatches[leftKey].Contains(selected.Key))
                    {
                        currentMatches[leftKey].Remove(selected.Key);
                        if (currentMatches[leftKey].Count == 0)
                        {
                            currentMatches.Remove(leftKey);
                        }
                    }
                }
                
                rightItems.Remove(selected);
                UpdateMatchesInfo();
            }
        }

        private void EditMatchesButton_Click(object sender, RoutedEventArgs e)
        {
            if (leftItems.Count == 0 || rightItems.Count == 0)
            {
                MessageBox.Show("Добавьте элементы в оба столбца перед установкой соответствий", 
                              "Внимание", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
                return;
            }

            var matchesWindow = new MatchingPairsWindow(
                leftItems.ToDictionary(item => item.Key, item => item.Value),
                rightItems.ToDictionary(item => item.Key, item => item.Value),
                currentMatches
            );
            
            matchesWindow.Owner = this;
            
            if (matchesWindow.ShowDialog() == true)
            {
                currentMatches = matchesWindow.CorrectMatches;
                UpdateMatchesInfo();
                
                int totalMatches = currentMatches.Sum(m => m.Value.Count);
                MessageBox.Show($"Установлено {totalMatches} соответствий", 
                              "Сохранено", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
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

            if (currentMatches.ContainsKey(leftSelected.Key) && 
                currentMatches[leftSelected.Key].Contains(rightSelected.Key))
            {
                currentMatches[leftSelected.Key].Remove(rightSelected.Key);
                
                if (currentMatches[leftSelected.Key].Count == 0)
                {
                    currentMatches.Remove(leftSelected.Key);
                }
                
                UpdateMatchesInfo();
                
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(QuestionTextTextBox.Text))
            {
                MessageBox.Show("Введите текст задания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (leftItems.Count < 1 || rightItems.Count < 1)
            {
                MessageBox.Show("Добавьте хотя бы по 1 элементу в каждом столбце", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ResultQuestion = new QuestionMatching
            {
                QuestionText = QuestionTextTextBox.Text.Trim(),
                Points = int.TryParse(PointsTextBox.Text, out int points) ? points : 2,
                LeftColumn = leftItems.ToDictionary(item => item.Key, item => item.Value),
                RightColumn = rightItems.ToDictionary(item => item.Key, item => item.Value),
                CorrectMatches = currentMatches.ToDictionary(
                    kv => kv.Key, 
                    kv => string.Join(";", kv.Value)
                )
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