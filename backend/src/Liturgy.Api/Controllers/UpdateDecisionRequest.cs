using Liturgy.Domain;

namespace Liturgy.Api.Controllers;

public record UpdateDecisionRequest(DiscernmentPath ChosenPath, string Rationale, string PrayedOverWith);
