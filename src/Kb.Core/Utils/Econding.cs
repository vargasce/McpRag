using System.Security.Cryptography;
using System.Text;

namespace Kb.Core.Utils;

public sealed class EncodingUtil
{
  public static string EncodeSha256(string content)
  {
    return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));
  }
}
