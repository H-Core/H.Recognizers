using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using H.Core;
using H.Core.Recognizers;
using Yandex.Cloud.Ai.Stt.V2;

namespace H.Recognizers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class YandexStreamingRecognition : StreamingRecognition
    {
        #region Properties

        private AsyncDuplexStreamingCall<StreamingRecognitionRequest, StreamingRecognitionResponse> Call { get; }

        private ConcurrentQueue<byte[]> WriteQueue { get; } = new ();
        private Task<string> ReceiveTask { get; }
        private Task WriteTask { get; }
        private bool IsFinished { get; set; }

        #endregion

        #region Constructors

        internal YandexStreamingRecognition(
            AudioSettings settings, 
            AsyncDuplexStreamingCall<StreamingRecognitionRequest, StreamingRecognitionResponse> call) :
            base(settings)
        {
            Call = call ?? throw new ArgumentNullException(nameof(call));

            // TODO: Implement exception return
            ReceiveTask = Task.Run(async () =>
            {
                while (!IsFinished && await Call.ResponseStream.MoveNext().ConfigureAwait(false))
                {
                    var response = Call.ResponseStream.Current;
                    var chunk = response.Chunks
                        .LastOrDefault();
                    var text = chunk?
                        .Alternatives
                        .OrderBy(i => i.Confidence)
                        .FirstOrDefault()?
                        .Text;

                    if (chunk != null && text != null && !string.IsNullOrWhiteSpace(text))
                    {
                        if (chunk.Final)
                        {
                            IsFinished = true;
                            
                            return text;
                        }

                        OnPreviewReceived(text);
                    }

                    Trace.WriteLine($"{DateTime.Now:h:mm:ss.fff} YandexStreamingRecognition: {response}");
                }

                return string.Empty;
            });
            WriteTask = Task.Run(async () =>
            {
                while (!IsFinished)
                {
                    // TODO: Combine all accumulated data in the queue into one message
                    if (!WriteQueue.TryDequeue(out var bytes))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1)).ConfigureAwait(false);
                        continue;
                    }

                    await Call.RequestStream.WriteAsync(new StreamingRecognitionRequest
                    {
                        AudioContent = ByteString.CopyFrom(bytes, 0, bytes.Length),
                    }).ConfigureAwait(false);
                }
            });
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
            WriteQueue.Enqueue(bytes);

            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<string> StopAsync(CancellationToken cancellationToken = default)
        {
            OnStopping();

            await WriteTask.ConfigureAwait(false);

            await Call.RequestStream.CompleteAsync().ConfigureAwait(false);

            Result = await ReceiveTask.ConfigureAwait(false);
            
            OnStopped(Result);

            return Result;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            Call.Dispose();

            base.Dispose();
        }

        #endregion
    }
}
