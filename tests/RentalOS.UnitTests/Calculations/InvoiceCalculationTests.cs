using FluentAssertions;
using Xunit;

namespace RentalOS.UnitTests.Calculations;

/// <summary>
/// Tests the invoice total calculation formula used in UpdateMeterReadingCommandHandler:
///   TotalAmount = RoomRent + Electricity + Water + ServiceFee + InternetFee + GarbageFee + OtherFees - Discount
/// </summary>
public class InvoiceCalculationTests
{
    // Mirrors the logic in UpdateMeterReadingCommandHandler
    private static decimal CalculateTotal(
        decimal roomRent,
        decimal electricityOld, decimal electricityNew, decimal electricityPrice,
        decimal waterOld, decimal waterNew, decimal waterPrice,
        decimal serviceFee = 0, decimal internetFee = 0, decimal garbageFee = 0,
        decimal otherFees = 0, decimal discount = 0)
    {
        var electricityAmount = (electricityNew - electricityOld) * electricityPrice;
        var waterAmount = (waterNew - waterOld) * waterPrice;
        return roomRent + electricityAmount + waterAmount + serviceFee + internetFee + garbageFee + otherFees - discount;
    }

    [Fact]
    public void Calculate_BasicInvoice_ReturnsCorrectTotal()
    {
        // Rent 3M + Electricity (100 units * 3500) + Water (10 * 15000)
        var total = CalculateTotal(
            roomRent: 3_000_000,
            electricityOld: 100, electricityNew: 200, electricityPrice: 3_500,
            waterOld: 10, waterNew: 20, waterPrice: 15_000);

        total.Should().Be(3_000_000 + 100 * 3_500 + 10 * 15_000); // 3,650,000
    }

    [Fact]
    public void Calculate_InvoiceWithFees_IncludesAllFees()
    {
        var total = CalculateTotal(
            roomRent: 2_500_000,
            electricityOld: 50, electricityNew: 100, electricityPrice: 3_000,
            waterOld: 0, waterNew: 5, waterPrice: 10_000,
            serviceFee: 50_000,
            internetFee: 100_000,
            garbageFee: 20_000,
            otherFees: 0,
            discount: 0);

        total.Should().Be(2_500_000 + 150_000 + 50_000 + 50_000 + 100_000 + 20_000);
    }

    [Fact]
    public void Calculate_InvoiceWithDiscount_SubtractsCorrectly()
    {
        var total = CalculateTotal(
            roomRent: 2_000_000,
            electricityOld: 0, electricityNew: 0, electricityPrice: 3_500,
            waterOld: 0, waterNew: 0, waterPrice: 10_000,
            discount: 100_000);

        total.Should().Be(1_900_000);
    }

    [Fact]
    public void Calculate_ZeroUsage_ReturnOnlyRent()
    {
        var total = CalculateTotal(
            roomRent: 1_500_000,
            electricityOld: 200, electricityNew: 200, electricityPrice: 3_500,
            waterOld: 5, waterNew: 5, waterPrice: 10_000);

        total.Should().Be(1_500_000);
    }

    [Fact]
    public void Calculate_ElectricityAmount_IsBasedOnConsumptionDelta()
    {
        var electricityAmount = (150m - 100m) * 3_800m;
        electricityAmount.Should().Be(190_000m);
    }
}
