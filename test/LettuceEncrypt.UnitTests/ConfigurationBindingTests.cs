// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace LettuceEncrypt.Tests;

public class ConfigurationBindingTests {
  [Fact]
  public void ItBindsToConfig() {
    var options = ParseOptions(new() {
      ["LettuceEncrypt:AcceptTermsOfService"] = "true",
      ["LettuceEncrypt:DomainNames:0"] = "one.com",
      ["LettuceEncrypt:DomainNames:1"] = "two.com",
      ["LettuceEncrypt:AllowedChallengeTypes"] = "Http01",
    });

    Assert.True(options.AcceptTermsOfService);
    Assert.Collection(options.DomainNames,
        one => Assert.Equal("one.com", one),
        two => Assert.Equal("two.com", two));
    Assert.Equal(Acme.ChallengeType.Http01, options.AllowedChallengeTypes);
  }

  [Fact]
  public void InMemoryProviderOptionsWin() {
    var data = new Dictionary<string, string> {
      ["LettuceEncrypt:EmailAddress"] = "config",
    };
    var config = new ConfigurationBuilder()
        .AddInMemoryCollection(data)
        .Build();

    var services = new ServiceCollection()
        .AddSingleton<IConfiguration>(config)
        .AddLettuceEncrypt(o => { o.EmailAddress = "code"; }, config)
        .Services
        .BuildServiceProvider(true);

    var options = services.GetRequiredService<IOptions<LettuceEncryptOptions>>();

    Assert.Equal("config", options.Value.EmailAddress);
  }

  [Fact]
  public void CustomProviderOptionsWin() {

    var config = new ConfigurationBuilder()
        .Build();

    var provider = new LettuceEncryptOptionsProvider();

    provider.SetAddresses(new[] { "one.com", "two.com" });

    var services = new ServiceCollection()
        .AddSingleton<ILettuceEncryptOptionsProvider>(provider)
        .AddLettuceEncrypt(o => { })
        .Services
        .BuildServiceProvider(true);

    var options = services.GetRequiredService<IOptions<LettuceEncryptOptions>>();

    Assert.Equal(2, options.Value.DomainNames.Length);
  }

  [Theory]
  [InlineData("http01", Acme.ChallengeType.Http01)]
  [InlineData("HTTP01", Acme.ChallengeType.Http01)]
  [InlineData("Any", Acme.ChallengeType.Any)]
  [InlineData("TlsAlpn01, http01", Acme.ChallengeType.TlsAlpn01 | Acme.ChallengeType.Http01)]
  public void ItParsesEnumValuesForChallengeType(string value, Acme.ChallengeType challengeType) {
    var options = ParseOptions(new() {
      ["LettuceEncrypt:AllowedChallengeTypes"] = value,
    });

    Assert.Equal(challengeType, options.AllowedChallengeTypes);
  }

  [Fact]
  public void DoesNotSupportWildcardDomains() {
    Assert.Throws<OptionsValidationException>(() =>
        ParseOptions(new() {
          ["LettuceEncrypt:DomainNames:0"] = "*.natemcmaster.com",
        }));
  }

  [Fact]
  public void CanSetAdditionalIssuers() {
    var options = ParseOptions(new() {
      ["LettuceEncrypt:AdditionalIssuers:0"] = "-----BEGIN CERTIFICATE-----surely-a-certificate-----END CERTIFICATE-----",
      ["LettuceEncrypt:AdditionalIssuers:1"] = "-----BEGIN CERTIFICATE-----surely-another-certificate-----END CERTIFICATE-----",
    });

    Assert.Collection(options.AdditionalIssuers,
        one => Assert.Equal("-----BEGIN CERTIFICATE-----surely-a-certificate-----END CERTIFICATE-----", one),
        two => Assert.Equal("-----BEGIN CERTIFICATE-----surely-another-certificate-----END CERTIFICATE-----", two));
  }

  private static LettuceEncryptOptions ParseOptions(Dictionary<string, string> input) {
    var config = new ConfigurationBuilder()
               .AddInMemoryCollection(input)
               .Build();

    var services = new ServiceCollection()
        .AddSingleton<IConfiguration>(config)
        .AddLettuceEncrypt()
        .Services
        .BuildServiceProvider(true);

    var options = services.GetRequiredService<IOptions<LettuceEncryptOptions>>();
    return options.Value;
  }
}
