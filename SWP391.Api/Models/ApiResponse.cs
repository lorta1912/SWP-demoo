namespace SWP391.Api.Models
{
    public class ApiResponse<T>
    {
        public int Code { get; set; }
        public string Message { get; set; } = null!;
        public T? Obj { get; set; }

        public ApiResponse(int code, string message, T? obj = default)
        {
            Code = code;
            Message = message;
            Obj = obj;
        }

        public static ApiResponse<T> Success(T obj, string message = "Success") =>
            new ApiResponse<T>(200, message, obj);

        public static ApiResponse<T> Error(int code, string message) =>
            new ApiResponse<T>(code, message, default);
    }
}
