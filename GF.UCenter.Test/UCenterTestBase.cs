﻿namespace GF.UCenter.Test
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition.Hosting;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Common.Settings;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Web;

    [TestClass]
    public class UCenterTestBase
    {
        private static readonly List<IDisposable> disposableList = new List<IDisposable>();

        private static readonly Lazy<List<char>> CharsPool = new Lazy<List<char>>(() =>
        {
            var chars = new List<char>();
            chars.AddRange(ParallelEnumerable.Range(48, 10).Select(i => (char)i)); // 0-9
            chars.AddRange(ParallelEnumerable.Range(65, 26).Select(i => (char)i)); // A-Z
            chars.AddRange(ParallelEnumerable.Range(97, 26).Select(i => (char)i)); // a-z
            return chars;
        }, LazyThreadSafetyMode.ExecutionAndPublication);

        internal static ExportProvider ExportProvider;

        protected readonly TenantEnvironment Tenant;

        public UCenterTestBase()
        {
            this.Tenant = ExportProvider.GetExportedValue<TenantEnvironment>();
        }

        protected string GenerateRandomString(int length = 8)
        {
            var random = new Random();
            var result = new List<char>();
            var maxIdx = CharsPool.Value.Count;
            result.Add(CharsPool.Value.ElementAt(random.Next(11, maxIdx)));

            for (var idx = 0; idx < length - 1; idx++)
            {
                result.Add(CharsPool.Value.ElementAt(random.Next(0, maxIdx)));
            }

            return string.Join("", result);
        }

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            AssemblyInitializeAsync().Wait();
        }

        private static async Task AssemblyInitializeAsync()
        {
            ExportProvider = CompositionContainerFactory.Create();

            SettingsInitializer.Initialize<Settings>(
                ExportProvider,
                SettingsDefaultValueProvider<Settings>.Default,
                AppConfigurationValueProvider.Default);

            SettingsInitializer.Initialize<Common.Settings.Settings>(
                ExportProvider,
                SettingsDefaultValueProvider<Common.Settings.Settings>.Default,
                AppConfigurationValueProvider.Default);

            var settings = ExportProvider.GetExportedValue<Common.Settings.Settings>();

            await InitProfileImageBlobsAsync(settings.DefaultProfileImageForFemaleBlobName);
            await InitProfileImageBlobsAsync(settings.DefaultProfileImageForMaleBlobName);
            await InitProfileImageBlobsAsync(settings.DefaultProfileThumbnailForFemaleBlobName);
            await InitProfileImageBlobsAsync(settings.DefaultProfileThumbnailForMaleBlobName);
        }

        private static async Task InitProfileImageBlobsAsync(string blobName)
        {
            using (var fileStream = File.OpenRead(@"TestData\github.png"))
            {
                var settings = ExportProvider.GetExportedValue<Common.Settings.Settings>();
                var blobContext = new StorageAccountContext(settings);
                await blobContext.UploadBlobAsync(blobName, fileStream, CancellationToken.None);
            }
        }

        [AssemblyCleanup]
        public static void AssemblyCleanUp()
        {
            foreach (var item in disposableList)
            {
                if (item != null)
                {
                    item.Dispose();
                }
            }
        }
    }
}