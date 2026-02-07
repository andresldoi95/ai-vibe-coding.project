namespace SaaS.Application.Common.Models;

/// <summary>
/// Generic API response wrapper
/// </summary>
public class ApiResponse<T>
{
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }

    public ApiResponse(T data, string message = "")
    {
        Data = data;
        Message = message;
        Success = true;
    }

    public ApiResponse(string message, bool success = false)
    {
        Message = message;
        Success = success;
    }
}
