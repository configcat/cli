using ConfigCat.Cli.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trybot;
using Trybot.Retry.Model;

namespace ConfigCat.Cli.Services.Api
{
    public abstract class ApiClient
    {
        private const string RetryingIdentifier = "retrying";

        protected IExecutionContextAccessor Accessor { get; }

        private readonly IBotPolicy<HttpResponseMessage> botPolicy;
        private readonly HttpClient httpClient;

        protected ApiClient(IExecutionContextAccessor accessor,
            IBotPolicy<HttpResponseMessage> botPolicy,
            HttpClient httpClient)
        {
            this.Accessor = accessor;
            this.botPolicy = botPolicy;
            this.httpClient = httpClient;

            this.botPolicy.Configure(policyBuilder => policyBuilder
                .Retry(retry => retry
                    .WhenExceptionOccurs(exception => exception is HttpRequestException)
                    .WhenResultIs(response => (int)response.StatusCode >= 500 ||
                        response.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
                        response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    .WithMaxAttemptCount(3)
                    .WaitBetweenAttempts((attempt, exception, result) =>
                    {
                        var backoffTime = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                        if (result.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            var retryAfter = result.Headers.RetryAfter.Delta;
                            return retryAfter ?? backoffTime;
                        }

                        return backoffTime;
                    })
                    .OnRetry(this.LogRetry)
                ));
        }

        protected async Task<TResult> GetAsync<TResult>(HttpMethod method, string path, CancellationToken token)
        {
            using var request = this.CreateRequest(method, path, token);

            this.Accessor.ExecutionContext.Output.Verbose($"Initiating HTTP request: {method.Method} {path}", ForegroundColorSpan.LightCyan());
            using var response = await this.SendRequest(request, token);

            this.Accessor.ExecutionContext.Output.Verbose($"HTTP response: {(int)response.StatusCode} {response.ReasonPhrase}",
                response.IsSuccessStatusCode ? ForegroundColorSpan.LightGreen() : ForegroundColorSpan.LightRed());

            var content = await response.Content.ReadAsStringAsync();
            this.Accessor.ExecutionContext.Output.Verbose($"Response body: {content}");

            this.ValidateResponse(response);

            return JsonSerializer.Deserialize<TResult>(content, Constants.CamelCaseOptions);
        }

        protected async Task SendAsync(HttpMethod method, string path, object body, CancellationToken token)
        {
            using var request = this.CreateRequest(method, path, token);
            this.Accessor.ExecutionContext.Output.Verbose($"Initiating Http request: {method.Method} {path}", ForegroundColorSpan.LightCyan());

            if (body is not null)
            {
                var jsonBody = JsonSerializer.Serialize(body, Constants.CamelCaseOptions);
                this.Accessor.ExecutionContext.Output.Verbose($"Request body: {jsonBody}");

                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            }

            using var response = await this.SendRequest(request, token);
            this.Accessor.ExecutionContext.Output.Verbose($"HTTP response: {(int)response.StatusCode} {response.ReasonPhrase}", 
                response.IsSuccessStatusCode ? ForegroundColorSpan.LightGreen() : ForegroundColorSpan.LightRed());

            var content = await response.Content.ReadAsStringAsync();
            this.Accessor.ExecutionContext.Output.Verbose($"Response body: {content}");

            this.ValidateResponse(response);
        }

        protected async Task<TResult> SendAsync<TResult>(HttpMethod method, string path, object body, CancellationToken token)
        {
            using var request = this.CreateRequest(method, path, token);
            this.Accessor.ExecutionContext.Output.Verbose($"Initiating HTTP request: {method.Method} {path}", ForegroundColorSpan.LightCyan());

            if (body is not null)
            {
                var jsonBody = JsonSerializer.Serialize(body, Constants.CamelCaseOptions);
                this.Accessor.ExecutionContext.Output.Verbose($"Request body: {jsonBody}");

                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            }

            using var response = await this.SendRequest(request, token);
            this.Accessor.ExecutionContext.Output.Verbose($"HTTP response: {(int)response.StatusCode} {response.ReasonPhrase}",
                response.IsSuccessStatusCode ? ForegroundColorSpan.LightGreen() : ForegroundColorSpan.LightRed());

            var content = await response.Content.ReadAsStringAsync();
            this.Accessor.ExecutionContext.Output.Verbose($"Response body: {content}");

            this.ValidateResponse(response);

            return JsonSerializer.Deserialize<TResult>(content, Constants.CamelCaseOptions);
        }

        private async Task<HttpResponseMessage> SendRequest(HttpRequestMessage request, CancellationToken token)
        {
            using var spinner = this.Accessor.ExecutionContext.Output.CreateSpinner(token);
            return await this.botPolicy.ExecuteAsync(async (ctx, cancellationToken) =>
            {
                var isRetrying = ctx.GenericData.ContainsKey(RetryingIdentifier);
                var currentRequest = isRetrying ? await request.CloneAsync() : request;
                return await this.httpClient.SendAsync(await request.CloneAsync(), cancellationToken);
            }, token);                
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string path, CancellationToken token)
        {
            var config = this.Accessor.ExecutionContext.Config.Auth;
            var request = new HttpRequestMessage(method, new Uri(new Uri($"https://{config.ApiHost}"), path));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{config.UserName}:{config.Password}")));
            return request;
        }

        private void LogRetry(HttpResponseMessage result, Exception exception, AttemptContext context)
        {
            context.ExecutionContext.GenericData.TryAdd(RetryingIdentifier, 1);
            var message = result is not null
                ? $"Status code does not indicate success: {(int)result.StatusCode} {result.ReasonPhrase}"
                : $"Error occured: {exception?.Message}";
            this.Accessor.ExecutionContext.Output.Verbose($"{message}, retrying... [{context.CurrentAttempt}. attempt, waiting {context.CurrentDelay}]", ForegroundColorSpan.LightYellow());
        }

        private void ValidateResponse(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
                throw new HttpStatusException(responseMessage.StatusCode,
                    responseMessage.ReasonPhrase);
        }
    }
}
