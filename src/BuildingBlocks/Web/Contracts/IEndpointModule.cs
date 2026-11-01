namespace RunbookForge.BuildingBlocks.Web.Contracts;

using Microsoft.AspNetCore.Routing;

public interface IEndpointModule
{
    void MapEndpoints(IEndpointRouteBuilder app);
}

