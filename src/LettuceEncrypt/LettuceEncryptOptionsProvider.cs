// Copyright (c) Fallenwood.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace LettuceEncrypt;

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

/// <summary>
///
/// </summary>
public interface ILettuceEncryptOptionsProvider : IConfigurationProvider {
  /// <summary>
  ///
  /// </summary>
  /// <param name="addresses"></param>
  public void SetAddresses(string[] addresses);

  /// <inheritdoc />
  public IChangeToken ChangeToken { get; }
}

internal sealed class LettuceEncryptOptionsProvider : ConfigurationProvider, ILettuceEncryptOptionsProvider {
  private readonly CancellationTokenSource _configurationChangeTokenSource = new();

  ///<inheritdoc />
  public IChangeToken ChangeToken => new CancellationChangeToken(_configurationChangeTokenSource.Token);

  ///<inheritdoc />
  public void SetAddresses(string[] addresses) {
    this.Data = new Dictionary<string, string?>();
    for (var i = 0; i < addresses.Length; i++) {
      this.Data[$"LettuceEncrypt:DomainNames:{i}"] = addresses[i];
    }

    this.OnReload();
  }

  public override bool TryGet(string key, out string value) {
    value = string.Empty;
    if (base.TryGet(key, out var nullableValue)) {
      if (nullableValue != null) {
        value = nullableValue;
        return true;
      }
    }

    return false;
  }
}
