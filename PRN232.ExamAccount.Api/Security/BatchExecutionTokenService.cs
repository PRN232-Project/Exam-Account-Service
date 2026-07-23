using System.Security.Cryptography;
using System.Text;

namespace PRN232.ExamAccount.Api.Security;

public static class BatchExecutionTokenService
{
    public static string Create() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
    public static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
