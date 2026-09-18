namespace DeliverySystem.API.Models;

public sealed record CollectionResponse(
    long CollectionId,
long OrderId,
int CourierId,
Guid IdempotencyKey,
decimal Amount,
DateTime CollectedAt,
string RecordedBy
);
