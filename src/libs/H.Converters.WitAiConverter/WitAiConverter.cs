﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Core.Converters;
using Newtonsoft.Json;

namespace H.Converters
{
    public sealed class WitAiConverter : Converter, IConverter
    {
        #region Properties

        bool IConverter.IsStreamingRecognitionSupported => true;

        public string Token { get; set; } = string.Empty;

        #endregion

        #region Constructors

        public WitAiConverter()
        {
            AddSetting(nameof(Token), o => Token = o, NoEmpty, string.Empty);
        }

        #endregion

        #region Public methods

        public override Task<IStreamingRecognition> StartStreamingRecognitionAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IStreamingRecognition>(new WitAiStreamingRecognition(Token));
        }

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
