namespace Tool.Compet.Http {
	using System.Net;
	using System.Net.Http.Headers;
	using Tool.Compet.Core;
	using Tool.Compet.Json;
	using Tool.Compet.Log;

	public class DkHttpClient {
		/// HttpClient is designed for concurrency, so we just use 1 instance of it on
		/// multiple requests instead of making new instance per request.
		private readonly HttpClient httpClient;

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
		/// @param schema: For eg,. "Bearer"
		/// @param token: For eg,. "Aksdtkasl2910dks"
		public DkHttpClient SetAuthorization(string schema, string token) {
			this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(schema, token);
			return this;
		}

		/// Sends a GET request, and return json-decoded response as `DkApiResponse`-based type.
		public async Task<T> Get<T>(string url) where T : DkApiResponse {
			// Perform try/catch for whole process
			try {
				// To check with larger range: !result.IsSuccessStatusCode
				var result = await httpClient.GetAsync(url);
				if (result.StatusCode != HttpStatusCode.OK) {
					if (DkBuildConfig.DEBUG) {
						DkLogs.Warning(this, $"NG response ({result.StatusCode}) when Get, reason: {result.ReasonPhrase}");
					}

					return DkObjects.NewInstace<T>().AlsoDk(res => {
						res.status = ((int)result.StatusCode);
						res.message = result.ReasonPhrase;
					});
				}

				// Add `!` to tell compiler that body and result are non-null.
				return (await result.Content.ReadFromJsonAsync<T>())!;
			}
			catch (Exception e) {
				if (DkBuildConfig.DEBUG) {
					DkLogs.Warning(this, $"Error when GET ! error: {e.Message}");
				}

				return DkObjects.NewInstace<T>().AlsoDk(res => {
					res.status = 0;
					res.code = "dkerror";
					res.message = e.Message;
				});
			}
		}

		/// Sends a GET request, and just return json-decoded response for given type.
		/// Take note that, returned non-null response does NOT indicate the request has succeed response.
		/// @return Object of given type if succeed. Otherwise null.
		public async Task<T?> GetForType<T>(string url) where T : class {
			// Perform try/catch for whole process
			try {
				// To check with larger range: !result.IsSuccessStatusCode
				var result = await httpClient.GetAsync(url);
				if (result.StatusCode != HttpStatusCode.OK) {
					if (DkBuildConfig.DEBUG) {
						DkLogs.Warning(this, $"NG response ({result.StatusCode}) when GetForType, reason: {result.ReasonPhrase}");
					}
					return null;
				}

				return await result.Content.ReadFromJsonAsync<T>();
			}
			catch (Exception e) {
				if (DkBuildConfig.DEBUG) {
					DkLogs.Warning(this, $"Error when GetForType ! error: {e.Message}");
				}

				return null;
			}
		}

		/// Sends a GET request, and just response as string type.
		/// @return Nullable body in string.
		public async Task<string?> GetForString(string url) {
			// Perform try/catch for whole process
			try {
				// To check with larger range: !result.IsSuccessStatusCode
				var result = await httpClient.GetAsync(url);
				if (result.StatusCode != HttpStatusCode.OK) {
					if (DkBuildConfig.DEBUG) {
						DkLogs.Warning(this, $"NG response ({result.StatusCode}) when GetForString, reason: {result.ReasonPhrase}");
					}
				}

				return await result.Content.ReadAsStringAsync();
			}
			catch (Exception e) {
				if (DkBuildConfig.DEBUG) {
					DkLogs.Warning(this, $"Error when GetForString ! error: {e.Message}");
				}

				return null;
			}
		}

		/// Sends a POST request, and return json-decoded response as `DkApiResponse`-based type.
		/// @param body: Can be Dictionary, json-serialized object,...
		public async Task<T> Post<T>(string url, object? body = null) where T : DkApiResponse {
			// Perform try/catch for whole process
			try {
				var json = body == null ? null : DkJsons.Obj2Json(body);

				// Other content types:
				// - StreamContent
				// - ByteArrayContent (StringContent, FormUrlEncodedContent)
				// - MultipartContent (MultipartFormDataContent)
				var stringContent = json == null ? null : new StringContent(json, System.Text.Encoding.UTF8, "application/json");
				var response = await httpClient.PostAsync(url, stringContent);

				// // For ASP.NET environment:
				// var response = await httpClient.PostAsJsonAsync(url, body);

				// To check with larger range: !result.IsSuccessStatusCode
				if (response.StatusCode != HttpStatusCode.OK) {
					if (DkBuildConfig.DEBUG) {
						DkLogs.Warning(this, $"NG response ({response.StatusCode}) when Post, reason: {response.ReasonPhrase}");
					}

					return DkObjects.NewInstace<T>().AlsoDk(res => {
						res.status = ((int)response.StatusCode);
						res.message = response.ReasonPhrase;
					});
				}

				// Add `!` to tell compiler that body and result are non-null.
				return (await response.Content.ReadFromJsonAsync<T>())!;
			}
			catch (Exception e) {
				if (DkBuildConfig.DEBUG) { DkLogs.Warning(this, $"Error when Post ! error: {e.Message}"); }

				return DkObjects.NewInstace<T>().AlsoDk(res => {
					res.status = 0;
					res.code = "dkerror";
					res.message = e.Message;
				});
			}
		}

		/// Sends a POST request, and return json-decoded response as given type.
		/// @param body: Can be Dictionary, json-serialized object,...
		/// @return Nullable object in given type.
		public async Task<T?> PostForType<T>(string url, object? body = null) where T : class {
			// Perform try/catch for whole process
			try {
				var json = body == null ? null : DkJsons.Obj2Json(body);

				// Other content types:
				// - StreamContent
				// - ByteArrayContent (StringContent, FormUrlEncodedContent)
				// - MultipartContent (MultipartFormDataContent)
				var stringContent = json == null ? null : new StringContent(json, System.Text.Encoding.UTF8, "application/json");
				var response = await httpClient.PostAsync(url, stringContent);

				// // For ASP.NET environment:
				// var response = await httpClient.PostAsJsonAsync(url, body);

				// To check with larger range: !result.IsSuccessStatusCode
				if (response.StatusCode != HttpStatusCode.OK) {
					if (DkBuildConfig.DEBUG) {
						DkLogs.Warning(this, $"NG response ({response.StatusCode}) when PostForType, reason: {response.ReasonPhrase}");
					}
				}

				return await response.Content.ReadFromJsonAsync<T>();
			}
			catch (Exception e) {
				if (DkBuildConfig.DEBUG) { DkLogs.Warning(this, $"Error when PostForType ! error: {e.Message}"); }

				return null;
			}
		}

		/// Sends a POST request, and return just response as string type.
		/// @param body: Can be Dictionary, json-serialized object,...
		public async Task<string?> PostForString(string url, object? body = null) {
			// Perform try/catch for whole process
			try {
				var json = body == null ? null : DkJsons.Obj2Json(body);

				// Other content types:
				// - StreamContent
				// - ByteArrayContent (StringContent, FormUrlEncodedContent)
				// - MultipartContent (MultipartFormDataContent)
				var stringContent = json == null ? null : new StringContent(json, System.Text.Encoding.UTF8, "application/json");
				var response = await httpClient.PostAsync(url, stringContent);

				// // For ASP.NET environment:
				// var response = await httpClient.PostAsJsonAsync(url, body);

				// To check with larger range: !result.IsSuccessStatusCode
				if (response.StatusCode != HttpStatusCode.OK) {
					if (DkBuildConfig.DEBUG) {
						DkLogs.Warning(this, $"NG response ({response.StatusCode}) when PostForString, reason: {response.ReasonPhrase}");
					}
				}

				// Add `!` to tell compiler that body and result are non-null.
				return await response.Content.ReadAsStringAsync();
			}
			catch (Exception e) {
				if (DkBuildConfig.DEBUG) { DkLogs.Warning(this, $"Error when PostForString ! error: {e.Message}"); }

				return null;
			}
		}

		/// Still in development !!
		/// This is detail implementation for sending request.
		/// Note that, `Get(), Post()` in this class are convenient versions of this method.
		private async Task<T> __Send<T>(string url, HttpMethod method) where T : DkApiResponse {
			// Perform try/catch for whole process
			try {
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
						new ByteArrayContent(new byte[] {1, 3}),
					},
				};

				var response = await httpClient.SendAsync(request, CancellationToken.None);
				var responseBody = await response.Content.ReadAsStringAsync();

				// To check with larger range: !result.IsSuccessStatusCode
				if (response.StatusCode != HttpStatusCode.OK) {
					if (DkBuildConfig.DEBUG) {
						DkLogs.Warning(this, "Send failed ! responseBody", responseBody);
					}

					return DkObjects.NewInstace<T>().AlsoDk(res => {
						res.status = ((int)response.StatusCode);
						res.message = responseBody;
					});
				}

				// Add `!` to tell compiler that body and result are non-null.
				return DkJsons.Json2Obj<T>(responseBody!)!;
			}
			catch (Exception e) {
				if (DkBuildConfig.DEBUG) {
					DkLogs.Warning(this, $"Error when Send ! error: {e.Message}");
				}

				return DkObjects.NewInstace<T>().AlsoDk(res => {
					res.status = 0;
					res.code = "dkerror";
					res.message = e.Message;
				});
			}
		}
	}
}
