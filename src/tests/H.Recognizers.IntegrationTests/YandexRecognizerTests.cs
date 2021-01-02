using System;
using System.Linq;
using System.Threading.Tasks;
using H.Core.Recognizers;
using H.Core.Recorders;
using H.Recorders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Recognizers.IntegrationTests
{
    [TestClass]
    public class YandexRecognizerTests
    {
        public static IRecorder CreateRecorder()
        {
            if (!NAudioRecorder.GetAvailableDevices().Any())
            {
                Assert.Inconclusive("No available devices for NAudioRecorder.");
            }

            return new NAudioRecorder();
        }

        public static IRecognizer CreateRecognizer()
        {
            var folderId = Environment.GetEnvironmentVariable("YANDEX_FOLDER_ID");
            var oAuthToken = Environment.GetEnvironmentVariable("YANDEX_OAUTH_TOKEN");
            if (folderId == null ||
                oAuthToken == null ||
                string.IsNullOrWhiteSpace(folderId) ||
                string.IsNullOrWhiteSpace(oAuthToken))
            {
                Assert.Inconclusive("YANDEX_FOLDER_ID or YANDEX_OAUTH_TOKEN environment variables are not found.");
                throw new Exception();
            }
            
            return new YandexRecognizer
            {
                OAuthToken = oAuthToken,
                FolderId = folderId,
                Lang = "ru-RU",
            };
        }

        [TestMethod]
        public async Task StartStreamingRecognitionTest()
        {
            using var recognizer = CreateRecognizer();

            await BaseTests.StartStreamingRecognitionTest(recognizer, "test_test_rus_8000.wav", "проверка");
        }

        [TestMethod]
        public async Task StartStreamingRecognitionTest_RealTime()
        {
            using var recorder = CreateRecorder();
            using var recognizer = CreateRecognizer();

            var exceptions = await BaseTests.StartStreamingRecognitionTest_RealTimeAsync(recorder, recognizer);
            exceptions.EnsureNoExceptions();
        }

        [TestMethod]
        public async Task ConvertTest()
        {
            using var recognizer = CreateRecognizer();

            await BaseTests.ConvertTest(recognizer, "test_test_rus_8000.wav", "проверка проверка");
        }

        [TestMethod]
        public async Task ConvertTest_RealTime()
        {
            using var recorder = CreateRecorder();
            using var recognizer = CreateRecognizer();

            await BaseTests.ConvertTest_RealTime(recorder, recognizer);
        }
    }
}
