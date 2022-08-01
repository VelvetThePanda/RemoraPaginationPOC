using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Caching.Services;
using Remora.Rest.Core;
using Remora.Results;

namespace RemoraPaginationSystem;

public static class PaginationExtension
{
    public static async Task<Result<IMessage>> PaginateAsync
    (
        IDiscordRestChannelAPI channels,
        CacheService cache,
        Snowflake channel,
        Snowflake user,
        IReadOnlyList<Embed> embeds,
        PaginationOptions? appearance = null,
        CancellationToken ct = default
    )
    {
        if (embeds.Count is 0)
        {
            throw new ArgumentOutOfRangeException("At least one page must be provided."); // Some would return an error here, but this could be 
                                                                                          // classified as developer error, so.
        }

        appearance ??= new();

        var data = new ButtonPaginatedMessageData()
        {
            Pages = embeds,
            Options = appearance,
            SourceUserID = user,
            UseInteractions = false
        };

        var send = await channels.CreateMessageAsync(channel, embeds: new[] { (IEmbed)embeds[0] }, ct: ct);

        if (!send.IsSuccess)
        {
            return send;
        }

        var message = send.Entity;

        await cache.CacheAsync(message.ID.ToString(), data);
        
        return send;
    }
}