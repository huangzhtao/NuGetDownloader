using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace NuGetDownloader
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var result = await Parser.Default.ParseArguments<DownloadOptions>(args)
                    .WithParsedAsync<DownloadOptions>(DownloadOptionsAsync);
        }

        static async Task DownloadOptionsAsync(DownloadOptions opts)
        {
            Downloader download = new Downloader(opts);
            await download.DownloadPackageAsync();
        }
    }
}
