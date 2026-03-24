namespace RentalOS.Application.Common.Constants;

public static class Amenities
{
    public static readonly IReadOnlyList<string> ValidList = new List<string>
    {
        "wifi", "ac", "water_heater", "fridge", "washing_machine", "private_wc",
        "balcony", "window", "parking_motorbike", "parking_car", "elevator", "security",
        "internet", "cable_tv", "kitchen"
    };

    public static bool IsValid(string amenity) => ValidList.Contains(amenity);
}
