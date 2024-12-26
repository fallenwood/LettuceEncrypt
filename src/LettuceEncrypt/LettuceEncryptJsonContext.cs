namespace LettuceEncrypt;

using System.Text.Json.Serialization;
using LettuceEncrypt.Accounts;

[JsonSerializable(typeof(AccountModel))]
public partial class LettuceEncryptJsonContext : JsonSerializerContext {
}
