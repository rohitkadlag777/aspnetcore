// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.DotNet.Watcher.Tools
{
    public class StaticContentHandler
    {
        private static readonly byte[] UpdateCssMessage = Encoding.UTF8.GetBytes("UpdateCSS");

        internal static async ValueTask<bool> TryHandleFileAction(BrowserRefreshServer browserRefreshServer, FileItem fileItem, CancellationToken cancellationToken)
        {
            if (browserRefreshServer is null)
            {
                return false;
            }

            if (fileItem.Name.EndsWith(".css", StringComparison.Ordinal)
                && await TryReadFileAsync(fileItem, cancellationToken) is byte[] cssBytes)
            {
                var filePath = fileItem.StaticWebAssetPath;

                var bytesToRent = UpdateCssMessage.Length + Encoding.UTF8.GetMaxByteCount(filePath.Length + 2) + cssBytes.Length;
                var bytes = ArrayPool<byte>.Shared.Rent(bytesToRent);

                try
                {
                    UpdateCssMessage.CopyTo(bytes.AsSpan());
                    var length = UpdateCssMessage.Length;
                    length += Encoding.UTF8.GetBytes(filePath + "||", bytes.AsSpan(length));
                    cssBytes.CopyTo(bytes.AsSpan(length));
                    length += cssBytes.Length;

                    await browserRefreshServer.SendMessage(bytes.AsMemory(0, length), cancellationToken);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(bytes);
                }
            }
            else if (fileItem.Name.EndsWith(".js", StringComparison.Ordinal))
            {
                await browserRefreshServer.ReloadAsync(cancellationToken);
            }

            return true;
        }

        private static async Task<byte[]> TryReadFileAsync(FileItem fileItem, CancellationToken cancellationToken)
        {
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    await Task.Delay((i + 1) * 100, cancellationToken);
                    return await File.ReadAllBytesAsync(fileItem.Name, cancellationToken);
                }
                catch (IOException)
                {
                }
            }

            return null;
        }
    }
}
