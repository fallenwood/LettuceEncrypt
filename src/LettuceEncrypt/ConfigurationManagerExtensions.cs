// Copyright (c) Fallenwood.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace LettuceEncrypt;

using Microsoft.Extensions.Configuration;

internal static class ConfigurationManagerExtensions {
  internal static ConfigurationManager AddLettuceEncryptOptionsProvider(
      this ConfigurationManager manager, ILettuceEncryptOptionsProvider provider) {
    IConfigurationBuilder configBuilder = manager;
    configBuilder.Add(new LettuceEncryptOptionsConfigurationSource(provider));

    return manager;
  }
}
