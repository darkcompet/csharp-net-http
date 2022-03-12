using System.Net;
using System.Net.Http.Headers;
using Tool.Compet.Core;
using Tool.Compet.Json;
using Tool.Compet.Log;

namespace Tool.Compet.Http {
	public class DkHttp {
		/// HttpClient is designed for concurrency, so we just use 1 instance of it on
		/// multiple requests instead of making new instance per request.
		private readonly HttpClient httpClient;

		public DkHttp() {
			this.httpClient = new HttpClient();

			// Use below if we wanna change default max-concurrency (default 10)
			// this.httpClient = new HttpClient(new HttpClientHandler() {
			// 	MaxConnectionsPerServer = 20
			// });
		}

		/// Set default request header for all requests.
		public DkHttp SetDefaultRequestHeader(string key, string value) {
			httpClient.DefaultRequestHeaders.Add(key, value);
			return this;
		}

		/// Set default request header for all requests.
		/// @param schema: For eg,. "Bearer"
		/// @param token: For eg,. "Aksdtkasl2910dks"
		public DkHttp SetDefaultAuthorization(string schema, string token) {
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(schema, token);
			return this;
		}

		/// Convenient method for sending GET request.
		public async Task<T> Get<T>(string url) where T : DkApiResponse {
			// Perform try/catch for whole process
			try {
				var result = await httpClient.GetAsync(url);
				var responseBody = await result.Content.ReadAsStringAsync();

				// To check with larger range: !result.IsSuccessStatusCode
				if (result.StatusCode != HttpStatusCode.OK) {
					if (DkBuildConfig.DEBUG) {
						DkLogs.Warning(this, $"NG responseBody when GET, reason: {result.ReasonPhrase}");
					}

					return DkObjects.NewInstace<T>().AlsoDk(res => {
						res.code = ((int)result.StatusCode);
						res.message = result.ReasonPhrase;
					});
				}

				// Add `!` to tell compiler that body and result are non-null.
				return DkJsons.Json2Obj<T>(responseBody!)!;
			}
			catch (Exception e) {
				if (DkBuildConfig.DEBUG) {
					DkLogs.Warning(this, $"Error when GET ! error: {e.Message}");
				}

				return DkObjects.NewInstace<T>().AlsoDk(res => {
					res.code = -1;
					res.message = e.Message;
				});
			}
		}

		/// Convenient method for sending POST request.
		/// @param `body`: Can be serialized with Json.
		public async Task<T> Post<T>(string url, object body) where T : DkApiResponse {
			// Perform try/catch for whole process
			try {
				var json = DkJsons.Obj2Json(body);
				// Other content types:
				// - StreamContent
				// - ByteArrayContent (StringContent, FormUrlEncodedContent)
				// - MultipartContent (MultipartFormDataContent)
				var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
				// Commented out since `PostAsJsonAsync()` only work in ASP.NET environment.
				// var response = await httpClient.PostAsJsonAsync(url, body);
				var response = await httpClient.PostAsync(url, stringContent);
				var responseBody = await response.Content.ReadAsStringAsync();

				// To check with larger range: !result.IsSuccessStatusCode
				if (response.StatusCode != HttpStatusCode.OK) {
					if (DkBuildConfig.DEBUG) {
						DkLogs.Warning(this, $"NG responseBody when POST, reason: {response.ReasonPhrase}");
					}

					return DkObjects.NewInstace<T>().AlsoDk(res => {
						res.code = ((int)response.StatusCode);
						res.message = response.ReasonPhrase;
					});
				}

				// Add `!` to tell compiler that body and result are non-null.
				return DkJsons.Json2Obj<T>(responseBody!)!;
			}
			catch (Exception e) {
				if (DkBuildConfig.DEBUG) { DkLogs.Warning(this, $"Error when POST ! error: {e.Message}"); }

				return DkObjects.NewInstace<T>().AlsoDk(res => {
					res.code = -1;
					res.message = e.Message;
				});
			}
		}

		/// This is detail implementation for sending request.
		/// Note that, `Get(), Post()` in this class are convenient versions of this method.
		private async Task<T> Send<T>(
			HttpMethod method, // HttpMethod.Get, HttpMethod.Post,...
			string url // https://kilobytes.com.vn
		) where T : DkApiResponse {
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
						res.code = ((int)response.StatusCode);
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
					res.code = -1;
					res.message = e.Message;
				});
			}
		}
	}
}
