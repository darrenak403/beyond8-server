namespace Beyond8.Common.Utilities;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public object? Metadata { get; set; }

    public static ApiResponse<T> SuccessResponse(
        T data,
        string message = "Success",
        object? metadata = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Message = message,
            Data = data,
            Metadata = metadata
        };
    }

    public static ApiResponse<T> FailureResponse(
        string message,
        object? metadata = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Message = message,
            Data = default,
            Metadata = metadata
        };
    }

    public static ApiResponse<List<T>> SuccessPagedResponse<T>(
        List<T> items,
        int totalItems,
        int pageNumber,
        int pageSize,
        string message = "Success")
    {
        var pagingMetadata = new PagingMetadata(totalItems, pageNumber, pageSize);

        return new ApiResponse<List<T>>
        {
            IsSuccess = true,
            Message = message,
            Data = items,
            Metadata = pagingMetadata
        };
    }
}