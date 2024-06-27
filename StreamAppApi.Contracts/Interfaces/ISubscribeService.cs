using StreamAppApi.Contracts.Commands.SubscribeCommands;
using StreamAppApi.Contracts.Dto;

namespace StreamAppApi.Contracts.Interfaces;

public interface ISubscribeService
{
    Task<SubscribeDto> GetSubscribeById(string id, CancellationToken cancellationToken);
    Task<List<SubscribeDto>> GetAllSubscribes(CancellationToken cancellationToken);
    Task<OrderDto> BuySubscribeById(string userId, string subscribeId, CancellationToken cancellationToken);
    
    /* Admin Rights */
    Task<SubscribeDto> CreateSubscribe(SubscribeCreateCommand subscribeCreateCommand, CancellationToken cancellationToken);
    Task<SubscribeDto> UpdateSubscribe(string id, SubscribeUpdateCommand subscribeUpdateCommand, CancellationToken cancellationToken);
    Task DeleteSubscribe(string id, CancellationToken cancellationToken);
}