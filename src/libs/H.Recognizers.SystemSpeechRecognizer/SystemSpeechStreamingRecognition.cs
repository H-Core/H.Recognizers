using System;
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
    public sealed class SystemSpeechStreamingRecognition : StreamingRecognition
    {
        #region Properties

        private SpeechRecognitionEngine SpeechRecognitionEngine { get; }

        #endregion

        #region Constructors

        internal SystemSpeechStreamingRecognition(AudioSettings settings, SpeechRecognitionEngine speechRecognitionEngine) : base(settings)
        {
            SpeechRecognitionEngine = speechRecognitionEngine ?? throw new ArgumentNullException(nameof(speechRecognitionEngine));

            SpeechRecognitionEngine.SpeechHypothesized += (_, args) => OnPreviewReceived(args.Result.Text);
            SpeechRecognitionEngine.SpeechRecognized += (_, args) =>
            {
                Result = args.Result.Text;
                
                OnStopped(Result);
            };

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
        public override Task<string> StopAsync(CancellationToken cancellationToken = default)
        {
            OnStopping();
            
            SpeechRecognitionEngine.RecognizeAsyncStop();
            
            return Task.FromResult(Result);
        }

        #endregion
    }
}
