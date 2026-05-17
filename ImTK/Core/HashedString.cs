using System;

namespace ImTK.Core
{
    /// <summary>
    /// 一個輕量級的字串識別碼，用於高效的系統查找 (例如 Theme Token, Class Name)
    /// </summary>
    public readonly struct HashedString : IEquatable<HashedString>
    {
        public readonly string Value;
        public readonly int Hash;

        public HashedString(string value)
        {
            Value = value ?? string.Empty;
            Hash = Value.GetHashCode();
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
