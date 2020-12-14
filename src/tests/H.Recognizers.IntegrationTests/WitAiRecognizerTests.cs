using System.Linq;
using System.Threading.Tasks;
using H.Core.Recognizers;
using H.Core.Recorders;
using H.Recorders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Recognizers.IntegrationTests
{
    [TestClass]
    public class WitAiRecognizerTests
    {
        public static IRecorder CreateRecorder()
        {
            if (!NAudioRecorder.GetAvailableDevices().Any())
            {
                Assert.Inconclusive("No available devices for NAudioRecorder.");
            }
            
            return new NAudioRecorder();
        }

        public static IRecognizer CreateRecognizer() => new WitAiRecognizer
        {
            Token = "XZS4M3BUYV5LBMEWJKAGJ6HCPWZ5IDGY"
        };

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

            var exceptions = await BaseTests.StartStreamingRecognitionTest_RealTimeAsync(recorder, recognizer, true);
            exceptions.EnsureNoExceptions();
        }

        [TestMethod]
        public async Task ConvertTest()
        {
            using var recognizer = CreateRecognizer();

            await BaseTests.ConvertTest(recognizer, "test_test_rus_8000.wav", "проверка");
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
