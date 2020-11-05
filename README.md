# NuGetDownloader
NuGet Package Downloader for Offline Environment

# Usage

**Supported Parameters**

  -p, --packages            Required. Input package name list to download.

  -o, --output              (Default: ./packages) Download directory.

  -v, --versions            Input package version list to download. If not set, download the newest version.

  -r, --pre-release         (Default: false) It works if download the newest version, include the pre-released version or not.

  -d, --dependency          (Default: false) Set if dependencies download is needed.

  -f, --target-framework    (Default: all) Set target framework to download dependencies, default to all.

  --repository              (Default: https://api.nuget.org/v3/index.json) Set repository other than the official repository.

  --verbose                 (Default: false) Set output to verbose messages.

  --help                    Display this help screen.

  --version                 Display version information.
