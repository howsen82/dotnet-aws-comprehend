using Amazon;
using Amazon.Comprehend;
using Amazon.Comprehend.Model;

using System.Text;

var region = RegionEndpoint.USEast1;

// Display title
Console.WriteLine("AWS AI API Sentiment Detector" + Environment.NewLine);

// Ask for phrase
Console.WriteLine("Type in phrase for analysis" + Environment.NewLine);
var phrase = Console.ReadLine();

// Detect Sentiment
// Detect Sentiment
AmazonComprehendClient comprehendClient = new AmazonComprehendClient();
Console.WriteLine("Calling DetectSentiment");
DetectSentimentRequest detectSentimentRequest = new DetectSentimentRequest()
{
    Text = phrase,
    LanguageCode = LanguageCode.En
};

DetectSentimentResponse detectSentimentResponse = await comprehendClient.DetectSentimentAsync(detectSentimentRequest);
Console.WriteLine(detectSentimentResponse.Sentiment);
Console.WriteLine("Done");

if (args.Length == 1)
{
    var filename = args[0];
    var analysisType = (args.Length > 1) ? args[1] : "text";

    Console.WriteLine($"Processing file {filename} with Amazon Comprehend");
    
    var text = File.ReadAllText(filename);
    var request = new DetectSentimentRequest
    {
        Text = text,
        LanguageCode = LanguageCode.En
    };

    // Sentiment Analysis
    var response = await comprehendClient.DetectSentimentAsync(request);

    Console.WriteLine($"Sentiment: {response.Sentiment} \n(Positive: {response.SentimentScore.Positive:N}% | Negative: {response.SentimentScore.Negative:N}% | Neutral: {response.SentimentScore.Neutral:N}% | Mixed: {response.SentimentScore.Mixed:N}%)");

    var requestEntities = new DetectEntitiesRequest()
    {
        Text = text,
        LanguageCode = LanguageCode.En
    };

    // Detect entities
    var responseEntities = await comprehendClient.DetectEntitiesAsync(requestEntities);

    foreach(var entity in responseEntities.Entities)
    {
        Console.WriteLine($"entity Type: {entity.Type.Value} | Text: {entity.Text}");
    }

    // Detect personally identifiable information (PII)
    var requestPII = new DetectPiiEntitiesRequest()
    {
        Text = text,
        LanguageCode = LanguageCode.En
    };

    var responsePII = await comprehendClient.DetectPiiEntitiesAsync(requestPII);

    if (responsePII.Entities.Count > 0)
    {
        var redactedText = new StringBuilder(requestPII.Text);

        foreach (var entity in responsePII.Entities)
        {
            Console.WriteLine($"PII entity Type: {entity.Type.Value} | Text: {entity.BeginOffset}-{entity.EndOffset}");
            for (var pos = entity.BeginOffset; pos < entity.EndOffset; pos++)
            {
                redactedText[pos] = 'X';
            }
        }

        Console.WriteLine();
        Console.WriteLine($"--- redacted text:");
        Console.WriteLine(redactedText);
    }

    Environment.Exit(0);
}

//Console.WriteLine("?Invalid parameter - command line format: dotnet run -- <file>");