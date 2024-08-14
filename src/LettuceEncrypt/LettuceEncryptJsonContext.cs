// Copyright (c) Fallenwood.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace LettuceEncrypt;

using System.Text.Json;

[JsonSerializable(typeof(AccountModel))]
public partial class LettuceEncryptJsonContext : JsonSerializerContext {

}