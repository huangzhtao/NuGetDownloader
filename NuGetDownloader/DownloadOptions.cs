using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace NuGetDownloader
{
    public class DownloadOptions
    {
        [Option('p', "packages", Required = true, HelpText = "Input package name list to download.")]
        public IEnumerable<string> Package { get; set; }

        [Option('o', "output", Required = false, Default = "./packages", HelpText = "Download directory.")]
        public string OutputDir { get; set; }

        [Option('v', "versions", Required = false, HelpText = "Input package version list to download. If not set, download the newest version.")]
        public IEnumerable<string> Version { get; set; }

        [Option('r', "pre-release", Required = false, Default = false, HelpText = "It works if download the newest version, include pre-released version or not.")]
        public bool PreReleased { get; set; }

        [Option('d', "dependency", Required = false, Default = false, HelpText = "Set if dependencies download is needed.")]
        public bool WithDependency { get; set; }

        [Option('f', "target-framework", Required = false, Default = "all", HelpText = "Set target framework to download dependencies, default to all.")]
        public IEnumerable<string> TargetFramework { get; set; }

        [Option("repository", Required = false, Default = "https://api.nuget.org/v3/index.json", HelpText = "Set repository other than the official repository.")]
        public string Repository { get; set; }

        [Option("verbose", Required = false, Default = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }
    }
}
