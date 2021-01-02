using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using H.Core;
using H.Core.Recognizers;
using H.Recognizers.Utilities;
using Newtonsoft.Json;
using Yandex.Cloud.Ai.Stt.V2;

namespace H.Recognizers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class YandexRecognizer : Recognizer
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public string Lang { get; set; } = string.Empty;
        
        /// <summary>
        /// 
        /// </summary>
        public string Topic { get; set; } = string.Empty;
        
        /// <summary>
        /// 
        /// </summary>
        public bool ProfanityFilter { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string FolderId { get; set; } = string.Empty;
        
        /// <summary>
        /// 
        /// </summary>
        public string OAuthToken { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public string? IamToken { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public YandexRecognizer()
        {
            AddSetting(nameof(FolderId), o => FolderId = o, Any, string.Empty);
            AddSetting(nameof(OAuthToken), o => OAuthToken = o, NoEmpty, string.Empty);

            AddEnumerableSetting(nameof(Lang), o => Lang = o, NoEmpty, new[] { "ru-RU", "en-US", "uk-UK", "tr-TR" });
            AddEnumerableSetting(nameof(Topic), o => Topic = o, NoEmpty, new[] { "general", "maps", "dates", "names", "numbers" });
            AddEnumerableSetting(nameof(ProfanityFilter), o => ProfanityFilter = o == "true", NoEmpty, new[] { "false", "true" });

            SupportedSettings.Add(new AudioSettings());
            SupportedSettings.Add(new AudioSettings(rate: 48000));
            SupportedSettings.Add(new AudioSettings(rate: 16000));
            SupportedSettings.Add(new AudioSettings(AudioFormat.Ogg));

            SupportedStreamingSettings.Add(new AudioSettings());
            SupportedStreamingSettings.Add(new AudioSettings(rate: 48000));
            SupportedStreamingSettings.Add(new AudioSettings(rate: 16000));
            SupportedStreamingSettings.Add(new AudioSettings(AudioFormat.Ogg));
        }

        #endregion

        #region Public methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<IStreamingRecognition> StartStreamingRecognitionAsync(
            AudioSettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            settings ??= SupportedStreamingSettings.First();

            IamToken ??= await RequestIamTokenByOAuthTokenAsync(OAuthToken, cancellationToken).ConfigureAwait(false);

            var channel = new Channel("stt.api.cloud.yandex.net", 443, new SslCredentials());
            var client = new SttService.SttServiceClient(channel);
            var call = client.StreamingRecognize(new Metadata
            {
                {"authorization", $"Bearer {IamToken}"}
            }, cancellationToken: cancellationToken);

            await call.RequestStream.WriteAsync(new StreamingRecognitionRequest
            {
                Config = new RecognitionConfig
                {
                    Specification = new RecognitionSpec
                    {
                        LanguageCode = Lang,
                        ProfanityFilter = ProfanityFilter,
                        Model = Topic,
                        AudioEncoding = settings.Format switch
                        {
                            AudioFormat.Ogg => RecognitionSpec.Types.AudioEncoding.OggOpus,
                            AudioFormat.Raw => RecognitionSpec.Types.AudioEncoding.Linear16Pcm,
                            _ => RecognitionSpec.Types.AudioEncoding.Unspecified,
                        },
                        SampleRateHertz = settings.Rate,
                        PartialResults = true,
                    },
                    FolderId = FolderId,
                }
            }).ConfigureAwait(false);

            return new YandexStreamingRecognition(settings, call);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="settings"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<string> ConvertAsync(
            byte[] bytes, 
            AudioSettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            settings ??= SupportedSettings.First();

            IamToken ??= await RequestIamTokenByOAuthTokenAsync(OAuthToken, cancellationToken).ConfigureAwait(false);

            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://stt.api.cloud.yandex.net/speech/v1/stt:recognize").WithQuery(new Dictionary<string, string?>
            {
                { "lang", Lang },
                { "topic", Topic },
                { "profanityFilter", ProfanityFilter ? "true" :  "false" },
                { "format", settings.Format switch
                    {
                        AudioFormat.Ogg => "oggopus",
                        _ or AudioFormat.Raw => "lpcm",
                    }
                },
                { "sampleRateHertz", $"{settings.Rate}" },
                { "folderId", FolderId },
            }))
            {
                Headers =
                {
                    { "Authorization", $"Bearer {IamToken}" },
                    //{ "Transfer-Encoding", "chunked" },
                },
                Content = new ByteArrayContent(bytes)
            };
            using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
           
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            return dictionary.TryGetValue("result", out var value)
                ? value
                : throw new InvalidOperationException($"Result is not found: {json}");
        }

        #endregion

        #region Private methods

        private static async Task<string> RequestIamTokenByOAuthTokenAsync(string oAuthToken, CancellationToken cancellationToken = default)
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://iam.api.cloud.yandex.net/iam/v1/tokens")
            {
                Content = new StringContent(JsonConvert.SerializeObject(new 
                {
                    yandexPassportOauthToken = oAuthToken,
                }), Encoding.UTF8, "application/json"),
            };
            using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            return dictionary.TryGetValue("iamToken", out var value)
                ? value
                : throw new InvalidOperationException($"Token is not found: {json}");
        }

        #endregion
    }
}
