using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core.Converters;
using Microsoft.Speech.Recognition;

namespace H.Converters
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SystemSpeechStreamingRecognition : StreamingRecognition
    {
        #region Properties

        private SpeechRecognitionEngine SpeechRecognitionEngine { get; }

        #endregion

        #region Constructors

        internal SystemSpeechStreamingRecognition(SpeechRecognitionEngine speechRecognitionEngine)
        {
            SpeechRecognitionEngine = speechRecognitionEngine ?? throw new ArgumentNullException(nameof(speechRecognitionEngine));

            SpeechRecognitionEngine.SpeechHypothesized += (_, args) => OnPartialResultsReceived(args.Result.Text);
            SpeechRecognitionEngine.SpeechRecognized += (_, args) => OnFinalResultsReceived(args.Result.Text);

            SpeechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task WriteAsync(byte[] bytes, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task StopAsync(CancellationToken cancellationToken = default)
        {
            OnStopping();
            
            SpeechRecognitionEngine.RecognizeAsyncStop();

            OnStopped();
            
            return Task.CompletedTask;
        }

        #endregion
    }
}
