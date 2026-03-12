using ImGuiNET;
using System.Numerics;

namespace ImTK;

public class ImStyle
{
    private int pushedVarsCount = 0;
    private int pushedColorsCount = 0;

    // ImGuiStyleVar - float
    public float? alpha { get; set; }
    public float? disabledAlpha { get; set; }
    public float? windowRounding { get; set; }
    public float? windowBorderSize { get; set; }
    public float? childRounding { get; set; }
    public float? childBorderSize { get; set; }
    public float? popupRounding { get; set; }
    public float? popupBorderSize { get; set; }
    public float? frameRounding { get; set; }
    public float? frameBorderSize { get; set; }
    public float? indentSpacing { get; set; }
    public float? scrollbarSize { get; set; }
    public float? scrollbarRounding { get; set; }
    public float? grabMinSize { get; set; }
    public float? grabRounding { get; set; }
    public float? tabRounding { get; set; }
    public float? tabBorderSize { get; set; }
    public float? tabBarBorderSize { get; set; }
    public float? tabBarOverlineSize { get; set; }
    public float? tableAngledHeadersAngle { get; set; }

    // ImGuiStyleVar - Vector2
    public Vector2? windowPadding { get; set; }
    public Vector2? windowMinSize { get; set; }
    public Vector2? windowTitleAlign { get; set; }
    public Vector2? framePadding { get; set; }
    public Vector2? itemSpacing { get; set; }
    public Vector2? itemInnerSpacing { get; set; }
    public Vector2? cellPadding { get; set; }
    public Vector2? buttonTextAlign { get; set; }
    public Vector2? selectableTextAlign { get; set; }

    // ImGuiCol - uint
    public uint? textColor { get; set; }
    public uint? textDisabledColor { get; set; }
    public uint? windowBgColor { get; set; }
    public uint? childBgColor { get; set; }
    public uint? popupBgColor { get; set; }
    public uint? borderColor { get; set; }
    public uint? borderShadowColor { get; set; }
    public uint? frameBgColor { get; set; }
    public uint? frameBgHoveredColor { get; set; }
    public uint? frameBgActiveColor { get; set; }
    public uint? titleBgColor { get; set; }
    public uint? titleBgActiveColor { get; set; }
    public uint? titleBgCollapsedColor { get; set; }
    public uint? menuBarBgColor { get; set; }
    public uint? scrollbarBgColor { get; set; }
    public uint? scrollbarGrabColor { get; set; }
    public uint? scrollbarGrabHoveredColor { get; set; }
    public uint? scrollbarGrabActiveColor { get; set; }
    public uint? checkMarkColor { get; set; }
    public uint? sliderGrabColor { get; set; }
    public uint? sliderGrabActiveColor { get; set; }
    public uint? buttonColor { get; set; }
    public uint? buttonHoveredColor { get; set; }
    public uint? buttonActiveColor { get; set; }
    public uint? headerColor { get; set; }
    public uint? headerHoveredColor { get; set; }
    public uint? headerActiveColor { get; set; }
    public uint? separatorColor { get; set; }
    public uint? separatorHoveredColor { get; set; }
    public uint? separatorActiveColor { get; set; }
    public uint? resizeGripColor { get; set; }
    public uint? resizeGripHoveredColor { get; set; }
    public uint? resizeGripActiveColor { get; set; }
    public uint? tabColor { get; set; }
    public uint? tabHoveredColor { get; set; }
    public uint? tabSelectedColor { get; set; }
    public uint? tabSelectedOverlineColor { get; set; }
    public uint? tabDimmedColor { get; set; }
    public uint? tabDimmedSelectedColor { get; set; }
    public uint? tabDimmedSelectedOverlineColor { get; set; }
    public uint? tableBorderStrongColor { get; set; }
    public uint? tableBorderLightColor { get; set; }
    public uint? tableRowBgColor { get; set; }
    public uint? tableRowBgAltColor { get; set; }
    public uint? textSelectedBgColor { get; set; }
    public uint? dragDropTargetColor { get; set; }
    public uint? navWindowingHighlightColor { get; set; }
    public uint? navWindowingDimBgColor { get; set; }
    public uint? modalWindowDimBgColor { get; set; }


    public void Push()
    {
        pushedVarsCount = 0;
        pushedColorsCount = 0;

        // Push StyleVars (float)
        PushStyleVar(ImGuiStyleVar.Alpha, alpha);
        PushStyleVar(ImGuiStyleVar.DisabledAlpha, disabledAlpha);
        PushStyleVar(ImGuiStyleVar.WindowRounding, windowRounding);
        PushStyleVar(ImGuiStyleVar.WindowBorderSize, windowBorderSize);
        PushStyleVar(ImGuiStyleVar.ChildRounding, childRounding);
        PushStyleVar(ImGuiStyleVar.ChildBorderSize, childBorderSize);
        PushStyleVar(ImGuiStyleVar.PopupRounding, popupRounding);
        PushStyleVar(ImGuiStyleVar.PopupBorderSize, popupBorderSize);
        PushStyleVar(ImGuiStyleVar.FrameRounding, frameRounding);
        PushStyleVar(ImGuiStyleVar.FrameBorderSize, frameBorderSize);
        PushStyleVar(ImGuiStyleVar.IndentSpacing, indentSpacing);
        PushStyleVar(ImGuiStyleVar.ScrollbarSize, scrollbarSize);
        PushStyleVar(ImGuiStyleVar.ScrollbarRounding, scrollbarRounding);
        PushStyleVar(ImGuiStyleVar.GrabMinSize, grabMinSize);
        PushStyleVar(ImGuiStyleVar.GrabRounding, grabRounding);
        PushStyleVar(ImGuiStyleVar.TabRounding, tabRounding);
        PushStyleVar(ImGuiStyleVar.TabBorderSize, tabBorderSize);
        PushStyleVar(ImGuiStyleVar.TabBarBorderSize, tabBarBorderSize);
        PushStyleVar(ImGuiStyleVar.TabBarOverlineSize, tabBarOverlineSize);
        PushStyleVar(ImGuiStyleVar.TableAngledHeadersAngle, tableAngledHeadersAngle);

        // Push StyleVars (Vector2)
        PushStyleVar(ImGuiStyleVar.WindowPadding, windowPadding);
        PushStyleVar(ImGuiStyleVar.WindowMinSize, windowMinSize);
        PushStyleVar(ImGuiStyleVar.WindowTitleAlign, windowTitleAlign);
        PushStyleVar(ImGuiStyleVar.FramePadding, framePadding);
        PushStyleVar(ImGuiStyleVar.ItemSpacing, itemSpacing);
        PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, itemInnerSpacing);
        PushStyleVar(ImGuiStyleVar.CellPadding, cellPadding);
        PushStyleVar(ImGuiStyleVar.ButtonTextAlign, buttonTextAlign);
        PushStyleVar(ImGuiStyleVar.SelectableTextAlign, selectableTextAlign);

        // Push StyleColors (uint)
        PushStyleColor(ImGuiCol.Text, textColor);
        PushStyleColor(ImGuiCol.TextDisabled, textDisabledColor);
        PushStyleColor(ImGuiCol.WindowBg, windowBgColor);
        PushStyleColor(ImGuiCol.ChildBg, childBgColor);
        PushStyleColor(ImGuiCol.PopupBg, popupBgColor);
        PushStyleColor(ImGuiCol.Border, borderColor);
        PushStyleColor(ImGuiCol.BorderShadow, borderShadowColor);
        PushStyleColor(ImGuiCol.FrameBg, frameBgColor);
        PushStyleColor(ImGuiCol.FrameBgHovered, frameBgHoveredColor);
        PushStyleColor(ImGuiCol.FrameBgActive, frameBgActiveColor);
        PushStyleColor(ImGuiCol.TitleBg, titleBgColor);
        PushStyleColor(ImGuiCol.TitleBgActive, titleBgActiveColor);
        PushStyleColor(ImGuiCol.TitleBgCollapsed, titleBgCollapsedColor);
        PushStyleColor(ImGuiCol.MenuBarBg, menuBarBgColor);
        PushStyleColor(ImGuiCol.ScrollbarBg, scrollbarBgColor);
        PushStyleColor(ImGuiCol.ScrollbarGrab, scrollbarGrabColor);
        PushStyleColor(ImGuiCol.ScrollbarGrabHovered, scrollbarGrabHoveredColor);
        PushStyleColor(ImGuiCol.ScrollbarGrabActive, scrollbarGrabActiveColor);
        PushStyleColor(ImGuiCol.CheckMark, checkMarkColor);
        PushStyleColor(ImGuiCol.SliderGrab, sliderGrabColor);
        PushStyleColor(ImGuiCol.SliderGrabActive, sliderGrabActiveColor);
        PushStyleColor(ImGuiCol.Button, buttonColor);
        PushStyleColor(ImGuiCol.ButtonHovered, buttonHoveredColor);
        PushStyleColor(ImGuiCol.ButtonActive, buttonActiveColor);
        PushStyleColor(ImGuiCol.Header, headerColor);
        PushStyleColor(ImGuiCol.HeaderHovered, headerHoveredColor);
        PushStyleColor(ImGuiCol.HeaderActive, headerActiveColor);
        PushStyleColor(ImGuiCol.Separator, separatorColor);
        PushStyleColor(ImGuiCol.SeparatorHovered, separatorHoveredColor);
        PushStyleColor(ImGuiCol.SeparatorActive, separatorActiveColor);
        PushStyleColor(ImGuiCol.ResizeGrip, resizeGripColor);
        PushStyleColor(ImGuiCol.ResizeGripHovered, resizeGripHoveredColor);
        PushStyleColor(ImGuiCol.ResizeGripActive, resizeGripActiveColor);
        PushStyleColor(ImGuiCol.Tab, tabColor);
        PushStyleColor(ImGuiCol.TabHovered, tabHoveredColor);
        PushStyleColor(ImGuiCol.TabSelected, tabSelectedColor);
        PushStyleColor(ImGuiCol.TabSelectedOverline, tabSelectedOverlineColor);
        PushStyleColor(ImGuiCol.TabDimmed, tabDimmedColor);
        PushStyleColor(ImGuiCol.TabDimmedSelected, tabDimmedSelectedColor);
        PushStyleColor(ImGuiCol.TabDimmedSelectedOverline, tabDimmedSelectedOverlineColor);
        PushStyleColor(ImGuiCol.TableBorderStrong, tableBorderStrongColor);
        PushStyleColor(ImGuiCol.TableBorderLight, tableBorderLightColor);
        PushStyleColor(ImGuiCol.TableRowBg, tableRowBgColor);
        PushStyleColor(ImGuiCol.TableRowBgAlt, tableRowBgAltColor);
        PushStyleColor(ImGuiCol.TextSelectedBg, textSelectedBgColor);
        PushStyleColor(ImGuiCol.DragDropTarget, dragDropTargetColor);
        PushStyleColor(ImGuiCol.NavWindowingHighlight, navWindowingHighlightColor);
        PushStyleColor(ImGuiCol.NavWindowingDimBg, navWindowingDimBgColor);
        PushStyleColor(ImGuiCol.ModalWindowDimBg, modalWindowDimBgColor);
    }

    public void Pop()
    {
        if (pushedVarsCount > 0)
        {
            ImGui.PopStyleVar(pushedVarsCount);
            pushedVarsCount = 0;
        }

        if (pushedColorsCount > 0)
        {
            ImGui.PopStyleColor(pushedColorsCount);
            pushedColorsCount = 0;
        }
    }

    private void PushStyleVar(ImGuiStyleVar styleVar, float? value)
    {
        if (value.HasValue)
        {
            ImGui.PushStyleVar(styleVar, value.Value);
            pushedVarsCount++;
        }
    }

    private void PushStyleVar(ImGuiStyleVar styleVar, Vector2? value)
    {
        if (value.HasValue)
        {
            ImGui.PushStyleVar(styleVar, value.Value);
            pushedVarsCount++;
        }
    }

    private void PushStyleColor(ImGuiCol styleCol, uint? value)
    {
        if (value.HasValue)
        {
            ImGui.PushStyleColor(styleCol, value.Value);
            pushedColorsCount++;
        }
    }
}
