﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;

namespace Microsoft.CodeAnalysis.Tools.Tests
{
    /// <summary>
    /// This test fixture sets the <see cref="Environment.CurrentDirectory" /> to the dotnet-format solution's path.
    /// </summary>
    public class SolutionPathFixture : IDisposable
    {
        private static int _registered = 0;
        private static string _currentDirectory;

        public void SetCurrentDirectory()
        {
            if (Interlocked.Exchange(ref _registered, 1) == 0)
            {
                _currentDirectory = Environment.CurrentDirectory;
                var solutionPath = Directory.GetParent(_currentDirectory).Parent.Parent.Parent.Parent.FullName;
                Environment.CurrentDirectory = solutionPath;
            }
        }

        public void Dispose()
        {
            Environment.CurrentDirectory = _currentDirectory;
            _currentDirectory = null;
        }
    }
}
