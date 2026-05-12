using System;

namespace ImTK.UI
{
    /// <summary>
    /// 用於將靜態方法或靜態欄位/屬性註冊為 MainMenu 項目的標籤。
    /// - 標註於 Method (需無參數或只有一個 ClickEvent 參數)：框架會自動呼叫 AddItem。
    /// - 標註於 Field/Property (型別必須為 MenuView)：框架會自動呼叫 AddMenu 將其掛載到指定的父節點路徑下。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class MainMenuAttribute : Attribute
    {
        public string path { get; }
        public int priority { get; set; }

        public MainMenuAttribute(string path)
        {
            this.path = path;
            this.priority = 0;
        }
    }
}
