#if NETFRAMEWORK
using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Recognizers.IntegrationTests
{
    [TestClass]
    [Ignore]
    public class SystemSpeechRecognizerTests
    {
        [TestMethod]
        public async Task StartStreamingRecognitionTest_RealTime()
        {
            using var recognizer = new SystemSpeechRecognizer();

            using var recognition = await recognizer.StartStreamingRecognitionAsync();
            recognition.PartialResultsReceived += (_, value) => Console.WriteLine($"{DateTime.Now:h:mm:ss.fff} PartialResultsReceived: {value}");
            recognition.FinalResultsReceived += (_, value) => Console.WriteLine($"{DateTime.Now:h:mm:ss.fff} FinalResultsReceived: {value}");

            await Task.Delay(TimeSpan.FromSeconds(5));

            await recognition.StopAsync();
        }
    }
}
#endif
