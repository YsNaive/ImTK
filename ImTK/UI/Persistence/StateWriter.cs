using ImTK.Database;

namespace ImTK.UI.Persistence
{
    /// <summary>
    /// 提供 UI 元件寫入快取狀態的安全介面。
    /// 寫入時若數值發生實質改變，會將 IsDirty 標記為 true。
    /// </summary>
    public class StateWriter
    {
        private readonly ImTKCacheAsset _asset;
        private readonly string _prefix;
        
        internal bool IsDirty { get; private set; }

        internal StateWriter(ImTKCacheAsset asset, string prefix)
        {
            _asset = asset;
            _prefix = prefix;
            IsDirty = false;
        }

        private string GetKey(string localKey) => $"{_prefix}/{localKey}";

        public void WriteFloat(string localKey, float value)
        {
            string key = GetKey(localKey);
            if (!_asset.Floats.TryGetValue(key, out float existing) || existing != value)
            {
                _asset.Floats[key] = value;
                IsDirty = true;
            }
        }

        public void WriteInt(string localKey, int value)
        {
            string key = GetKey(localKey);
            if (!_asset.Ints.TryGetValue(key, out int existing) || existing != value)
            {
                _asset.Ints[key] = value;
                IsDirty = true;
            }
        }

        public void WriteString(string localKey, string value)
        {
            string key = GetKey(localKey);
            if (!_asset.Strings.TryGetValue(key, out string existing) || existing != value)
            {
                _asset.Strings[key] = value;
                IsDirty = true;
            }
        }

        public void WriteBool(string localKey, bool value)
        {
            string key = GetKey(localKey);
            if (!_asset.Bools.TryGetValue(key, out bool existing) || existing != value)
            {
                _asset.Bools[key] = value;
                IsDirty = true;
            }
        }
    }
}
