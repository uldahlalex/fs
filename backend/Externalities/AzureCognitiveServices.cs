using System.Net.Http.Json;
using Externalities._3rdPartyTransferModels;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace Externalities;

public class AzureCognitiveServices
{
    public async Task<string> GetToxicityAnalysis(string? message)
    {
        const string URL = "https://toxicityfilter.cognitiveservices.azure.com/contentsafety/text:analyze?api-version=2023-10-01";
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key",
            Environment.GetEnvironmentVariable("FULLSTACK_AZURE_COGNITIVE_SERVICES"));
        var request = new ToxicityRequest
        {
            categories = new List<string> { "Hate", "SelfHarm", "Sexual", "Violence" },
            outputType = "FourSeverityLevels",
            text = message!
        };
        return await (await httpClient.PostAsJsonAsync(URL, request)).Content.ReadAsStringAsync();
    }

    public async Task<string> Az()
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key",
            Environment.GetEnvironmentVariable("FULLSTACK_AZURE_COGNITIVE_SERVICES_SPEECH"));
        return "";
    }
    
    public async Task Realtime()
    {
        var speechConfig = SpeechConfig.FromSubscription(Environment.GetEnvironmentVariable("FULLSTACK_AZURE_COGNITIVE_SERVICES_SPEECH"), "northeurope");
        using var audioConfig = AudioConfig.FromWavFileInput("/home/alex/source/fs/backend/api/speech.wav");
        using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
        var stopRecognition = new TaskCompletionSource<int>();
        speechRecognizer.Recognizing += (s, e) =>
        {
            Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
        };

        speechRecognizer.Recognized += (s, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
            }
            else if (e.Result.Reason == ResultReason.NoMatch)
            {
                Console.WriteLine($"NOMATCH: Speech could not be recognized.");
            }
        };

        speechRecognizer.Canceled += (s, e) =>
        {
            Console.WriteLine($"CANCELED: Reason={e.Reason}");

            if (e.Reason == CancellationReason.Error)
            {
                Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
            }

            stopRecognition.TrySetResult(0);
        };

        speechRecognizer.SessionStopped += (s, e) =>
        {
            Console.WriteLine("\n    Session stopped event.");
            stopRecognition.TrySetResult(0);
        };
        Task.WaitAny(new[] { stopRecognition.Task });
    }

    public async Task File()
    {
        


        static void OutputSpeechRecognitionResult(SpeechRecognitionResult speechRecognitionResult)
        {
            switch (speechRecognitionResult.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    Console.WriteLine($"RECOGNIZED: Text={speechRecognitionResult.Text}");
                    break;
                case ResultReason.NoMatch:
                    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                    break;
                case ResultReason.Canceled:
                    var cancellation = CancellationDetails.FromResult(speechRecognitionResult);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                    }

                    break;
            }
        }
        
        var speechConfig = SpeechConfig.FromSubscription(Environment.GetEnvironmentVariable("FULLSTACK_AZURE_COGNITIVE_SERVICES_SPEECH"), "northeurope");
        Console.WriteLine(speechConfig.SubscriptionKey);
        Console.WriteLine(speechConfig.Region);
            speechConfig.SetProperty("CONFIG_MAX_CRL_SIZE_KB", "15000");
            speechConfig.SetProperty("OPENSSL_CONTINUE_ON_CRL_DOWNLOAD_FAILURE", "true");
            speechConfig.SetProperty("OPENSSL_DISABLE_CRL_CHECK", "true");

        using var audioConfig = AudioConfig.FromWavFileInput("speech.wav");
        audioConfig.SetProperty("CONFIG_MAX_CRL_SIZE_KB", "15000");
        audioConfig.SetProperty("OPENSSL_CONTINUE_ON_CRL_DOWNLOAD_FAILURE", "true");

        speechConfig.SpeechRecognitionLanguage = "en-US";

     
        using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

    
        var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
        OutputSpeechRecognitionResult(speechRecognitionResult);

    }
}