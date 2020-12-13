using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using H.Core.Converters;
using H.Core.Utilities;
using H.Recorders;

namespace H.Converters.App.WPF
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            ConverterComboBox.ItemsSource = new List<string>
            {
                nameof(YandexConverter), 
                nameof(WitAiConverter)
            };
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ProcessButton.IsEnabled = false;
            OutputTextBox.Text += $"{DateTime.Now:h:mm:ss.fff} Started {Environment.NewLine}";

            try
            {
                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var cancellationToken = cancellationTokenSource.Token;
                
                using var recorder = new NAudioRecorder();
                using var converter = ConverterComboBox.Text switch
                {
                    nameof(YandexConverter) => new YandexConverter
                    {
                        OAuthToken = OAuthTokenTextBox.Text,
                        FolderId = FolderIdTextBox.Text,
                        Lang = "ru-RU",
                        SampleRateHertz = 8000,
                    },
                    nameof(WitAiConverter) or _ => (IConverter)new WitAiConverter
                    {
                        Token = !string.IsNullOrWhiteSpace(OAuthTokenTextBox.Text)
                            ? OAuthTokenTextBox.Text
                            : "KATWBG4RQCFNBLQTY6QQUKB2SH6EIELG",
                    },
                };
                var exceptions = new ExceptionsBag();
                exceptions.ExceptionOccurred += (_, exception) => OnException(exception);

                using var recognition = await converter.StartStreamingRecognitionAsync(recorder, false, exceptions, cancellationToken).ConfigureAwait(false);
                recognition.PartialResultsReceived += (_, value) => Dispatcher?.Invoke(() =>
                {
                    OutputTextBox.Text += $"{DateTime.Now:h:mm:ss.fff} Partial: {value}{Environment.NewLine}";
                });
                recognition.FinalResultsReceived += (_, value) => Dispatcher?.Invoke(() =>
                {
                    OutputTextBox.Text += $"{DateTime.Now:h:mm:ss.fff} Final: {value}{Environment.NewLine}";
                });

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken).ConfigureAwait(false);

                await recognition.StopAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                OnException(exception);
            }

            Dispatcher?.Invoke(() =>
            {
                ProcessButton.IsEnabled = true;
                OutputTextBox.Text += $"{DateTime.Now:h:mm:ss.fff} Ended {Environment.NewLine}";
            });
        }

        private static void OnException(Exception exception)
        {
            MessageBox.Show(exception.ToString());
        }
    }
}
