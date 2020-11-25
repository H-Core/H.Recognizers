﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using H.Core;
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
                using var recorder = ConverterComboBox.Text switch
                {
                    nameof(WitAiConverter) => new NAudioRecorder(),
                    nameof(YandexConverter) => new NAudioRecorder(),
                    _ => throw new NotImplementedException()
                };
                await recorder.InitializeAsync();
                await recorder.StartAsync();

                using var converter = ConverterComboBox.Text switch
                {
                    nameof(WitAiConverter) => (IConverter)new WitAiConverter
                    {
                        Token = !string.IsNullOrWhiteSpace(OAuthTokenTextBox.Text) ? OAuthTokenTextBox.Text : "KATWBG4RQCFNBLQTY6QQUKB2SH6EIELG",
                    },
                    nameof(YandexConverter) => new YandexConverter
                    {
                        OAuthToken = OAuthTokenTextBox.Text,
                        FolderId = FolderIdTextBox.Text,
                        Lang = "ru-RU",
                        SampleRateHertz = 8000,
                    },
                    _ => throw new NotImplementedException(),
                };

                using var recognition = await converter.StartStreamingRecognitionAsync().ConfigureAwait(false);
                recognition.AfterPartialResults += (_, args) => Dispatcher?.Invoke(() =>
                {
                    OutputTextBox.Text += $"{DateTime.Now:h:mm:ss.fff} {args.Text}{Environment.NewLine}";
                    OutputTextBlock.Text = $"{DateTime.Now:h:mm:ss.fff} {args.Text}";
                });
                recognition.AfterFinalResults += (_, args) => Dispatcher?.Invoke(() =>
                {
                    OutputTextBox.Text += $"{DateTime.Now:h:mm:ss.fff} {args.Text}{Environment.NewLine}";
                    OutputTextBlock.Text = $"{DateTime.Now:h:mm:ss.fff} {args.Text}";
                });

                if (recorder.RawData != null)
                {
                    await recognition.WriteAsync(recorder.RawData.ToArray()).ConfigureAwait(false);
                }

                // ReSharper disable once AccessToDisposedClosure
                recorder.RawDataReceived += async (_, args) =>
                {
                    if (args.RawData == null)
                    {
                        return;
                    }

                    await recognition.WriteAsync(args.RawData.ToArray()).ConfigureAwait(false);
                };

                await Task.Delay(TimeSpan.FromMilliseconds(5000)).ConfigureAwait(false);

                await recorder.StopAsync();
                await recognition.StopAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }

            Dispatcher?.Invoke(() =>
            {
                ProcessButton.IsEnabled = true;
                OutputTextBox.Text += $"{DateTime.Now:h:mm:ss.fff} Ended {Environment.NewLine}";
            });
        }
    }
}
