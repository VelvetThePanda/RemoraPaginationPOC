using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Interactivity;
using Remora.Discord.Caching.Services;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace RemoraPaginationSystem;

public class PaginationHandler : InteractionGroup
{
    private readonly CacheService _cache;
    private readonly InteractionContext _context;
    private readonly IDiscordRestChannelAPI _channels;
    private readonly IDiscordRestInteractionAPI _interactions;
    
    public PaginationHandler
    (
        CacheService cache,
        InteractionContext context,
        IDiscordRestChannelAPI channels,
        IDiscordRestInteractionAPI interactions
    )
    {
        _cache = cache;
        _context = context;
        _channels = channels;
        _interactions = interactions;
    }

    [Button("first")]
    public ValueTask<Result> FirstAsync() => UpdateAsync(d => d.MoveFirst());

    [Button("previous")]
    public ValueTask<Result> PreviousAsync() => UpdateAsync(d => d.MovePrevious());
    
    [Button("next")]
    public ValueTask<Result> NextAsync() => UpdateAsync(d => d.MoveNext());
    
    [Button("last")]
    public ValueTask<Result> LastAsync() => UpdateAsync(d => d.MoveLast());

    public async Task<Result> CloseAsync()
    {
        if (!_context.Message.IsDefined(out var message))
        {
            return new InvalidOperationError("Interation data did not contain a message.");
        }

        var data = await _cache.EvictAsync<ButtonPaginatedMessageData>(message.ID.ToString());
        
        if (!data.IsDefined(out var paginatedMessageData))
        {
            return new InvalidOperationError("Interation data did not contain a paginated message.");
        }

        if (paginatedMessageData.UseInteractions)
        {
            return await _interactions.DeleteOriginalInteractionResponseAsync
            (
                _context.ApplicationID,
                _context.Token,
                this.CancellationToken
            );
        }

        return await _channels.DeleteMessageAsync(message.ChannelID, message.ID);
    }
    
    private async ValueTask<Result> UpdateAsync(Func<ButtonPaginatedMessageData, bool> update)
    {
        if (!_context.Message.IsDefined(out var message))
        {
            return new InvalidOperationError("Interation data did not contain a message.");
        }

        var data = await _cache.TryGetValueAsync<ButtonPaginatedMessageData>(message.ID.ToString());

        if (!data.IsDefined(out var paginationData))
        {
            return new NotFoundError("Pagination was invoked, but no backing data exists for the message.");
        }
        
        var updated = update(paginationData);

        if (!updated)
        {
            return Result.FromSuccess(); // Don't bother with a request if nothing changed.
        }

            await _cache.CacheAsync(message.ID.ToString(), paginationData);

        var page = paginationData.GetCurrentPage();
        var components = paginationData.GetCurrentComponents();
        
        if (paginationData.UseInteractions)
        {
            return (Result)await _interactions.EditOriginalInteractionResponseAsync
            (
                _context.ApplicationID,
                _context.Token,
                embeds: new[] { page },
                components: new(message.Components.IsDefined(out var existing) ? existing.Concat(components).ToArray() : components),
                ct: this.CancellationToken
            );
        }
        else
        {
            return (Result)await _channels.EditMessageAsync
            (
                message.ChannelID,
                message.ID,
                embeds: new[] { page },
                components: new(message.Components.IsDefined(out var existing) ? existing.Concat(components).ToArray() : components),
                ct: this.CancellationToken
            );
        }
    }
}