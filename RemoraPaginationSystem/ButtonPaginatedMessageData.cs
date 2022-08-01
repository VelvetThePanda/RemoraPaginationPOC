using System.Text.Json.Serialization;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Interactivity;
using Remora.Rest.Core;

namespace RemoraPaginationSystem;

internal sealed class ButtonPaginatedMessageData
{
    public short Page { get; private set; } // Do you really need more than 65535 pages?
    
    public bool UseInteractions { get; init; }
    public Snowflake SourceUserID { get; init; }
    public PaginationOptions Options { get; init; }
    public  IReadOnlyList<Embed> Pages { get; init; }

    public bool MoveFirst()
    {
        if (Options.SkipButtonWraps)
        {
            Page = (short)(Page > 0 ? 0 : Pages.Count - 1);
            return true;
        }
        else
        {
            if (Page is 0)
                return false;

            Page = 0;
            return true;
        }
    }

    public bool MoveLast()
    {
        if (Options.SkipButtonWraps)
        {
            Page = (short)(Pages.Count - 1);
            return true;
        }
        else
        {
            if (Page <= Pages.Count - 1)
                return false;
            
            Page = (short)(Pages.Count - 1);
            return true;
        }
    }
    
    public bool MoveNext() => (Page = (short)Math.Min(Page + 1, Pages.Count - 1)) < Pages.Count;
    
    public bool MovePrevious() => (Page = (short)Math.Max(Page - 1, 0)) >= 0;

    public Embed GetCurrentPage() => Pages[Page];
    
    public IReadOnlyList<IMessageComponent> GetCurrentComponents()
    {
        var components = new List<IMessageComponent>();
        
        if (Options.First is not null)
            components.Add(Options.First with
            {
                CustomID = CustomIDHelpers.CreateButtonID("first"),
                IsDisabled = !Options.SkipButtonWraps && Page is 0
            });
        
        if (Options.Previous is not null)
            components.Add(Options.Previous with
            {
                CustomID = CustomIDHelpers.CreateButtonID("previous"),
                IsDisabled = !Options.SkipButtonWraps && Page is 0
            });
        
        if (Options.Stop is not null)
            components.Add(Options.Stop with { CustomID = CustomIDHelpers.CreateButtonID("stop") });
        
        if (Options.Next is not null)
            components.Add(Options.Next with
            {
                CustomID = CustomIDHelpers.CreateButtonID("next"),
                IsDisabled = !Options.SkipButtonWraps && Page >= Pages.Count - 1
            });
        
        if (Options.Last is not null)
            components.Add(Options.Last with
            {
                CustomID = CustomIDHelpers.CreateButtonID("last"),
                IsDisabled = !Options.SkipButtonWraps && Page >= Pages.Count - 1
            });
        
        // TODO: Help button? Would be on-par with Remora.
        
        return new[] { new ActionRowComponent(components) };
    }
}