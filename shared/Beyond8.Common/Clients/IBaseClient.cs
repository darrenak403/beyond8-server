namespace Beyond8.Common.Clients;

public interface IBaseClient
{
    Task<TResponse> GetAsync<TResponse>(string url);
    Task<TResponse> PostAsync<TResponse>(string url, object body);
    Task<TResponse> PutAsync<TResponse>(string url, object body);
    Task<TResponse> DeleteAsync<TResponse>(string url);
    Task<TResponse> PatchAsync<TResponse>(string url, object body);
}
