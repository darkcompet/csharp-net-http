namespace Tool.Compet.Http {
	/// Api response body.
	/// We need add to each field to include them in response.
	/// Otherwise that field will be auto excluded from response by ASP.NET Core framework.
	public class TheApiResponse {
		/// Status code, for eg,. 200, 201, 400, 401, 404, 500, 501,...
		public virtual int code { get; set; }

		/// Code of error if failed, for eg,. "player_name_duplicated", "not_enough_gem",...
		public virtual string? errCode { get; set; }

		/// Detail message for both success or failure cases.
		public virtual string? message { get; set; }
	}
}
