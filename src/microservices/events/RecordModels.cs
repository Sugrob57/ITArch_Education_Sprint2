namespace KafkaEventsApi
{
	// --- Модели событий ---
	record Event(string Id, string Type, DateTime Timestamp, object Payload);

	record MovieEvent(int MovieId, string Title, string Action, int? UserId = null,
					   double? Rating = null, string[]? Genres = null, string? Description = null);

	record UserEvent(int UserId, string Action, DateTime Timestamp, string? Username = null, string? Email = null);

	record PaymentEvent(int PaymentId, int UserId, double Amount, string Status, DateTime Timestamp, string? MethodType = null);

	record EventResponse(string Status, int Partition, long Offset, Event Event);
}