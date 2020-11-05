using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetDownloader
{
    public class Downloader
    {
        private IEnumerable<string> _packageIdList;
        private string _outputDirectory;
        private IEnumerable<string> _packageVersionList;
        private bool _preReleased;
        private bool _withDependency;
        private IEnumerable<string> _targetFramework;
        private string _repository;
        private bool _verbose;

        private Queue<PackageInfo> _downloadQueue = new Queue<PackageInfo>();
        private List<string> _cacheDownloadedFileName = new List<string>();

        private ILogger logger = NullLogger.Instance;
        private CancellationToken cancellationToken = CancellationToken.None;
        private SourceCacheContext cache = new SourceCacheContext();
        private SourceRepository repository;



        public Downloader(DownloadOptions opts)
        {
            _packageIdList = opts.Package;
            _outputDirectory = opts.OutputDir;
            _packageVersionList = opts.Version;
            _preReleased = opts.PreReleased;
            _withDependency = opts.WithDependency;
            _targetFramework = opts.TargetFramework;
            _repository = opts.Repository;
            _verbose = opts.Verbose;

            logger = NullLogger.Instance;
            cache = new SourceCacheContext();
            repository = Repository.Factory.GetCoreV3(_repository);
    }

        public async Task<PackageInfo> GetBestMatchPackageVersionsAsync(string packageId, VersionRange range)
        {
            cancellationToken = CancellationToken.None;
            FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();

            IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync(
                packageId,
                cache,
                logger,
                cancellationToken);

            NuGetVersion bestMatchVersion = range.FindBestMatch(versions);
            return new PackageInfo { packageId = packageId, packageVersion = bestMatchVersion };
        }

        public async Task DownloadPackageAsync()
        {
            // check if output directory exists
            if (!Directory.Exists(_outputDirectory))
            {
                Directory.CreateDirectory(_outputDirectory);
            }

            // set all parent packages
            for (int i = 0; i < _packageIdList.Count(); i++)
            {
                string packageId = _packageIdList.ElementAt(i);

                PackageInfo package = null;
                if (_packageVersionList != null && _packageVersionList.Count() > i)
                {
                    string packageVersionValue = _packageVersionList.ElementAt(i);
                    NuGetVersion packageVersion = new NuGetVersion(packageVersionValue);
                    package = new PackageInfo { packageId = packageId, packageVersion = packageVersion };
                    _downloadQueue.Enqueue(package);
                }
                else
                {
                    FloatRange floatRange = null;
                    if (_preReleased == true)
                    {
                        // include pre-release
                        floatRange = new FloatRange(NuGetVersionFloatBehavior.AbsoluteLatest);
                    }
                    else
                    {
                        // released
                        floatRange = new FloatRange(NuGetVersionFloatBehavior.Major);
                    }
                    FloatRange fr = new FloatRange(NuGetVersionFloatBehavior.Major);
                    VersionRange range = new VersionRange(floatRange:fr);
                    
                    package = await GetBestMatchPackageVersionsAsync(packageId, range);
                }
                _downloadQueue.Enqueue(package);
            }

            cancellationToken = CancellationToken.None;
            FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();

            while (_downloadQueue.Count > 0)
            {
                PackageInfo package = _downloadQueue.Dequeue();

                string packageFilePath = $"{_outputDirectory}/{package.packageId}.{package.packageVersion}.nupkg";

                if (_cacheDownloadedFileName.Contains(packageFilePath))
                {
                    continue;
                }
                else
                {
                    _cacheDownloadedFileName.Add(packageFilePath);
                }

                using FileStream packageStream = new FileStream(packageFilePath, FileMode.Create);

                await resource.CopyNupkgToStreamAsync(
                    package.packageId,
                    package.packageVersion,
                    packageStream,
                    cache,
                    logger,
                    cancellationToken);

                Console.WriteLine($"Downloaded package {package.packageId} {package.packageVersion}");

                using PackageArchiveReader packageReader = new PackageArchiveReader(packageStream);
                NuspecReader nuspecReader = await packageReader.GetNuspecReaderAsync(cancellationToken);

                Console.WriteLine($"Tags: {nuspecReader.GetTags()}");
                Console.WriteLine($"Description: {nuspecReader.GetDescription()}");

                using PackageArchiveReader reader = new PackageArchiveReader(packageStream);
                NuspecReader nuspec = reader.NuspecReader;
                Console.WriteLine($"ID: {nuspec.GetId()}");
                Console.WriteLine($"Version: {nuspec.GetVersion()}");
                Console.WriteLine($"Description: {nuspec.GetDescription()}");
                Console.WriteLine($"Authors: {nuspec.GetAuthors()}");

                if (_withDependency == false)
                {
                    Console.WriteLine("\nDependencies download is not need.");
                    continue;
                }

                Console.WriteLine("\nDependencies:");
                foreach (var dependencyGroup in nuspec.GetDependencyGroups())
                {
                    Console.WriteLine($" - {dependencyGroup.TargetFramework.GetFrameworkString()}");

                    // check target framework
                    if (!_targetFramework.Contains("all", StringComparer.InvariantCultureIgnoreCase) 
                        && !_targetFramework.Contains(dependencyGroup.TargetFramework.GetFrameworkString(), StringComparer.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine($" -- {dependencyGroup.TargetFramework.GetFrameworkString()} not match target framework.");
                        continue;
                    }

                    foreach (var dependency in dependencyGroup.Packages)
                    {
                        Console.WriteLine($"   > {dependency.Id} {dependency.VersionRange}");

                        PackageInfo dependencyPackage = await GetBestMatchPackageVersionsAsync(dependency.Id, dependency.VersionRange);
                        Console.WriteLine($"   -- best match version: {dependencyPackage.packageVersion}");
                        _downloadQueue.Enqueue(dependencyPackage);
                    }
                }
            }
        }
    }
}
