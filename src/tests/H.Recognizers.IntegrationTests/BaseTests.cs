using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core.Recognizers;
using H.Core.Recorders;
using H.Core.Utilities;
using H.IO.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Recognizers.IntegrationTests
{
    public static class BaseTests
    {
        public static async Task StartStreamingRecognitionTest(IRecognizer recognizer, string name, string expected, int bytesPerWrite = 8000)
        {
            using var recognition = await recognizer.StartStreamingRecognitionAsync();
            recognition.PartialResultsReceived += (_, value) => Console.WriteLine($"{DateTime.Now:h:mm:ss.fff} AfterPartialResults: {value}");
            recognition.FinalResultsReceived += (_, value) =>
            {
                Console.WriteLine($"{DateTime.Now:h:mm:ss.fff} AfterFinalResults: {value}");

                Assert.AreEqual(expected, value);
            };

            var bytes = ResourcesUtilities.ReadFileAsBytes(name);
            // 44 - is default wav header length
            for (var i = 44; i < bytes.Length; i += bytesPerWrite)
            {
                var chunk = new ArraySegment<byte>(bytes, i, i < bytes.Length - bytesPerWrite ? bytesPerWrite : bytes.Length - i).ToArray();
                await recognition.WriteAsync(chunk);

                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }

            await recognition.StopAsync();
        }

        public static async Task<ExceptionsBag> StartStreamingRecognitionTest_RealTimeAsync(
            IRecorder recorder,
            IRecognizer recognizer, 
            bool writeWavHeader = false,
            CancellationToken cancellationToken = default)
        {
            var exceptions = new ExceptionsBag();
            using var recognition = await recognizer.StartStreamingRecognitionAsync(recorder, writeWavHeader, exceptions, cancellationToken);
            recognition.PartialResultsReceived += (_, value) =>
            {
                Console.WriteLine($"{DateTime.Now:h:mm:ss.fff} {nameof(recognition.PartialResultsReceived)}: {value}");
            };
            recognition.FinalResultsReceived += (_, value) =>
            {
                Console.WriteLine($"{DateTime.Now:h:mm:ss.fff} {nameof(recognition.FinalResultsReceived)}: {value}");
            };

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            await recognition.StopAsync(cancellationToken);

            return exceptions;
        }

        public static async Task ConvertTest(IRecognizer recognizer, string name, string expected)
        {
            var bytes = ResourcesUtilities.ReadFileAsBytes(name);
            var actual = await recognizer.ConvertAsync(bytes);

            Assert.AreEqual(expected, actual);
        }

        public static async Task ConvertTest_RealTime(IRecorder recorder, IRecognizer recognizer)
        {
            using var recognition = await recorder.StartAsync();

            await Task.Delay(TimeSpan.FromSeconds(5));

            await recognition.StopAsync();

            var bytes = recognition.WavData;
            Assert.IsNotNull(bytes, $"{nameof(bytes)} == null");

            var result = await recognizer.ConvertAsync(bytes);

            Console.WriteLine(result);
        }
    }
}
