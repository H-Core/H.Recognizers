using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core.Recognizers;
using Microsoft.Speech.Recognition;

namespace H.Recognizers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SystemSpeechRecognizer : Recognizer, IRecognizer
    {
        #region Properties

        bool IRecognizer.IsStreamingRecognitionSupported => true;

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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<IStreamingRecognition> StartStreamingRecognitionAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IStreamingRecognition>(new SystemSpeechStreamingRecognition(SpeechRecognitionEngine));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<string> ConvertAsync(byte[] bytes, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

#pragma warning disable CA1063 // Implement IDisposable Correctly
        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
#pragma warning restore CA1063 // Implement IDisposable Correctly
        {
            SpeechRecognitionEngine.Dispose();

            base.Dispose();
        }

        #endregion
    }
}
