﻿using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Core.Recognizers;
using Microsoft.Speech.Recognition;

namespace H.Recognizers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SystemSpeechRecognizer : Recognizer
    {
        #region Properties

        private SpeechRecognitionEngine SpeechRecognitionEngine { get; }
        
        /// <summary>
        /// 
        /// </summary>
        public string Recognizer { get; set; } = string.Empty;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public SystemSpeechRecognizer()
        {
            AddEnumerableSetting(nameof(Recognizer), o => Recognizer = o, NoEmpty, SpeechRecognitionEngine.InstalledRecognizers().Select(i => i.Name).ToArray());

            SpeechRecognitionEngine = new SpeechRecognitionEngine(new CultureInfo("ru-RU"));
            SpeechRecognitionEngine.SetInputToDefaultAudioDevice();

            var builder = new GrammarBuilder
            {
                Culture = new CultureInfo("ru-RU"),
            };

            builder.Append("Бот");
            builder.Append(new Choices("сделай", "принеси"));
            builder.Append(new Choices("справочник", "отчет"));

            SpeechRecognitionEngine.LoadGrammar(new Grammar(builder));
        }

        #endregion

        #region Public methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<IStreamingRecognition> StartStreamingRecognitionAsync(
            AudioSettings? settings = null, 
            CancellationToken cancellationToken = default)
        {
            settings ??= new AudioSettings();

            return Task.FromResult<IStreamingRecognition>(
                new SystemSpeechStreamingRecognition(settings, SpeechRecognitionEngine));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="settings"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<string> ConvertAsync(
            byte[] bytes,
            AudioSettings? settings = null, 
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            SpeechRecognitionEngine.Dispose();

            base.Dispose();
        }

        #endregion
    }
}
