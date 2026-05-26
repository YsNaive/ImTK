namespace ImTK.UI
{
    /// <summary>
    /// 定義 Drawer 在視覺樹中的排版模式。
    /// </summary>
    public enum DrawerLayoutMode
    {
        /// <summary>
        /// 同行展開排版。Label 與內容會在同一行顯示 (FlexDirection.Row)。
        /// 適用於單一或小型的輸入控制項 (例如數值、字串)。
        /// </summary>
        Inline,

        /// <summary>
        /// 換行展開排版。Label 顯示在上方，內容在下一行顯示 (FlexDirection.Column)。
        /// 適用於需要較大空間的複合元件或複雜物件 (例如 ObjectDrawer)。
        /// </summary>
        Expand
    }
}
