// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.DotNet.Watcher
{
    public readonly struct FileItem
    {
        public FileItem(string name, WatchAction watchAction = WatchAction.Default, string staticWebAssetPath = null)
        {
            Name = name;
            WatchAction = watchAction;
            StaticWebAssetPath = staticWebAssetPath;
        }

        public string Name { get; }

        public WatchAction WatchAction { get; }

        public string StaticWebAssetPath { get; }
    }
}
