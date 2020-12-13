using System.Threading.Tasks;
using H.Core.Recognizers;
using H.Core.Recorders;
using H.Recorders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Converters.IntegrationTests
{
    [TestClass]
    [Ignore]
    public class YandexConverterTests
    {
        public const string FolderId = "$FolderId$";
        public const string OAuthToken = "$OAuthToken$";

        public static IRecorder CreateRecorder() => new NAudioRecorder();

        public static IRecognizer CreateRecognizer() => new YandexRecognizer
        {
            OAuthToken = OAuthToken,
            FolderId = FolderId,
            Lang = "ru-RU",
            SampleRateHertz = 8000,
        };

        [TestMethod]
        public async Task StartStreamingRecognitionTest()
        {
            using var recognizer = CreateRecognizer();

            await BaseConvertersTests.StartStreamingRecognitionTest(recognizer, "test_test_rus_8000.wav", "проверка");
        }

        [TestMethod]
        public async Task StartStreamingRecognitionTest_RealTime()
        {
            using var recorder = CreateRecorder();
            using var recognizer = CreateRecognizer();

            var exceptions = await BaseConvertersTests.StartStreamingRecognitionTest_RealTimeAsync(recorder, recognizer);
            exceptions.EnsureNoExceptions();
        }

        [TestMethod]
        public async Task ConvertTest()
        {
            using var recognizer = CreateRecognizer();

            await BaseConvertersTests.ConvertTest(recognizer, "test_test_rus_8000.wav", "проверка проверка");
        }

        [TestMethod]
        public async Task ConvertTest_RealTime()
        {
            using var recorder = CreateRecorder();
            using var recognizer = CreateRecognizer();

            await BaseConvertersTests.ConvertTest_RealTime(recorder, recognizer);
        }
    }
}
