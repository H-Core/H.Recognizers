using System.Threading.Tasks;
using H.Core.Converters;
using H.Core.Recorders;
using H.Recorders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Converters.IntegrationTests
{
    [TestClass]
    public class WitAiConverterTests
    {
        public static IRecorder CreateRecorder() => new NAudioRecorder();
        public static IConverter CreateConverter() => new WitAiConverter
        {
            Token = "XZS4M3BUYV5LBMEWJKAGJ6HCPWZ5IDGY"
        };

        [TestMethod]
        public async Task StartStreamingRecognitionTest()
        {
            using var converter = CreateConverter();

            await BaseConvertersTests.StartStreamingRecognitionTest(converter, "test_test_rus_8000.wav", "проверка");
        }

        [TestMethod]
        [Ignore]
        public async Task StartStreamingRecognitionTest_RealTime()
        {
            using var recorder = CreateRecorder();
            using var converter = CreateConverter();

            var exceptions = await BaseConvertersTests.StartStreamingRecognitionTest_RealTimeAsync(recorder, converter, true);
            exceptions.EnsureNoExceptions();
        }

        [TestMethod]
        public async Task ConvertTest()
        {
            using var converter = CreateConverter();

            await BaseConvertersTests.ConvertTest(converter, "test_test_rus_8000.wav", "проверка");
        }

        [TestMethod]
        [Ignore]
        public async Task ConvertTest_RealTime()
        {
            using var recorder = CreateRecorder();
            using var converter = CreateConverter();

            await BaseConvertersTests.ConvertTest_RealTime(recorder, converter);
        }
    }
}
