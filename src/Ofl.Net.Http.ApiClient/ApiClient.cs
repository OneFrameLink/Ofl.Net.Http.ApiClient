using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Ofl.Threading.Tasks;

namespace Ofl.Net.Http.ApiClient
{
    public abstract class ApiClient
    {
        #region Constructor

        protected ApiClient(IHttpClientFactory httpClientFactory) : this(httpClientFactory, Microsoft.Extensions.Options.Options.DefaultName)
        { }

        protected ApiClient(IHttpClientFactory httpClientFactory, string httpClientName)
        {
            // Validate parameters.
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpClientName = string.IsNullOrWhiteSpace(httpClientName) ? throw new ArgumentNullException(nameof(httpClientName)) : httpClientName;
        }

        #endregion

        #region Instance, read-only state.

        private readonly IHttpClientFactory _httpClientFactory;

        private readonly string _httpClientName;

        #endregion

        #region Helpers

        private HttpClient CreateHttpClient() => _httpClientFactory.CreateClient(_httpClientName);

        #endregion

        #region Overrides

        protected virtual ValueTask<string> FormatUrlAsync(string url, CancellationToken cancellationToken)
        {
            // Validate parameters.
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));

            // Just return the URL.
            return ValueTaskExtensions.FromResult(url);
        }

        protected virtual Task<HttpResponseMessage> ProcessHttpResponseMessageAsync(
            HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken)
        {
            // Validate parameters.
            if (httpResponseMessage == null) throw new ArgumentNullException(nameof(httpResponseMessage));

            // Ensure success.
            httpResponseMessage.EnsureSuccessStatusCode();

            // Return the message.
            return Task.FromResult(httpResponseMessage);
        }

        protected virtual async Task GetAsync(string url, CancellationToken cancellationToken)
        {
            // Validate parameters.
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));

            // Format the URL.
            url = await FormatUrlAsync(url, cancellationToken).ConfigureAwait(false);

            // Get the http client.
            HttpClient client = CreateHttpClient();

            // Get the response.
            using (HttpResponseMessage originalResponse = await client.GetAsync(url, cancellationToken).ConfigureAwait(false))
                // Process the response message.
            using (await ProcessHttpResponseMessageAsync(originalResponse, cancellationToken).ConfigureAwait(false))
            { }
        }

        protected abstract Task<T> GetAsync<T>(string url, CancellationToken cancellationToken);

        protected abstract Task<TResponse> PostAsync<TRequest, TResponse>(string url,
            TRequest request, CancellationToken cancellationToken);

        protected abstract Task PostAsync<TRequest>(string url,
            TRequest request, CancellationToken cancellationToken);

        protected virtual async Task DeleteAsync(string url, CancellationToken cancellationToken)
        {
            // Validate parameters.
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));

            // Format the URL.
            url = await FormatUrlAsync(url, cancellationToken).ConfigureAwait(false);

            // Get the http client.
            HttpClient client = CreateHttpClient();

            // Get the response.
            using (HttpResponseMessage originalResponse = await client.DeleteAsync(url, cancellationToken).ConfigureAwait(false))
                // Process the response message.
            using (await ProcessHttpResponseMessageAsync(originalResponse, cancellationToken).ConfigureAwait(false))
            { }
        }

        protected abstract Task<TResponse> DeleteAsync<TResponse>(string url, CancellationToken cancellationToken);

        #endregion
    }
}
