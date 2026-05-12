using System;
using System.IO;
using ImTK.Database;

namespace ImTK.Test.Database.Mocks
{
    public class MockReadOnlyAsset : ImTKAsset
    {
        public string Content { get; private set; }
        public bool WasDisposed { get; private set; }

        protected internal override void OnLoad(string absolutePath)
        {
            if (File.Exists(absolutePath))
            {
                Content = File.ReadAllText(absolutePath);
            }
            else
            {
                Content = "Default Mock Content";
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            WasDisposed = true;
        }
    }

    public class MockSaveableAsset : ImTKSaveableAsset
    {
        public string Content { get; set; } = "Initial Value";
        public bool WasDisposed { get; private set; }

        protected internal override void OnLoad(string absolutePath)
        {
            if (File.Exists(absolutePath))
            {
                Content = File.ReadAllText(absolutePath);
            }
        }

        protected internal override void OnSave(string absolutePath)
        {
            File.WriteAllText(absolutePath, Content);
        }

        public override void Dispose()
        {
            base.Dispose();
            WasDisposed = true;
        }
    }

    public class AnotherMockAsset : ImTKAsset
    {
        protected internal override void OnLoad(string absolutePath) { }
    }
}
