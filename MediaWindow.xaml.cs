using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Tester2_01_GUI.Models;
using System.IO;

namespace Tester2_01_GUI
{
    public partial class MediaWindow : Window
    {
        public QuestionMedia? ResultQuestion { get; private set; }
        
        public MediaWindow()
        {
            InitializeComponent();
            InitializeMediaPreview();
        }

        private void InitializeMediaPreview()
        {
            // Инициализируем только после полной загрузки окна
            this.Loaded += (s, e) => 
            {
                // Скрываем все превью
                ImagePreview.Visibility = Visibility.Collapsed;
                VideoPreview.Visibility = Visibility.Collapsed;
                AudioPreview.Visibility = Visibility.Collapsed;
                MediaPreviewText.Visibility = Visibility.Visible;
            };
        }

        public void UpdateMediaPreview()
        {
            // Проверяем, что элементы UI инициализированы
            if (MediaPathTextBox == null || MediaPreviewText == null || 
                ImagePreview == null || VideoPreview == null || AudioPreview == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(MediaPathTextBox.Text) || !File.Exists(MediaPathTextBox.Text))
            {
                MediaPreviewText.Text = "Файл не выбран или не существует";
                MediaPreviewText.Visibility = Visibility.Visible;
                ImagePreview.Visibility = Visibility.Collapsed;
                VideoPreview.Visibility = Visibility.Collapsed;
                AudioPreview.Visibility = Visibility.Collapsed;
                return;
            }

            MediaPreviewText.Visibility = Visibility.Collapsed;

            try
            {
                switch (MediaTypeComboBox.SelectedIndex)
                {
                    case 0: // Изображение
                        ImagePreview.Visibility = Visibility.Visible;
                        VideoPreview.Visibility = Visibility.Collapsed;
                        AudioPreview.Visibility = Visibility.Collapsed;
                        
                        var imageUri = new System.Uri(MediaPathTextBox.Text);
                        ImagePreview.Source = new System.Windows.Media.Imaging.BitmapImage(imageUri);
                        break;

                    case 1: // Видео
                        ImagePreview.Visibility = Visibility.Collapsed;
                        VideoPreview.Visibility = Visibility.Visible;
                        AudioPreview.Visibility = Visibility.Collapsed;
                        
                        VideoPreview.Source = new System.Uri(MediaPathTextBox.Text);
                        VideoPreview.LoadedBehavior = System.Windows.Controls.MediaState.Manual;
                        break;

                    case 2: // Аудио
                        ImagePreview.Visibility = Visibility.Collapsed;
                        VideoPreview.Visibility = Visibility.Collapsed;
                        AudioPreview.Visibility = Visibility.Visible;
                        
                        AudioPreview.Source = new System.Uri(MediaPathTextBox.Text);
                        AudioPreview.LoadedBehavior = System.Windows.Controls.MediaState.Manual;
                        break;
                }
            }
            catch (Exception ex)
            {
                MediaPreviewText.Text = $"Ошибка загрузки: {ex.Message}";
                MediaPreviewText.Visibility = Visibility.Visible;
                ImagePreview.Visibility = Visibility.Collapsed;
                VideoPreview.Visibility = Visibility.Collapsed;
                AudioPreview.Visibility = Visibility.Collapsed;
            }
        }

        private void MediaTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Проверяем, что окно загружено
            if (this.IsLoaded)
            {
                UpdateMediaPreview();
            }
        }

        private void BrowseMediaButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            
            switch (MediaTypeComboBox.SelectedIndex)
            {
                case 0: // Изображение
                    openFileDialog.Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Все файлы (*.*)|*.*";
                    openFileDialog.Title = "Выберите изображение";
                    break;
                case 1: // Видео
                    openFileDialog.Filter = "Видео файлы (*.mp4;*.avi;*.wmv;*.mov)|*.mp4;*.avi;*.wmv;*.mov|Все файлы (*.*)|*.*";
                    openFileDialog.Title = "Выберите видео";
                    break;
                case 2: // Аудио
                    openFileDialog.Filter = "Аудио файлы (*.mp3;*.wav;*.wma)|*.mp3;*.wav;*.wma|Все файлы (*.*)|*.*";
                    openFileDialog.Title = "Выберите аудио";
                    break;
            }

            if (openFileDialog.ShowDialog() == true)
            {
                MediaPathTextBox.Text = openFileDialog.FileName;
                UpdateMediaPreview();
            }
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(MediaPathTextBox.Text) || !File.Exists(MediaPathTextBox.Text))
            {
                MessageBox.Show("Сначала выберите медиафайл", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            switch (MediaTypeComboBox.SelectedIndex)
            {
                case 1: // Видео
                    VideoPreview.Play();
                    break;
                case 2: // Аудио
                    AudioPreview.Play();
                    break;
            }
        }

        private void StopPreviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (VideoPreview != null)
                VideoPreview.Stop();
            if (AudioPreview != null)
                AudioPreview.Stop();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(QuestionTextTextBox.Text))
            {
                MessageBox.Show("Введите описание медиаконтента", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!string.IsNullOrEmpty(MediaPathTextBox.Text) && !File.Exists(MediaPathTextBox.Text))
            {
                MessageBox.Show("Выбранный медиафайл не существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ResultQuestion = new QuestionMedia
            {
                QuestionText = QuestionTextTextBox.Text.Trim(),
                Points = int.TryParse(PointsTextBox.Text, out int points) ? points : 0,
                MediaType = (MediaType)MediaTypeComboBox.SelectedIndex,
                MediaPath = MediaPathTextBox.Text,
                Caption = CaptionTextBox.Text.Trim(),
                AutoPlay = AutoPlayCheckBox.IsChecked ?? false,
                Loop = LoopCheckBox.IsChecked ?? false
            };

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (VideoPreview != null)
                VideoPreview.Stop();
            if (AudioPreview != null)
                AudioPreview.Stop();
            this.DialogResult = false;
            this.Close();
        }
    }
}