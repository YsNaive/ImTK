using System;

namespace ImTK.UI
{
    /// <summary>
    /// 定義選單系統元素的基礎介面。
    /// 實作此介面的元件才能被 MenuView 正確排序與渲染。
    /// </summary>
    public interface IMenuElement
    {
        /// <summary>
        /// 選單元素的顯示名稱。
        /// </summary>
        string name { get; set; }

        /// <summary>
        /// 用於決定選單元素渲染順序的優先權。數值越小越靠前。
        /// 當相鄰兩個元素的 priority 差距超過一定閾值時，MenuView 會自動插入分隔線。
        /// </summary>
        int priority { get; set; }
    }
}
