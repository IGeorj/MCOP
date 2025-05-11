using System.Net.Http.Headers;

namespace MCOP.Core.Services.Shared;

public static class HttpService
{
    private static readonly HttpClient _client;
    private static readonly HttpClientHandler _handler;

    static HttpService()
    {
        _handler = new HttpClientHandler { AllowAutoRedirect = false };
        _client = new HttpClient(_handler, disposeHandler: true)
        {
            Timeout = TimeSpan.FromMinutes(2)
        };
        _client.DefaultRequestHeaders.UserAgent.ParseAdd("MCOP/1.0 (by georj)");
    }

    public static async Task<(HttpResponseHeaders, HttpContentHeaders)> HeadAsync(Uri requestUri)
    {
        using var m = new HttpRequestMessage(HttpMethod.Head, requestUri);
        HttpResponseMessage resp = await _client.SendAsync(m);
        return (resp.Headers, resp.Content.Headers);
    }

    public static Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage)
        => _client.SendAsync(requestMessage);

    public static Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        => _client.PostAsync(requestUri, content);

    public static Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent content)
        => _client.PostAsync(requestUri, content);

    public static Task<HttpResponseMessage> GetAsync(string requestUri)
        => _client.GetAsync(requestUri);

    public static Task<string> GetStringAsync(Uri requestUri)
        => _client.GetStringAsync(requestUri);

    public static Task<string> GetStringAsync(string requestUri)
        => _client.GetStringAsync(requestUri);

    public static Task<Stream> GetStreamAsync(Uri requestUri)
        => _client.GetStreamAsync(requestUri);

    public static Task<Stream> GetStreamAsync(string requestUri)
        => _client.GetStreamAsync(requestUri);

    public static Task<MemoryStream> GetMemoryStreamAsync(Uri requestUri)
        => GetMemoryStreamAsync(requestUri);

    public static Task<byte[]> GetByteArrayAsync(string? requestUri)
        => _client.GetByteArrayAsync(requestUri);

    public static Task<byte[]> GetByteArrayAsync(Uri requestUri)
        => _client.GetByteArrayAsync(requestUri);

    public static async Task<MemoryStream> GetMemoryStreamAsync(string requestUri)
    {
        using Stream stream = await GetStreamAsync(requestUri);
        var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

}
