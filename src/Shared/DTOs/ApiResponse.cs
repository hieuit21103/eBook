namespace Shared.DTOs;

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }

    public static ApiResponse SuccessResponse(string message = "Success")
    {
        return new ApiResponse { Success = true, Message = message };
    }

    public static ApiResponse Fail(string message, List<string>? errors = null)
    {
        return new ApiResponse 
        { 
            Success = false, 
            Message = message, 
            Errors = errors 
        };
    }
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success")
    {
        return new ApiResponse<T> 
        { 
            Success = true, 
            Message = message, 
            Data = data 
        };
    }
}
