using StreamAppApi.Contracts.Commands.CommentCommands;
using StreamAppApi.Contracts.Dto;

namespace StreamAppApi.Contracts.Interfaces;

public interface ICommentService
{
    Task<CommentDto> SetComment(string userId, SetCommentCommand setCommentCommand, CancellationToken cancellationToken);
    Task<List<VideoCommentDto>> GetVideoComments(string videoId, CancellationToken cancellationToken);
}