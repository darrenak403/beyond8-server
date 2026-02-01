using Beyond8.Common.Clients;
using Microsoft.AspNetCore.Http;

namespace Beyond8.Assessment.Application.Clients.Catalog
{
    public class CatalogService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : BaseClient(httpClient, httpContextAccessor), ICatalogService
    {

    }
}