using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;

namespace RemoraPaginationSystem;

public class PaginationOptions
{
    public ButtonComponent? First { get; set; } = new(ButtonComponentStyle.Primary, Emoji: new PartialEmoji(Name: "⏪"));
    public ButtonComponent? Previous { get; set; } = new(ButtonComponentStyle.Primary, Emoji: new PartialEmoji(Name: "◀"));
    public ButtonComponent? Stop { get; set; } = new(ButtonComponentStyle.Primary, Emoji: new PartialEmoji(Name: "⏹"));
    public ButtonComponent? Next { get; set; } = new(ButtonComponentStyle.Primary, Emoji: new PartialEmoji(Name: "▶"));
    public ButtonComponent? Last { get; set; } = new(ButtonComponentStyle.Primary, Emoji: new PartialEmoji(Name: "⏩"));
    
    /// <summary>
    /// Whether or not skip buttons should wrap around to the first/last page.
    /// </summary>
    public bool SkipButtonWraps { get; set; } = false;
}