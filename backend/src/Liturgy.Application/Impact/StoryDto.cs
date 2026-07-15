using Liturgy.Domain;

namespace Liturgy.Application.Impact;

public record StoryDto(int Week, string Text)
{
    public static StoryDto From(Story s) => new(s.Week, s.Text);
}
