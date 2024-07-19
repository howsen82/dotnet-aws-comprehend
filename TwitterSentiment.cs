using Amazon;
using Amazon.Comprehend;
using Amazon.Comprehend.Model;
using Tweetinvi;
using Tweetinvi.Models;
using System.Diagnostics;

public class TwitterSentiment
{
    const string API_KEY = "[twitter-api-key]";
    const string API_SECRET = "[twitter-api-secret]";
    const string ACCESS_TOKEN = "[twitter-access-token]";
    const string ACCESS_TOKEN_SECRET = "[twitter-access-token-secret]";

    const int MAX_TWEETS_TO_DISPLAY = 15;

    static RegionEndpoint region = RegionEndpoint.USEast1;

    public static async Task Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: dotnet run -- <topic>");
            Environment.Exit(1);
        }

        // Retrieve tweets
        var topic = args[0];

        Console.WriteLine($"Searching Twitter for tweets on {topic}");

        var twitterClient = new TwitterClient(API_KEY, API_SECRET, ACCESS_TOKEN, ACCESS_TOKEN_SECRET);
        ITweet[] tweets = await twitterClient.Search.SearchTweetsAsync(topic);

        // Analyze tweet sentiment
        Console.WriteLine($"Analzying sentiment with Amazon Comprehend");

        var client = new AmazonComprehendClient(region);
        var sentiment = new Dictionary<long, string>();
        int positive = 0, negative = 0, mixed = 0, neutral = 0;

        foreach (var tweet in tweets)
        {
            if (tweet.RetweetedTweet == null)
            {
                var request = new DetectSentimentRequest()
                {
                    Text = tweet.FullText,
                    LanguageCode = LanguageCode.En
                };

                var response = await client.DetectSentimentAsync(request);
                sentiment.Add(tweet.Id, response.Sentiment);
                switch(response.Sentiment)
                {
                    case "POSITIVE":
                        positive++;
                        break;
                    case "NEGATIVE":
                        negative++;
                        break;
                    case "MIXED":
                        mixed++;
                        break;
                    case "NEUTRAL":
                        neutral++;
                        break;
                }
            }
        }

        // Generate sentiment report web page
        var filename = "tweets.html";
        Console.WriteLine($"Generating sentiment report {filename}");

        using (TextWriter tw = File.CreateText(filename))
        {
            tw.WriteLine($@"<html>
            <head>
            <title>Twitter Sentiment Analysis: #{topic}</title>
            <style>body {{ font-family: Helvetica, Arial; }}
            td {{ padding: 10px; vertical-align: top; }}
            .POSITIVE {{ background-color: limegreen; color: white; }}
            .NEGATIVE {{ background-color: tomato; color: white; }} 
            .NEUTRAL {{ background-color: gray; color: white; }} 
            .MIXED {{ background-color: tan; color: white; }}
            </style>
            </head>
            
            <body>
            <h1>topic {topic} | positive: {positive} | negative {negative} | mixed {mixed} | neutral {neutral}</h1>
            <p>&nbsp;</p>
            <table>");

            int count = 0;
            int columns = 0;
            
            foreach (var tweet in tweets)
            {
                if (tweet.RetweetedTweet==null && sentiment[tweet.Id] != "NEUTRAL")
                {
                    var embedTweet = await tweet.GenerateOEmbedTweetAsync();
                    
                    if (columns==0)
                    {
                        tw.WriteLine("      <tr>");
                    }
                    
                    tw.WriteLine($@"        <td class=""{sentiment[tweet.Id]}"">{sentiment[tweet.Id]}<br/>{embedTweet.HTML}</td>");
                    Console.Write(".");
                    
                    columns++;
                    if (columns >=4)
                    {
                        tw.WriteLine("      </tr>");
                        columns = 0;
                    }
                    count++;
                    if (count >= MAX_TWEETS_TO_DISPLAY) break;
                }
            }
            Console.WriteLine();

            if (columns > 0)
            {
                tw.WriteLine("      </tr>");
                columns = 0;
            }

            tw.WriteLine(
                @"    </table>
                </body>
                </html>");
        }

        var path = Path.GetFullPath(filename);
        Console.WriteLine($"Opening {path} in browser");

        var p = new Process();
        p.StartInfo = new ProcessStartInfo(path)
        {
            UseShellExecute = true
        };
        p.Start();
    }    
}

// dotnet run -- #airfryer
// dotnet run -- #wfh