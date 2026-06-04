using System;

namespace ImTK.UI
{
    public class VisualElementGizmoContext
    {
        /// <summary>
        /// 決定這個 Context 是否要對傳入的元件執行 action。
        /// 若為 null 則代表作用於所有元件 (全域)。
        /// </summary>
        public Func<VisualElement, bool> filter { get; set; }
        
        /// <summary>
        /// 執行自定義的 Gizmo 繪圖指令。這會在該元件的 ImGui Window Context 中被呼叫，
        /// 確保 GetWindowViewport() 與座標轉換是絕對準確的。
        /// </summary>
        public Action<VisualElement> action { get; set; }
    }
}
