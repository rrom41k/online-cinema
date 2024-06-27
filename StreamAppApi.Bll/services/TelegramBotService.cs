using StreamAppApi.Contracts.Models;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

using File = System.IO.File;

namespace StreamAppApi.Bll;

public class TelegramBotService
{
    private readonly ITelegramBotClient _botClient;
    private readonly string _chatId;

    public TelegramBotService(TelegramBotOptions options)
    {
        _botClient = new TelegramBotClient(options.Token);
        _chatId = options.ChatId;

        var me = _botClient.GetMeAsync().Result;
        Console.WriteLine($"Bot ID: {me.Id}, Bot Name: {me.Username}");
    }

    private static async Task<byte[]> DownloadImageAsync(string imageUrl)
    {
        using var httpClient = new HttpClient();
        return await httpClient.GetByteArrayAsync(imageUrl);
    }

    public async Task SendPostLink(string text, string imageUrl)
    {
        // Получаем изображение по URL
        var imageBytes = await DownloadImageAsync(imageUrl);
        if (imageBytes.Length == 0)
            throw new("The poster could not be retrieved");
        // Отправка фото
        using var photoStream = new MemoryStream(imageBytes);
        var photoInput = new InputFileStream(photoStream);
        var caption = new InputTextMessageContent(text);
        await _botClient.SendPhotoAsync(
            _chatId,
            photoInput,
            caption: text);
    }

    public async void SendPostLocal(string text, string photoPath)
    {
        // Отправка фото
        await using var photoStream = File.OpenRead(photoPath);
        var photoInput = new InputFileStream(photoStream);
        var caption = new InputTextMessageContent(text);
        await _botClient.SendPhotoAsync(
            _chatId,
            photoInput,
            caption: text);
    }
}