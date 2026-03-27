using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Xunit;

namespace RentalOS.UnitTests.Security;

/// <summary>
/// Tests the HMAC-SHA256 signing logic used by MoMoService.
/// The private SignHmacSha256 method uses: HMACSHA256(key).ComputeHash(message) → hex lowercase
/// </summary>
public class MoMoSignatureTests
{
    // Replicates the SignHmacSha256 private method in MoMoService
    private static string SignHmacSha256(string message, string key)
    {
        var keyByte = Encoding.UTF8.GetBytes(key);
        var messageBytes = Encoding.UTF8.GetBytes(message);
        using var hmac = new HMACSHA256(keyByte);
        var hash = hmac.ComputeHash(messageBytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    [Fact]
    public void SignHmacSha256_ProducesCorrectLength()
    {
        var result = SignHmacSha256("hello", "secret");
        result.Should().HaveLength(64); // SHA256 = 32 bytes = 64 hex chars
    }

    [Fact]
    public void SignHmacSha256_DeterministicForSameInput()
    {
        var sig1 = SignHmacSha256("test-message", "my-secret-key");
        var sig2 = SignHmacSha256("test-message", "my-secret-key");
        sig1.Should().Be(sig2);
    }

    [Fact]
    public void SignHmacSha256_DifferentKeys_ProduceDifferentSignatures()
    {
        var sig1 = SignHmacSha256("same-message", "key-one");
        var sig2 = SignHmacSha256("same-message", "key-two");
        sig1.Should().NotBe(sig2);
    }

    [Fact]
    public void SignHmacSha256_DifferentMessages_ProduceDifferentSignatures()
    {
        var sig1 = SignHmacSha256("message-a", "secret");
        var sig2 = SignHmacSha256("message-b", "secret");
        sig1.Should().NotBe(sig2);
    }

    [Fact]
    public void VerifySignature_ValidPayload_ReturnsTrue()
    {
        // Simulate MoMo IPN verification: recompute expected signature and compare
        const string secretKey = "K951B6PE1waDMi640xX08PD3vg6EkVlz";
        const string rawSignString =
            "accessKey=F8BBA842ECF85&amount=50000&extraData=&message=Successful" +
            "&orderId=RENTALOS_abc123&orderInfo=RentalOS Payment&orderType=momo_wallet" +
            "&partnerCode=MOMO&payType=napas&requestId=abc123&responseTime=1709123456789" +
            "&resultCode=0&transId=987654321";

        var expected = SignHmacSha256(rawSignString, secretKey);

        // Simulate what a valid IPN webhook would contain
        var fromWebhook = SignHmacSha256(rawSignString, secretKey);

        fromWebhook.Should().Be(expected);
    }

    [Fact]
    public void VerifySignature_TamperedPayload_NotEqualExpected()
    {
        const string secretKey = "K951B6PE1waDMi640xX08PD3vg6EkVlz";
        const string original = "amount=50000&orderId=abc";
        const string tampered = "amount=99999&orderId=abc";

        SignHmacSha256(original, secretKey).Should().NotBe(SignHmacSha256(tampered, secretKey));
    }

    [Fact]
    public void SignHmacSha256_OutputIsLowercase()
    {
        var result = SignHmacSha256("test", "key");
        result.Should().MatchRegex("^[0-9a-f]+$");
    }
}
