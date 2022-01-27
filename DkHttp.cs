using Tool.Compet.Json;

namespace Tool.Compet.Http {
	public class DkHttp {
		public async Task<T> GetAsync<T>() where T : TheApiResponse {
			try {
				using var httpClient = new HttpClient();
				var result = await httpClient.GetAsync("");

				// var responseBody = await httpClient.GetStringAsync("");
				// var response = DkJsons.Json2Obj<T>(responseBody);

				var responseBody = await result.Content.ReadAsStringAsync();

				if (!result.IsSuccessStatusCode) {
					return new TheApiResponse {
						code = result.StatusCode,
						message = responseBody
					};
				}

				var response = DkJsons.Json2Obj<T>(responseBody).AlsoDk(res => {
					// res.
				});

				return response;
			}
			catch (Exception e) {
				return null;
			}
		}
	}
}
