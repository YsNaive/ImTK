using System;

namespace ImTK.Database
{
    /// <summary>
    /// 當指定的資源檔案不存在時拋出。
    /// </summary>
    public class AssetNotFoundException : Exception
    {
        public AssetNotFoundException(string path)
            : base($"Asset not found at path: {path}") { }
    }

    /// <summary>
    /// 當嘗試建立資源但檔案已存在時拋出。
    /// </summary>
    public class AssetAlreadyExistsException : Exception
    {
        public AssetAlreadyExistsException(string path)
            : base($"Asset already exists at path: {path}") { }
    }

    /// <summary>
    /// 當嘗試用錯誤的泛型型別載入已快取的資源時拋出，確保單一路徑絕對型別的一致性。
    /// </summary>
    public class AssetTypeMismatchException : Exception
    {
        public AssetTypeMismatchException(string path, Type expectedType, Type actualType)
            : base($"Asset at path '{path}' was requested as {expectedType.Name}, but is cached as {actualType.Name}.") { }
    }

    /// <summary>
    /// 當傳入的路徑格式不合法（例如絕對路徑）時拋出。
    /// </summary>
    public class AssetPathInvalidException : Exception
    {
        public AssetPathInvalidException(string path, string reason)
            : base($"Invalid asset path '{path}': {reason}") { }
    }
}
