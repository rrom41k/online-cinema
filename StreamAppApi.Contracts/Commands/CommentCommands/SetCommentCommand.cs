namespace StreamAppApi.Contracts.Commands.CommentCommands;

public record SetCommentCommand(string videoId, string value);