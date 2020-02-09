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

        protected ApiClient(HttpClient httpClient)
        {
            // Validate parameters.
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        #endregion

        #region Instance, read-only state.

        protected HttpClient HttpClient { get; }

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

            // Get the response.
            using HttpResponseMessage originalResponse = await HttpClient
                .GetAsync(url, cancellationToken)
                .ConfigureAwait(false);

            // Process the response message.
            using var _ = await ProcessHttpResponseMessageAsync(originalResponse, cancellationToken)
                .ConfigureAwait(false);
        }

        protected virtual Task<T> GetAsync<T>(string url, CancellationToken cancellationToken)
            => GetAsync<T, T>(url, (_, r) => r, cancellationToken);

        protected abstract Task<TReturn> GetAsync<TResponse, TReturn>(
            string url,
            Func<HttpResponseMessage, TResponse, TReturn> transformer,
            CancellationToken cancellationToken
        );

        protected abstract Task PostAsync<TRequest>(string url,
            TRequest request, CancellationToken cancellationToken);

        protected virtual Task<TResponse> PostAsync<TRequest, TResponse>(
            string url,
            TRequest request,
            CancellationToken cancellationToken
        ) => PostAsync<TRequest, TResponse, TResponse>(url, request, (_, r) => r, cancellationToken);

        protected abstract Task<TReturn> PostAsync<TRequest, TResponse, TReturn>(
            string url,
            TRequest request, 
            Func<HttpResponseMessage, TResponse, TReturn> transformer,
            CancellationToken cancellationToken
        );


        protected virtual async Task DeleteAsync(string url, CancellationToken cancellationToken)
        {
            // Validate parameters.
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));

            // Format the URL.
            url = await FormatUrlAsync(url, cancellationToken).ConfigureAwait(false);

            // Get the response.
            using HttpResponseMessage originalResponse = await HttpClient
                .DeleteAsync(url, cancellationToken)
                .ConfigureAwait(false);

            // Process the response message.
            using var _ = await ProcessHttpResponseMessageAsync(originalResponse, cancellationToken)
                .ConfigureAwait(false);
        }

        protected virtual Task<TResponse> DeleteAsync<TResponse>(
            string url,
            CancellationToken cancellationToken
        ) => DeleteAsync<TResponse, TResponse>(url, (_, r) => r, cancellationToken);

        protected abstract Task<TReturn> DeleteAsync<TResponse, TReturn>(
            string url,
            Func<HttpResponseMessage, TResponse, TReturn> transformer,
            CancellationToken cancellationToken
        );

        #endregion
    }
}
