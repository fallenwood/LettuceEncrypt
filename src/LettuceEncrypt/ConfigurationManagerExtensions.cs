// Copyright (c) Fallenwood.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace LettuceEncrypt;

using Microsoft.Extensions.Configuration;

/// <summary>
/// 
/// </summary>
public static class ConfigurationManagerExtensions {
  /// <summary>
  /// 
  /// </summary>
  /// <param name="manager"></param>
  /// <param name="provider"></param>
  /// <returns></returns>
  public static ConfigurationManager AddLettuceEncryptOptionsProvider(
      this ConfigurationManager manager, ILettuceEncryptOptionsProvider provider) {
    IConfigurationBuilder configBuilder = manager;
    configBuilder.Add(new LettuceEncryptOptionsConfigurationSource(provider));

    return manager;
  }
}
