using System.Net;
using System.Text;
using System.Text.Json;

namespace IgroCehApp.Tests.Infrastructure
{
    // stands for every outer service the API calls (Discord, VideoGamesCritic, Steam, OpenXBL, PSN): it becomes the primary handler
    // of every HttpClient of the app, so no request leaves the test; a test describes the answers it needs, an unknown request gets 404
    // and is remembered, so a test can see what the app asked for
    public class FakeExternalApis : HttpMessageHandler
    {
        private readonly List<(Func<HttpRequestMessage, bool> Matches, Func<HttpRequestMessage, string, HttpResponseMessage> Respond)> _routes = new();
        private readonly List<RecordedRequest> _requests = new();
        private readonly object _lock = new();

        public IReadOnlyList<RecordedRequest> Requests
        {
            get
            {
                lock (_lock)
                {
                    return _requests.ToList();
                }
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _routes.Clear();
                _requests.Clear();
            }
        }

        // the last added route wins, so a test can replace an answer it set before
        public void On(HttpMethod method, string url, Func<HttpRequestMessage, string, HttpResponseMessage> respond)
        {
            lock (_lock)
            {
                _routes.Insert(0, (request => request.Method == method && Address(request).Equals(url, StringComparison.OrdinalIgnoreCase), respond));
            }
        }

        public void OnGet(string url, object json, HttpStatusCode status = HttpStatusCode.OK) =>
            On(HttpMethod.Get, url, (_, _) => Json(json, status));

        public void OnPost(string url, object json, HttpStatusCode status = HttpStatusCode.OK) =>
            On(HttpMethod.Post, url, (_, _) => Json(json, status));

        public int CountOf(string url) => Requests.Count(r => r.Address.Equals(url, StringComparison.OrdinalIgnoreCase));

        public static HttpResponseMessage Json(object json, HttpStatusCode status = HttpStatusCode.OK) => new(status)
        {
            Content = new StringContent(JsonSerializer.Serialize(json), Encoding.UTF8, "application/json"),
        };

        public static HttpResponseMessage Text(string text, HttpStatusCode status = HttpStatusCode.OK) => new(status)
        {
            Content = new StringContent(text, Encoding.UTF8, "text/plain"),
        };

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content == null ? "" : await request.Content.ReadAsStringAsync(cancellationToken);
            Func<HttpRequestMessage, string, HttpResponseMessage> respond;
            lock (_lock)
            {
                _requests.Add(new RecordedRequest(request.Method, Address(request), request.RequestUri!.Query, body,
                    request.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value))));
                respond = _routes.FirstOrDefault(route => route.Matches(request)).Respond;
            }

            var response = respond?.Invoke(request, body) ?? Text($"No fake answer for {request.Method} {request.RequestUri}", HttpStatusCode.NotFound);
            response.RequestMessage = request;
            return response;
        }

        // HttpClientFactory disposes the handlers it rotates, the fake lives as long as the app
        protected override void Dispose(bool disposing)
        {
        }

        // the address without the query, the routes are matched by it
        private static string Address(HttpRequestMessage request) => request.RequestUri!.GetLeftPart(UriPartial.Path);
    }

    public record RecordedRequest(HttpMethod Method, string Address, string Query, string Body, Dictionary<string, string> Headers);
}
