using System;
using System.Collections.Generic;
using System.Windows;
using H.Core.Recognizers;
using H.Core.Utilities;
using H.Recorders;

namespace H.Recognizers.App.WPF
{
    public partial class MainWindow
    {
        #region Properties

        protected IStreamingRecognition? Recognition { get; set; }

        #endregion

        #region MyRegion

        public MainWindow()
        {
            InitializeComponent();

            RecognizerComboBox.ItemsSource = new List<string>
            {
                nameof(YandexRecognizer), 
                nameof(WitAiRecognizer)
            };
        }

        #endregion

        #region Event Handlers

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                StartButton.IsEnabled = false;
                StopButton.IsEnabled = true;
                OutputTextBox.Text += $"{DateTime.Now:h:mm:ss.fff} Started {Environment.NewLine}";
            });

            try
            {
                using var recorder = new NAudioRecorder();
                using var recognizer = RecognizerComboBox.Text switch
                {
                    nameof(YandexRecognizer) => new YandexRecognizer
                    {
                        OAuthToken = OAuthTokenTextBox.Text,
                        FolderId = FolderIdTextBox.Text,
                        Lang = "ru-RU",
                    },
                    nameof(WitAiRecognizer) or _ => (IRecognizer)new WitAiRecognizer
                    {
                        Token = !string.IsNullOrWhiteSpace(OAuthTokenTextBox.Text)
                            ? OAuthTokenTextBox.Text
                            : "KATWBG4RQCFNBLQTY6QQUKB2SH6EIELG",
                    },
                };
                var exceptions = new ExceptionsBag();
                exceptions.ExceptionOccurred += (_, exception) => OnException(exception);

                Recognition = await recognizer.StartStreamingRecognitionAsync(recorder, exceptions).ConfigureAwait(false);
                Recognition.PreviewReceived += (_, value) => Dispatcher?.Invoke(() =>
                {
                    OutputTextBox.Text += $"{DateTime.Now:h:mm:ss.fff} Preview: {value}{Environment.NewLine}";
                });
                Recognition.Stopped += (_, value) => Dispatcher?.Invoke(() =>
                {
                    OutputTextBox.Text += $"{DateTime.Now:h:mm:ss.fff} Final: {value}{Environment.NewLine}";
                });
            }
            catch (Exception exception)
            {
                OnException(exception);
            }
        }
        
        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                StartButton.IsEnabled = true;
                StopButton.IsEnabled = false;
                OutputTextBox.Text += $"{DateTime.Now:h:mm:ss.fff} Ended {Environment.NewLine}";
            });
            
            try
            {
                if (Recognition != null)
                {
                    await Recognition.StopAsync().ConfigureAwait(false);
                    
                    Recognition.Dispose();
                    Recognition = null;
                }
            }
            catch (Exception exception)
            {
                OnException(exception);
            }
        }

        private static void OnException(Exception exception)
        {
            MessageBox.Show(exception.ToString());
        }

        #endregion
    }
}
