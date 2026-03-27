using FluentAssertions;
using Xunit;
using RentalOS.Application.Common.Services;
using RentalOS.Domain.Enums;
using RentalOS.Domain.Entities;

namespace RentalOS.UnitTests.Services;

public class PlanServiceTests
{
    private readonly PlanService _sut = new();

    // ── Room limits ────────────────────────────────────────────────────
    [Theory]
    [InlineData(PlanType.Trial, 20)]
    [InlineData(PlanType.Starter, 20)]
    [InlineData(PlanType.Pro, 100)]
    [InlineData(PlanType.Business, int.MaxValue)]
    public void GetRoomLimit_ReturnsCorrectLimit(PlanType plan, int expected)
    {
        _sut.GetRoomLimit(plan).Should().Be(expected);
    }

    // ── Property limits ────────────────────────────────────────────────
    [Theory]
    [InlineData(PlanType.Trial, 1)]
    [InlineData(PlanType.Starter, 1)]
    [InlineData(PlanType.Pro, 3)]
    [InlineData(PlanType.Business, int.MaxValue)]
    public void GetPropertyLimit_ReturnsCorrectLimit(PlanType plan, int expected)
    {
        _sut.GetPropertyLimit(plan).Should().Be(expected);
    }

    // ── Staff limits ───────────────────────────────────────────────────
    [Theory]
    [InlineData(PlanType.Trial, 2)]
    [InlineData(PlanType.Starter, 2)]
    [InlineData(PlanType.Pro, 5)]
    [InlineData(PlanType.Business, int.MaxValue)]
    public void GetStaffLimit_ReturnsCorrectLimit(PlanType plan, int expected)
    {
        _sut.GetStaffLimit(plan).Should().Be(expected);
    }

    // ── Feature gating ─────────────────────────────────────────────────
    [Theory]
    [InlineData(PlanType.Trial, "ai", false)]
    [InlineData(PlanType.Starter, "ai", false)]
    [InlineData(PlanType.Pro, "ai", true)]
    [InlineData(PlanType.Business, "ai", true)]
    [InlineData(PlanType.Trial, "reports", false)]
    [InlineData(PlanType.Pro, "reports", true)]
    [InlineData(PlanType.Pro, "api", false)]
    [InlineData(PlanType.Business, "api", true)]
    [InlineData(PlanType.Trial, "unknown", true)]
    public void HasFeature_ReturnsCorrectGating(PlanType plan, string feature, bool expected)
    {
        _sut.HasFeature(plan, feature).Should().Be(expected);
    }

    // ── Trial expiry ───────────────────────────────────────────────────
    [Fact]
    public void IsTrialExpired_ReturnsFalse_WhenPlanIsNotTrial()
    {
        var tenant = new Tenant { Plan = PlanType.Pro, TrialEndsAt = DateTime.UtcNow.AddDays(-10) };
        _sut.IsTrialExpired(tenant).Should().BeFalse();
    }

    [Fact]
    public void IsTrialExpired_ReturnsTrue_WhenTrialDateInPast()
    {
        var tenant = new Tenant { Plan = PlanType.Trial, TrialEndsAt = DateTime.UtcNow.AddDays(-1) };
        _sut.IsTrialExpired(tenant).Should().BeTrue();
    }

    [Fact]
    public void IsTrialExpired_ReturnsFalse_WhenTrialDateInFuture()
    {
        var tenant = new Tenant { Plan = PlanType.Trial, TrialEndsAt = DateTime.UtcNow.AddDays(7) };
        _sut.IsTrialExpired(tenant).Should().BeFalse();
    }

    // ── Plan expiry ────────────────────────────────────────────────────
    [Fact]
    public void IsPlanExpired_ReturnsFalse_WhenPlanIsTrial()
    {
        var tenant = new Tenant { Plan = PlanType.Trial, PlanExpiresAt = DateTime.UtcNow.AddDays(-5) };
        _sut.IsPlanExpired(tenant).Should().BeFalse();
    }

    [Fact]
    public void IsPlanExpired_ReturnsTrue_WhenExpiredDateInPast()
    {
        var tenant = new Tenant { Plan = PlanType.Pro, PlanExpiresAt = DateTime.UtcNow.AddDays(-1) };
        _sut.IsPlanExpired(tenant).Should().BeTrue();
    }

    [Fact]
    public void IsPlanExpired_ReturnsFalse_WhenExpiredDateInFuture()
    {
        var tenant = new Tenant { Plan = PlanType.Pro, PlanExpiresAt = DateTime.UtcNow.AddDays(30) };
        _sut.IsPlanExpired(tenant).Should().BeFalse();
    }
}
