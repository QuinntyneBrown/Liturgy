using Liturgy.Domain;

namespace Liturgy.Application.Impact;

public record GratitudeDto(string Quote, string Attribution)
{
    public static GratitudeDto From(Gratitude g) => new(g.Quote, g.Attribution);
}
