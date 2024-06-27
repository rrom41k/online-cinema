using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using StreamAppApi.Bll.DbConfiguration;
using StreamAppApi.Contracts.Commands.CommentCommands;
using StreamAppApi.Contracts.Dto;
using StreamAppApi.Contracts.Interfaces;
using StreamAppApi.Contracts.Models;

namespace StreamAppApi.Bll;

public class CommentService : ICommentService
{
    private readonly StreamPlatformDbContext _dbContext;

    public CommentService(StreamPlatformDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
    }

    public async Task<CommentDto> SetComment(
        string userId,
        SetCommentCommand setCommentCommand,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) // Проверка на отмену запроса
        {
            throw new OperationCanceledException();
        }

        var comment = await _dbContext.Comments
            .FirstOrDefaultAsync(
                comment =>
                    comment.VideoId == setCommentCommand.videoId && comment.UserId == userId,
                cancellationToken);
        
        if (!setCommentCommand.value.IsNullOrEmpty())
        {
            if (comment != null)
            {
                comment.Value = setCommentCommand.value;
            }
            else
            {
                comment = new() { UserId = userId, VideoId = setCommentCommand.videoId };
                
                _dbContext.Comments.Add(comment);
            }
        }
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CommentToCommentDto(comment);
    }

    public async Task<List<VideoCommentDto>> GetVideoComments(
        string videoId,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) // Проверка на отмену запроса
        {
            throw new OperationCanceledException();
        }
        
        var comments = await _dbContext.Comments
            .Include(comment => comment.User)
            .Where(comments => comments.VideoId == videoId)
            .ToListAsync(cancellationToken);

        return comments.Select(comment => new VideoCommentDto(comment.User.Login, comment.Value)).ToList();
    }

    private CommentDto CommentToCommentDto(Comment comment)
    {
        return new(comment.UserId, comment.VideoId, comment.Value);
    }
    
    public static List<VideoCommentDto> MapCommentsToVideoCommentDtos(ICollection<Comment> comments)
    {
        List<VideoCommentDto> result = new();
        
        foreach (var comment in comments)
            result.Add(new(comment.User.Login, comment.Value));
        
        return result;
    }
}