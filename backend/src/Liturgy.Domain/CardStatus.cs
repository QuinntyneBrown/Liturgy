namespace Liturgy.Domain;

/// <summary>
/// Lifecycle status of a <see cref="Card"/>, independent of its <see cref="BoardColumn"/>
/// position. An Open card lives on the active board; Closed (resolved/shelved) and
/// Cancelled (abandoned) cards are terminal states that leave the active board without
/// touching the 5R Done gate.
/// </summary>
public enum CardStatus
{
    Open = 0,
    Closed = 1,
    Cancelled = 2
}
