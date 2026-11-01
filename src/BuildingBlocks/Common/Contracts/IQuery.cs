namespace RunbookForge.BuildingBlocks.Common.Contracts;

using MediatR;

public interface IQuery<out TResult> : IRequest<TResult>;

