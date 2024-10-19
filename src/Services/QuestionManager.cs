using System.Text.Json;
using FifteenToOne.Models;

namespace FifteenToOne.Services;

public sealed class QuestionManager
{
    public IReadOnlyList<Question> Questions { get; private set; } = [];

    public async Task LoadAsync()
    {
        using var client = new HttpClient();

        var url = "https://gist.githubusercontent.com/marcel2215/9097bd792600e9249e3f73aa5cbe4f19/raw/a60ee40cb1564fd03df425406936663ad2e61252/quiz-questions-pl.json";
        var json = await client.GetStreamAsync(url);

        using var document = await JsonDocument.ParseAsync(json);
        var questions = new List<Question>();

        foreach (var question in document.RootElement.EnumerateArray())
        {
            var content = question.GetProperty("q").GetString();
            var answer = question.GetProperty("a").GetString();

            if (!string.IsNullOrWhiteSpace(content) && !string.IsNullOrWhiteSpace(answer))
            {
                questions.Add(new Question(content, answer));
            }
        }

        Questions = questions;
    }

    public Question GetRandomQuestion()
    {
        return Questions[Random.Shared.Next(Questions.Count)];
    }
}

public static class QuestionManagerExtensions
{
    public static IServiceCollection AddQuestionManager(this IServiceCollection services)
    {
        return services.AddSingleton<QuestionManager>();
    }

    public static IApplicationBuilder InitializeQuestionManager(this IApplicationBuilder builder)
    {
        builder.ApplicationServices.GetRequiredService<QuestionManager>().LoadAsync().Wait();
        return builder;
    }
}
