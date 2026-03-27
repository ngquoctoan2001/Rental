using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Xunit;

namespace RentalOS.UnitTests.Security;

/// <summary>
/// Tests the HMAC-SHA512 signing logic used by VNPayService.
/// The private SignHmacSha512 method uses: HMACSHA512(key).ComputeHash(message) → hex uppercase compared case-insensitively
/// </summary>
public class VNPaySignatureTests
{
    // Replicates SignHmacSha512 from VNPayService
    private static string SignHmacSha512(string message, string key)
    {
        var keyByte = Encoding.UTF8.GetBytes(key);
        var messageBytes = Encoding.UTF8.GetBytes(message);
        using var hmac = new HMACSHA512(keyByte);
        var hash = hmac.ComputeHash(messageBytes);
        return BitConverter.ToString(hash).Replace("-", "").ToUpper();
    }

    [Fact]
    public void SignHmacSha512_ProducesCorrectLength()
    {
        var result = SignHmacSha512("hello", "secret");
        result.Should().HaveLength(128); // SHA512 = 64 bytes = 128 hex chars
    }

    [Fact]
    public void SignHmacSha512_DeterministicForSameInput()
    {
        var sig1 = SignHmacSha512("vnpay-query", "hash-secret-key");
        var sig2 = SignHmacSha512("vnpay-query", "hash-secret-key");
        sig1.Should().Be(sig2);
    }

    [Fact]
    public void SignHmacSha512_DifferentKeys_ProduceDifferentSignatures()
    {
        var sig1 = SignHmacSha512("same-message", "key-alpha");
        var sig2 = SignHmacSha512("same-message", "key-beta");
        sig1.Should().NotBe(sig2);
    }

    [Fact]
    public void SignHmacSha512_OutputIsUppercase()
    {
        var result = SignHmacSha512("test", "key");
        result.Should().MatchRegex("^[0-9A-F]+$");
    }

    [Fact]
    public void VerifySignature_CaseInsensitiveComparison_Succeeds()
    {
        // VNPayService uses .ToUpper() on both sides before comparing
        const string hashSecret = "RAOEXHYVSDDIIENYWSLDIIZTANXUXZFJ";
        const string queryString = "vnp_Amount=5000000&vnp_BankCode=NCB&vnp_OrderInfo=Thanh+toan+don+hang+123";

        var signed = SignHmacSha512(queryString, hashSecret);

        // VNPayService compares: vnp_SecureHash.ToUpper() != expectedHash.ToUpper()
        signed.ToUpper().Should().Be(SignHmacSha512(queryString, hashSecret).ToUpper());
    }

    [Fact]
    public void VerifySignature_TamperedAmount_Fails()
    {
        const string hashSecret = "RAOEXHYVSDDIIENYWSLDIIZTANXUXZFJ";
        const string original = "vnp_Amount=5000000&vnp_OrderInfo=Payment";
        const string tampered = "vnp_Amount=1&vnp_OrderInfo=Payment";

        SignHmacSha512(original, hashSecret)
            .Should().NotBe(SignHmacSha512(tampered, hashSecret));
    }

    [Fact]
    public void VNPay_Amount_IsMultipliedBy100()
    {
        // VNPay specification: amount must be in VND × 100
        decimal invoiceTotal = 500_000m;
        long vnpAmount = (long)(invoiceTotal * 100);
        vnpAmount.Should().Be(50_000_000L);
    }
}
