﻿// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace Microsoft.Toolkit.Uwp
{
    /// <summary>
    /// This class exposes functionality of HttpClient through a singleton to take advantage of built-in connection pooling.
    /// </summary>
    public class HttpHelper
    {
        /// <summary>
        /// Maximum number of Http Clients that can be pooled.
        /// </summary>
        private const int MaxPoolSize = 10;

        /// <summary>
        /// Private singleton field.
        /// </summary>
        private static HttpHelper _instance;

        private SemaphoreSlim _semaphore = new SemaphoreSlim(MaxPoolSize);

        /// <summary>
        /// Private instance field.
        /// </summary>
        private ConcurrentQueue<HttpClient> _httpClientQueue = null;

        /// <summary>
        /// Gets public singleton property.
        /// </summary>
        public static HttpHelper Instance => _instance ?? (_instance = new HttpHelper());

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHelper"/> class.
        /// </summary>
        protected HttpHelper()
        {
            _httpClientQueue = new ConcurrentQueue<HttpClient>();
        }

        /// <summary>
        /// Process Http Request using instance of HttpClient.
        /// </summary>
        /// <param name="request">instance of <see cref="HttpHelperRequest"/></param>
        /// <returns>Instane of <see cref="HttpHelperResponse"/></returns>
        public async Task<HttpHelperResponse> SendRequestAsync(HttpHelperRequest request)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);

            HttpClient client = null;

            try
            {
                var httpRequestMessage = request.ToHttpRequestMessage();

                client = GetHttpClientInstance();

                var response = await client.SendRequestAsync(httpRequestMessage).AsTask().ConfigureAwait(false);

                FixInvalidCharset(response);

                return new HttpHelperResponse(response);
            }
            finally
            {
                // Add the HttpClient instance back to the queue.
                if (client != null)
                {
                    _httpClientQueue.Enqueue(client);
                }

                _semaphore.Release();
            }
        }

        private HttpClient GetHttpClientInstance()
        {
            HttpClient client = null;

            // Try and get HttpClient from the queue
            if (!_httpClientQueue.TryDequeue(out client))
            {
                var filter = new HttpBaseProtocolFilter();
                filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;

                client = new HttpClient(filter);
            }

            return client;
        }

        /// <summary>
        /// Fix invalid charset returned by some web sites.
        /// </summary>
        /// <param name="response">HttpResponseMessage instance.</param>
        private void FixInvalidCharset(HttpResponseMessage response)
        {
            if (response != null && response.Content != null && response.Content.Headers != null
                && response.Content.Headers.ContentType != null && response.Content.Headers.ContentType.CharSet != null)
            {
                // Fix invalid charset returned by some web sites.
                string charset = response.Content.Headers.ContentType.CharSet;
                if (charset.Contains("\""))
                {
                    response.Content.Headers.ContentType.CharSet = charset.Replace("\"", string.Empty);
                }
            }
        }
    }
}
