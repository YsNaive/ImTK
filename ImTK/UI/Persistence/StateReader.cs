using ImTK.Database;

namespace ImTK.UI.Persistence
{
    /// <summary>
    /// 提供 UI 元件讀取快取狀態的安全介面。
    /// 會自動處理 Key 的 Window 前綴與預設值回退邏輯。
    /// </summary>
    public class StateReader
    {
        private readonly ImTKCacheAsset _asset;
        private readonly string _prefix;

        internal StateReader(ImTKCacheAsset asset, string prefix)
        {
            _asset = asset;
            _prefix = prefix;
        }

        private string GetKey(string localKey) => $"{_prefix}/{localKey}";

        public float ReadFloat(string localKey, float defaultValue)
        {
            if (_asset.Floats.TryGetValue(GetKey(localKey), out float value))
                return value;
            return defaultValue;
        }

        public int ReadInt(string localKey, int defaultValue)
        {
            if (_asset.Ints.TryGetValue(GetKey(localKey), out int value))
                return value;
            return defaultValue;
        }

        public string ReadString(string localKey, string defaultValue)
        {
            if (_asset.Strings.TryGetValue(GetKey(localKey), out string value))
                return value;
            return defaultValue;
        }

        public bool ReadBool(string localKey, bool defaultValue)
        {
            if (_asset.Bools.TryGetValue(GetKey(localKey), out bool value))
                return value;
            return defaultValue;
        }
    }
}
