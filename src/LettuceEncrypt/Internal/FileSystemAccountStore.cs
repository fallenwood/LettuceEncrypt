﻿// Copyright (c) Nate McMaster.
// Copyright (c) Fallenwood.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace LettuceEncrypt.Internal;

using System.Text.Json;
using LettuceEncrypt.Accounts;
using LettuceEncrypt.Acme;
using Microsoft.Extensions.Logging;


internal class FileSystemAccountStore : IAccountStore {
    private static JsonSerializerOptions JsonSerializerOptions = new {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly DirectoryInfo _accountDir;
    private readonly ILogger _logger;

    public FileSystemAccountStore(
        ILogger logger,
        ICertificateAuthorityConfiguration certificateAuthority)
        : this(new DirectoryInfo(AppContext.BaseDirectory), logger, certificateAuthority)
    {
    }

    public FileSystemAccountStore(
        DirectoryInfo rootDirectory,
        ILogger logger,
        ICertificateAuthorityConfiguration certificateAuthority)
    {
        _logger = logger;

        var topAccountDir = rootDirectory.CreateSubdirectory("accounts");
        var directoryUri = certificateAuthority.AcmeDirectoryUri;
        var subPath = Path.Combine(directoryUri.Authority, directoryUri.LocalPath.Substring(1));
        _accountDir = topAccountDir.CreateSubdirectory(subPath);
    }

    public async Task<AccountModel?> GetAccountAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Looking for account information in {path}", _accountDir.FullName);

        foreach (var jsonFile in _accountDir.GetFiles("*.json"))
        {
            _logger.LogTrace("Parsing {path} for account info", jsonFile);

            var accountModel = await Deserialize(jsonFile, cancellationToken);
            if (accountModel != null)
            {
                _logger.LogDebug("Loaded account information from {path}", _accountDir.FullName);
                return accountModel;
            }
        }

        _logger.LogDebug("Could not find account information in {path}", _accountDir.FullName);
        return default;
    }

    private static async Task<AccountModel?> Deserialize(FileInfo jsonFile, CancellationToken cancellationToken)
    {
        using var fileStream = jsonFile.OpenRead();

        return await JsonSerializer.DeserializeAsync<AccountModel>(
            fileStream,
            LettuceEncryptJsonContext.Default.AccountModel,
            JsonSerializerOptions,
            cancellationToken);
    }

    public async Task SaveAccountAsync(AccountModel account, CancellationToken cancellationToken)
    {
        _accountDir.Create();

        var jsonFile = new FileInfo(Path.Combine(_accountDir.FullName, $"{account.Id}.json"));
        _logger.LogTrace("Saving account information to {path}", jsonFile.FullName);

        using var writeStream = jsonFile.OpenWrite();
        await JsonSerializer.SerializeAsync(
            writeStream,
            account,
            LettuceEncryptJsonContext.Default.AccountModel,
            JsonSerializerOptions,
            cancellationToken);

        _logger.LogDebug("Saved account information to {path}", jsonFile.FullName);
    }
}
