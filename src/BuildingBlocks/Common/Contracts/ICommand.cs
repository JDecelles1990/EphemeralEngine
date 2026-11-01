namespace RunbookForge.BuildingBlocks.Common.Contracts;

using MediatR;

public interface ICommand<out TResult> : IRequest<TResult>;

public interface ICommand : IRequest;

