namespace RentalOS.Domain.Exceptions;

/// <summary>Thrown when a room cannot be rented because it is not in 'Available' status.</summary>
public class RoomNotAvailableException(Guid roomId, string currentStatus)
    : Exception($"Room '{roomId}' is not available for rent. Current status: {currentStatus}.")
{
    public Guid RoomId { get; } = roomId;
    public string CurrentStatus { get; } = currentStatus;
}
