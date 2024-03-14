namespace Tool.Compet.Http;

using System.Net;
using System.Net.Http.Headers;
using Tool.Compet.Core;
using Tool.Compet.Json;

public class DkHttpClient {
	/// HttpClient is designed for concurrency, so we just use 1 instance of it on
	/// multiple requests instead of making new instance per request.
	public readonly HttpClient httpClient;

	public DkHttpClient() {
		this.httpClient = new HttpClient();

		// Use below if we wanna change default max-concurrency (default 10)
		// this.httpClient = new HttpClient(new HttpClientHandler() {
		// 	MaxConnectionsPerServer = 20
		// });
	}

	/// Set default request header for all requests.
	public DkHttpClient SetRequestHeaderEntry(string key, string value) {
		this.httpClient.DefaultRequestHeaders.Add(key, value);
		return this;
	}

	/// Set default request header for all requests.
	/// @param scheme: For eg,. "Bearer", or just a token "Akasdjka02mma"
	/// @param token: For eg,. "Aksdtkasl2910dks", or just be null if the scheme was token.
	public DkHttpClient SetAuthorization(string schema, string? token = null) {
		this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(schema, token);
		return this;
	}

	/// Set wait timeout for the request.
	/// If the request wait over the timeout, the request will be cancelled.
	public DkHttpClient SetTimeOut(TimeSpan timeout) {
		this.httpClient.Timeout = timeout;
		return this;
	}

	/// Sends a GET request, and return json-decoded response as `DkApiResponse`-based type.
	public async Task<T> GetAsync<T>(string url) where T : DkApiResponse {
		// Perform try/catch for whole process
		try {
			return await this.GetOrThrowAsync<T>(url);
		}
		catch (Exception e) {
			// 	DkLogs.Warning(this, $"Error when GET ! error: {e.Message}");

			return DkObjects.NewInstace<T>().AlsoDk(res => {
				res.status = 0;
				res.msg = e.Message;
			});
		}
	}

	public async Task<T> GetOrThrowAsync<T>(string url) where T : DkApiResponse {
		// To check with larger range: !result.IsSuccessStatusCode
		var response = await this.GetRawAsync(url);
		if (response.StatusCode != HttpStatusCode.OK) {
			// 	DkLogs.Warning(this, $"NG response ({result.StatusCode}) when Get, reason: {result.ReasonPhrase}");

			return DkObjects.NewInstace<T>().AlsoDk(res => {
				res.status = (int)response.StatusCode;
				res.msg = response.ReasonPhrase;
			});
		}

		// Add `!` to tell compiler that body and result are non-null.
		// Or use: return DkJsons.ToObj<T>(await result.Content.ReadAsStringAsync())!;
		return (await response.Content.ReadFromJsonAsync<T>())!;
	}

	/// Sends a GET request, and just return json-decoded response for given type.
	/// Take note that, returned non-null response does NOT indicate the request has succeed response.
	/// @return Object of given type if succeed. Otherwise null.
	public async Task<T?> GetForTypeAsync<T>(string url) where T : class {
		// Perform try/catch for whole process
		try {
			return await this.GetForTypeOrThrowAsync<T>(url);
		}
		catch (Exception e) {
			// 	DkLogs.Warning(this, $"Error when GetForType ! error: {e.Message}");
			return null;
		}
	}

	/// Sends a GET request, and just return json-decoded response for given type.
	/// Take note that, returned non-null response does NOT indicate the request has succeed response.
	/// @return Object of given type if succeed. Otherwise null.
	public async Task<T?> GetForTypeOrThrowAsync<T>(string url) where T : class {
		// To check with larger range: !result.IsSuccessStatusCode
		var response = await this.GetRawAsync(url);
		if (response.StatusCode != HttpStatusCode.OK) {
			// 	DkLogs.Warning(this, $"NG response ({result.StatusCode}) when GetForType, reason: {result.ReasonPhrase}");
			return null;
		}

		return await response.Content.ReadFromJsonAsync<T>();
	}

	/// Sends a GET request, and just response as string type.
	/// @return Nullable body in string.
	public async Task<string?> GetForStringAsync(string url) {
		// Perform try/catch for whole process
		try {
			return await this.GetForStringOrThrowAsync(url);
		}
		catch (Exception e) {
			// 	DkLogs.Warning(this, $"Error when GetForString ! error: {e.Message}");
			return null;
		}
	}

	/// Sends a GET request, and just response as string type.
	/// @return Nullable body in string.
	public async Task<string?> GetForStringOrThrowAsync(string url) {
		var response = await this.GetRawAsync(url);
		if (response.StatusCode != HttpStatusCode.OK) {
			// 	DkLogs.Warning(this, $"NG response ({result.StatusCode}) when GetForString, reason: {result.ReasonPhrase}");
		}

		return await response.Content.ReadAsStringAsync();
	}

	/// Sends a GET request, and get raw response.
	/// @return Raw response so caller can read with various format (byte, string, json, stream, ...).
	public async Task<HttpResponseMessage> GetRawAsync(string url) {
		return await this.httpClient.GetAsync(url);
	}

	/// Sends a POST request, and return json-decoded response as `DkApiResponse`-based type.
	/// @param body: Can be Dictionary, json-serialized object,...
	public async Task<T> PostAsync<T>(string url, object? body = null) where T : DkApiResponse {
		// Perform try/catch for whole process
		try {
			return await this.PostOrThrowAsync<T>(url, body);
		}
		catch (Exception e) {
			// DkLogs.Warning(this, $"Error when Post ! error: {e.Message}");

			return DkObjects.NewInstace<T>().AlsoDk(res => {
				res.status = 0;
				res.msg = e.Message;
			});
		}
	}

	/// Sends a POST request, and return json-decoded response as `DkApiResponse`-based type.
	/// @param body: Can be Dictionary, json-serialized object,...
	public async Task<T> PostOrThrowAsync<T>(string url, object? body = null) where T : DkApiResponse {
		var response = await this.PostRawAsync(url, body);

		// // For ASP.NET environment:
		// var response = await httpClient.PostAsJsonAsync(url, body);

		// To check with larger range: !result.IsSuccessStatusCode
		if (response.StatusCode != HttpStatusCode.OK) {
			// 	DkLogs.Warning(this, $"NG response ({response.StatusCode}) when Post, reason: {response.ReasonPhrase}");

			return DkObjects.NewInstace<T>().AlsoDk(res => {
				res.status = (int)response.StatusCode;
				res.msg = response.ReasonPhrase;
			});
		}

		return (await response.Content.ReadFromJsonAsync<T>())!;
	}

	/// Sends a POST request, and return json-decoded response as given type.
	/// @param body: Can be Dictionary, json-serialized object,...
	/// @return Nullable object in given type.
	public async Task<T?> PostForTypeAsync<T>(string url, object? body = null) where T : class {
		// Perform try/catch for whole process
		try {
			return await this.PostForTypeOrThrowAsync<T>(url, body);
		}
		catch (Exception e) {
			// DkLogs.Warning(this, $"Error when PostForType ! error: {e.Message}");
			return null;
		}
	}

	/// Sends a POST request, and return json-decoded response as given type.
	/// @param body: Can be Dictionary, json-serialized object,...
	/// @return Nullable object in given type.
	public async Task<T?> PostForTypeOrThrowAsync<T>(string url, object? body = null) where T : class {
		var response = await this.PostRawAsync(url, body);
		return await response.Content.ReadFromJsonAsync<T>();
	}

	/// Sends a POST request, and return just response as string type.
	/// @param body: Can be Dictionary, json-serialized object,...
	public async Task<string?> PostForStringAsync(string url, object? body = null) {
		try {
			return await this.PostForStringOrThrowAsync(url, body);
		}
		catch (Exception e) {
			// DkLogs.Warning(this, $"Error when PostForString ! error: {e.Message}");
			return null;
		}
	}

	/// Sends a POST request, and return just response as string type.
	/// @param body: Can be Dictionary, json-serialized object,...
	public async Task<string?> PostForStringOrThrowAsync(string url, object? body = null) {
		var response = await this.PostRawAsync(url, body);
		return await response.Content.ReadAsStringAsync();
	}

	/// Sends a POST request with payload as json, and return raw response.
	/// @param body: Can be Dictionary, json-serialized object,...
	/// @return Raw response so caller can read with various format (byte, string, json, stream, ...).
	public async Task<HttpResponseMessage> PostRawAsync(string url, object? body = null) {
		var json = body == null ? null : DkJsons.ToJson(body);

		// Other content types:
		// - StreamContent
		// - ByteArrayContent (StringContent, FormUrlEncodedContent)
		// - MultipartContent (MultipartFormDataContent)
		var stringContent = json == null ? null : new StringContent(json, System.Text.Encoding.UTF8, "application/json");
		var response = await this.httpClient.PostAsync(url, stringContent);

		// // For ASP.NET environment:
		// var response = await httpClient.PostAsJsonAsync(url, body);

		// if (response.StatusCode != HttpStatusCode.OK) {
		// 	// 	DkLogs.Warning(this, $"NG response ({response.StatusCode}) when PostForString, reason: {response.ReasonPhrase}");
		// }

		return response;
	}

	/// Still in development !!
	/// This is detail implementation for sending request.
	/// Note that, `Get(), Post()` in this class are convenient versions of this method.
	private async Task<HttpResponseMessage> __FullSendAsync(string url, HttpMethod method, CancellationToken cancellationToken, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead) {
		// Make request data
		var request = new HttpRequestMessage {
			Method = method,
			RequestUri = new Uri(url),
			// Headers in here are for this request
			Headers = {
					// { HttpRequestHeader.Authorization.ToString(), $"Bearer {accessToken}"},
					// { HttpRequestHeader.ContentType.ToString(), "application/json" },
				},
			// We can attach multiple contents for this request
			Content = new MultipartContent {
					new StringContent(""),
					new ByteArrayContent([1, 3]),
				},
		};

		return await this.httpClient.SendAsync(request, completionOption, cancellationToken);
	}
}
