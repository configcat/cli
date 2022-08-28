using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services.Exceptions;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trybot;
using Trybot.Retry.Model;

namespace ConfigCat.Cli.Services.Api;

public abstract class ApiClient
{
    private const string RetryingIdentifier = "retrying";

    protected IOutput Output { get; }

    private readonly CliConfig config;
    private readonly IBotPolicy<HttpResponseMessage> botPolicy;
    private readonly HttpClient httpClient;

    protected ApiClient(IOutput output,
        CliConfig config,
        IBotPolicy<HttpResponseMessage> botPolicy,
        HttpClient httpClient)
    {
        this.Output = output;
        this.config = config;
        this.botPolicy = botPolicy;
        this.httpClient = httpClient;

        this.botPolicy.Configure(policyBuilder => policyBuilder
            .Retry(retry => retry
                .WhenExceptionOccurs(exception => exception is HttpRequestException)
                .WhenResultIs(response => (int)response.StatusCode >= 500 ||
                                          response.StatusCode is System.Net.HttpStatusCode.RequestTimeout or System.Net.HttpStatusCode.TooManyRequests)
                .WithMaxAttemptCount(3)
                .WaitBetweenAttempts((attempt, exception, result) =>
                {
                    var backoffTime = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    if (result is not { StatusCode: System.Net.HttpStatusCode.TooManyRequests }) return backoffTime;
                    var retryAfter = result.Headers.RetryAfter.Delta;
                    return retryAfter ?? backoffTime;
                })
                .OnRetry(this.LogRetry)
            ));
    }

    protected async Task<TResult> GetAsync<TResult>(HttpMethod method, string path, CancellationToken token)
    {
        using var request = this.CreateRequest(method, path);

        this.Output.Verbose($"Initiating HTTP request: {method.Method} {path}", ConsoleColor.Cyan);
        using var response = await this.SendRequest(request, token);

        this.Output.Verbose($"HTTP response: {(int)response.StatusCode} {response.ReasonPhrase}",
            response.IsSuccessStatusCode ? ConsoleColor.Green : ConsoleColor.Red);

        var content = await response.Content.ReadAsStringAsync(token);
        this.Output.Verbose($"Response body: {content}");

        ValidateResponse(response);

        try
        {
            return JsonSerializer.Deserialize<TResult>(content, Constants.CamelCaseOptions);
        }
        catch (JsonException exception)
        {
            throw new JsonParsingFailedException("Invalid JSON response. Please make sure you're using the proper Management API URL (usually: api.configcat.com).", exception);
        }
    }

    protected async Task SendAsync(HttpMethod method, string path, object body, CancellationToken token)
    {
        using var request = this.CreateRequest(method, path);
        this.Output.Verbose($"Initiating Http request: {method.Method} {path}", ConsoleColor.Cyan);

        if (body is not null)
        {
            var jsonBody = JsonSerializer.Serialize(body, Constants.CamelCaseOptions);
            this.Output.Verbose($"Request body: {jsonBody}");

            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }

        using var response = await this.SendRequest(request, token);
        this.Output.Verbose($"HTTP response: {(int)response.StatusCode} {response.ReasonPhrase}",
            response.IsSuccessStatusCode ? ConsoleColor.Green : ConsoleColor.Red);

        var content = await response.Content.ReadAsStringAsync(token);
        this.Output.Verbose($"Response body: {content}");

        ValidateResponse(response);
    }

    protected async Task<TResult> SendAsync<TResult>(HttpMethod method, string path, object body, CancellationToken token)
    {
        using var request = this.CreateRequest(method, path);
        this.Output.Verbose($"Initiating HTTP request: {method.Method} {path}", ConsoleColor.Cyan);

        if (body is not null)
        {
            var jsonBody = JsonSerializer.Serialize(body, Constants.CamelCaseOptions);
            this.Output.Verbose($"Request body: {jsonBody}");

            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }

        using var response = await this.SendRequest(request, token);
        this.Output.Verbose($"HTTP response: {(int)response.StatusCode} {response.ReasonPhrase}",
            response.IsSuccessStatusCode ? ConsoleColor.Green : ConsoleColor.Red);

        var content = await response.Content.ReadAsStringAsync(token);
        this.Output.Verbose($"Response body: {content}");

        ValidateResponse(response);

        try
        {
            return JsonSerializer.Deserialize<TResult>(content, Constants.CamelCaseOptions);
        }
        catch (JsonException exception)
        {
            throw new JsonParsingFailedException("Invalid JSON response. Please make sure you're using the proper Management API URL (usually: api.configcat.com).", exception);
        }
    }

    private async Task<HttpResponseMessage> SendRequest(HttpRequestMessage request, CancellationToken token)
    {
        using var spinner = this.Output.CreateSpinner(token);
        return await this.botPolicy.ExecuteAsync(async (ctx, cancellationToken) =>
        {
            var isRetrying = ctx.GenericData.ContainsKey(RetryingIdentifier);
            var currentRequest = isRetrying ? await request.CloneAsync() : request;
            return await this.httpClient.SendAsync(await request.CloneAsync(), cancellationToken);
        }, token);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        var configAuth = this.config.Auth;
        var request = new HttpRequestMessage(method, new Uri(new Uri($"https://{configAuth.ApiHost}"), path));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"{configAuth.UserName}:{configAuth.Password}")));
        return request;
    }

    private void LogRetry(HttpResponseMessage result, Exception exception, AttemptContext context)
    {
        context.ExecutionContext.GenericData.TryAdd(RetryingIdentifier, 1);
        var message = result is not null
            ? $"Status code does not indicate success: {(int)result.StatusCode} {result.ReasonPhrase}"
            : $"Error occured: {exception?.Message}";
        this.Output.Verbose($"{message}, retrying... [{context.CurrentAttempt}. attempt, waiting {context.CurrentDelay}]", ConsoleColor.Yellow);
    }

    private static void ValidateResponse(HttpResponseMessage responseMessage)
    {
        if (!responseMessage.IsSuccessStatusCode)
            throw new HttpStatusException(responseMessage.StatusCode,
                responseMessage.ReasonPhrase);
    }
}