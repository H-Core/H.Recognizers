using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Core.Recognizers;
using Newtonsoft.Json;

namespace H.Recognizers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class WitAiRecognizer : Recognizer
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public string Token { get; set; } = string.Empty;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public WitAiRecognizer() : base(AudioFormat.Wav, AudioFormat.Wav)
        {
            AddSetting(nameof(Token), o => Token = o, NoEmpty, string.Empty);
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
            return Task.FromResult<IStreamingRecognition>(new WitAiStreamingRecognition(Token));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<string> ConvertAsync(byte[] bytes, CancellationToken cancellationToken = default)
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.wit.ai/speech")
            {
                Headers =
                {
                    Authorization = AuthenticationHeaderValue.Parse($"Bearer {Token}"),
                    TransferEncodingChunked = true,
                },
                Content = new ByteArrayContent(bytes)
                {
                    Headers =
                    {
                        ContentType = MediaTypeHeaderValue.Parse("audio/wav"),
                    },
                },
            };
            using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Invalid response: {json}");
            }

            var obj = JsonConvert.DeserializeObject<WitAiResponse>(json);

            return obj.Text ?? string.Empty;
            //return await ConvertOverStreamingRecognition(bytes, cancellationToken).ConfigureAwait(false);
        }

        #endregion
    }
}
