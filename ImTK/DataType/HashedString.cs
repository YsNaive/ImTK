using ImTK.Log;
using ImTK.Core;
using System;

namespace ImTK
{
    /// <summary>
    /// 一個輕量級的字串識別碼，用於高效的系統查找 (例如 Theme Token, Class Name)
    /// </summary>
    public readonly struct HashedString : IEquatable<HashedString>
    {

        
        // 全域字串到 Hash 的對應 (加速查找)
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, int> s_stringToHash = new System.Collections.Concurrent.ConcurrentDictionary<string, int>();
        // 全域 Hash 到字串的對應 (防碰撞註冊表)
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, string> s_hashToString = new System.Collections.Concurrent.ConcurrentDictionary<int, string>();

        public readonly string Value;
        public readonly int Hash;

        public HashedString(string value)
        {
            Value = value ?? string.Empty;

            if (s_stringToHash.TryGetValue(Value, out int existingHash))
            {
                Hash = existingHash;
                return;
            }

            if (s_stringToHash.Count >= ImTKEnvironment.HashedStringCapacityWarningThreshold)
            {
                ImTKLog.Error($"HashedString registry capacity exceeded threshold ({ImTKEnvironment.HashedStringCapacityWarningThreshold}). Potential memory leak from dynamically generated strings.");
            }

            // 1. 計算初始 Hash (使用決定性的 FNV-1a 32-bit 演算法)
            int hash = ComputeFNV1a(Value);
            if (hash == 0) hash = 1;

            // 2. 註冊與自動防碰撞機制
            while (true)
            {
                if (s_hashToString.TryAdd(hash, Value))
                {
                    // 成功註冊沒有碰撞
                    s_stringToHash.TryAdd(Value, hash);
                    Hash = hash;
                    break;
                }
                else
                {
                    // Hash 已經存在，檢查是否為同一個字串
                    if (s_hashToString.TryGetValue(hash, out string registeredValue))
                    {
                        if (registeredValue == Value)
                        {
                            // 已經被其他執行緒註冊
                            Hash = hash;
                            break;
                        }
                        else
                        {
                            // 發生了真實的雜湊碰撞！發出警告並自動解決 (Linear Probing)
                            ImTKLog.Warning($"Hash collision detected between '{registeredValue}' and '{Value}'. Resolving automatically.");
                            hash = hash + 1;
                            if (hash == 0) hash = 1;
                        }
                    }
                }
            }
        }

        private static int ComputeFNV1a(string text)
        {
            unchecked
            {
                int hash = (int)2166136261;
                foreach (char c in text)
                {
                    hash = (hash ^ c) * 16777619;
                }
                return hash;
            }
        }

        // 隱式轉換：讓開發者可以無縫傳入字串
        public static implicit operator HashedString(string value) => new HashedString(value);

        // 隱式轉換：回傳字串 (主要用於 Log 或 Debug)
        public static implicit operator string(HashedString hs) => hs.Value;

        public bool Equals(HashedString other) => Hash == other.Hash;

        public override bool Equals(object obj) => obj is HashedString other && Equals(other);

        public override int GetHashCode() => Hash;

        public static bool operator ==(HashedString left, HashedString right) => left.Hash == right.Hash;

        public static bool operator !=(HashedString left, HashedString right) => left.Hash != right.Hash;

        public override string ToString() => Value;
    }
}
