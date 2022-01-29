namespace Tool.Compet.Http {
	/// Api status code in response body.
	public class ApiCode {
		public const int UNKNOWN = -1;
		public const int OK = 200;
		public const int BAD_REQUEST = 400;
		public const int UNAUTHORIZED = 401;
		public const int NOT_FOUND = 404;
		public const int INTERNAL_SERVER_ERROR = 500;
		public const int MAINTENANCE = 503;
	}

	/// Api response body.
	/// We need add to each field to include them in response.
	/// Otherwise that field will be auto excluded from response by ASP.NET Core framework.
	public class TheApiResponse {
		/// Status code, for eg,. 200, 201, 400, 401, 404, 500, 501,...
		public virtual int code { get; set; }

		/// Detail message for both success or failure cases.
		public virtual string? message { get; set; }
	}
}
