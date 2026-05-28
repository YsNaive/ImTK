using System;
using System.IO;
using ImTK.Database;

namespace ImTK.Test.Database.Mocks
{
    public class MockReadOnlyAsset : ImTKAsset
    {
        public string Content { get; set; }
        public bool WasDisposed { get; private set; }

        public override void Dispose()
        {
            base.Dispose();
            WasDisposed = true;
        }
    }

    public class MockReadOnlyAssetImporter : ImTK.Database.Importers.IAssetImporter<MockReadOnlyAsset>
    {
        public MockReadOnlyAsset Import(string absolutePath, string relativePath)
        {
            var asset = new MockReadOnlyAsset();
            if (File.Exists(absolutePath))
            {
                asset.Content = File.ReadAllText(absolutePath);
            }
            else
            {
                asset.Content = "Default Mock Content";
            }
            return asset;
        }
    }

    public class MockSaveableAsset : ImTKAsset
    {
        public string Content { get; set; } = "Initial Value";
        public bool WasDisposed { get; private set; }

        public override void Dispose()
        {
            base.Dispose();
            WasDisposed = true;
        }
    }

    public class MockSaveableAssetImporter : ImTK.Database.Importers.IAssetImporter<MockSaveableAsset>
    {
        public MockSaveableAsset Import(string absolutePath, string relativePath)
        {
            var asset = new MockSaveableAsset();
            if (File.Exists(absolutePath))
            {
                asset.Content = File.ReadAllText(absolutePath);
            }
            return asset;
        }
    }

    public class MockSaveableAssetExporter : ImTK.Database.Importers.IAssetExporter<MockSaveableAsset>
    {
        public void Export(MockSaveableAsset asset, string absolutePath)
        {
            var dir = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(absolutePath, asset.Content);
        }
    }

    public class AnotherMockAsset : ImTKAsset
    {
    }

    public class AnotherMockAssetImporter : ImTK.Database.Importers.IAssetImporter<AnotherMockAsset>
    {
        public AnotherMockAsset Import(string absolutePath, string relativePath)
        {
            return new AnotherMockAsset();
        }
    }
}
