#nullable enable

using Garyon.Functions;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace AdventOfCode;

public static class WebsiteScraping
{
    private static readonly Regex puzzleAnswerPattern = new(@"Your puzzle answer was <code>(?'answer'.*)</code>");

    public static string DownloadInput(int year, int day, bool outputLog = false)
    {
        var inputURI = GetProblemInputURI(year, day);
        return DownloadContent(inputURI, outputLog);
    }
    public static string GetProblemInputURI(int year, int day) => $"{GetProblemURI(year, day)}/input";
    public static string GetProblemURI(int year, int day) => $"https://adventofcode.com/{year}/day/{day}";

    private static string DownloadContent(string targetURI, bool outputLog = false)
    {
        if (SecretsStorage.Cookies is null)
            throw new InvalidOperationException("No cookie container class to use during input retrieval has been specified.");

        using var client = new HttpClient();
        SecretsStorage.Cookies.AddToDefaultRequestHeaders(client);

        while (true)
        {
            try
            {
                if (outputLog)
                    Console.WriteLine("Downloading input from the website...");

                var response = client.GetAsync(targetURI).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;

                if (outputLog)
                    Console.WriteLine("Input downloaded\n");

                return responseString;
            }
            catch (HttpRequestException requestException)
            {
                if (outputLog)
                    ConsoleUtilities.WriteExceptionInfo(requestException);
            }
            // Other exceptions are not to be handled
        }
    }

    public static ProblemOutput DownloadAnsweredCorrectOutputs(int year, int day)
    {
        var inputURI = GetProblemURI(year, day);
        var content = DownloadContent(inputURI, false);
        return ParseAnsweredCorrectOutputs(content);
    }

    private static ProblemOutput ParseAnsweredCorrectOutputs(string siteContents)
    {
        var matches = puzzleAnswerPattern.Matches(siteContents);
        return ProblemOutput.Parse(matches.Select(match => match.Groups["answer"].Value).ToArray());
    }
}
