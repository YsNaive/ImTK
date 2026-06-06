using System;
using System.Reflection;
using System.Collections.Generic;
using Hexa.NET.ImGui;
using ImTK.UI;
using ImTK.Core;
using ImTK.Log;

namespace ImTK.DebugTools
{
    public class InspectorPropertiesPanel : ScrollView
    {
        private VisualElement m_target;

        private InspectorHeaderView m_header;
        private BoxModelVisualizer m_boxModel;
        private StyleVisualizer m_styleVisualizer;

        public InspectorPropertiesPanel()
        {
            this.style.flexGrow = 1f;
            this.style.padding = new Thickness(10f);
            this.style.itemSpacing = new System.Numerics.Vector2(0, 10f);
            this.flags.horizontalScrollbar = true;
            m_header = new InspectorHeaderView();
            m_header.style.minHeight = 160f;

            m_boxModel = new BoxModelVisualizer();
            m_boxModel.style.minHeight = 180f;

            m_styleVisualizer = new StyleVisualizer();
            m_styleVisualizer.style.flexGrow = 1f;

            Add(m_header);
            Add(new Label("Layout") { style = { colorFamily = ThemeColorFamily.Warning } });
            Add(m_boxModel);

            Add(new Label("Style") { style = { colorFamily = ThemeColorFamily.Warning } });
            Add(m_styleVisualizer);
        }

        public void SetTarget(VisualElement target)
        {
            m_target = target;
            m_header.target = target;
            m_boxModel.target = target;
            m_styleVisualizer.SetTarget(target);
        }

        public override void OnRender()
        {
            // 防呆：如果 target 被銷毀或移除，自動清空
            if (m_target != null && m_target.hierarchy.parent == null && !(m_target is Window))
            {
                SetTarget(null);
            }
            base.OnRender();
        }
    }

    public class InspectorHeaderView : VisualElement
    {
        public VisualElement target;

        protected override System.Numerics.Vector2 MeasureContent(LayoutConstraint constraint)
        {
            // Fixed height to prevent overlapping with siblings during Yoga layout
            // 如果 AvailableWidth 是無限大 (例如在水平捲動的 ScrollView 中)，我們只回報基本的最小寬度 (200f)
            // 在排版階段 (ArrangeContent) 會透過 Flexbox 的 AlignItems.Stretch 自動拉伸填滿虛擬寬度
            float width = float.IsPositiveInfinity(constraint.AvailableWidth) ? 200f : (constraint.AvailableWidth > 0 ? constraint.AvailableWidth : 200f);
            return new System.Numerics.Vector2(width, 260f);
        }

        public override void OnRender()
        {
            base.OnRender();

            if (target == null)
            {
                var hNone = new ImTKUtf8StringHandler(16, 0);
                hNone.AppendLiteral("Selected ID: None");
                RenderEngine.TextDisabledBuffered(ref hNone);
                return;
            }

            ImGui.BeginGroup();

            var hType = new ImTKUtf8StringHandler(32, 1);
            hType.AppendLiteral("Type: ");
            hType.AppendFormatted(target.GetType().Name);
            RenderEngine.TextBuffered(ref hType);

            var hId = new ImTKUtf8StringHandler(32, 1);
            hId.AppendLiteral("ID: ");
            hId.AppendFormatted(target.m_elementId);
            RenderEngine.TextBuffered(ref hId);

            var hClass = new ImTKUtf8StringHandler(64, 1);
            hClass.AppendLiteral("ClassList: ");
            var classes = target.classList;
            bool hasClass = false;
            foreach (var c in classes)
            {
                if (hasClass) hClass.AppendLiteral(", ");
                hClass.AppendFormatted(c.Value);
                hasClass = true;
            }
            if (!hasClass)
            {
                hClass.AppendLiteral("None");
            }
            RenderEngine.TextBuffered(ref hClass);

            ImGui.Separator();

            var hState = new ImTKUtf8StringHandler(16, 0);
            hState.AppendLiteral("Live State:");
            RenderEngine.TextColoredBuffered(ImTKTheme.GlobalTheme.successColor.text, ref hState);

            var hChild = new ImTKUtf8StringHandler(32, 1);
            hChild.AppendLiteral("Child Count: ");
            hChild.AppendFormatted(target.hierarchy.childCount);
            RenderEngine.TextBuffered(ref hChild);

            var hHover = new ImTKUtf8StringHandler(32, 1);
            hHover.AppendLiteral("Is Hovered: ");
            hHover.AppendFormatted(target.CheckHoverState() ? "True" : "False");
            RenderEngine.TextBuffered(ref hHover);

            var hPick = new ImTKUtf8StringHandler(32, 1);
            hPick.AppendLiteral("Picking Mode: ");
            hPick.AppendFormatted(target.pickingMode.ToString()); // Enum ToString is okay for debug panel
            RenderEngine.TextBuffered(ref hPick);

            var hDisp = new ImTKUtf8StringHandler(32, 1);
            hDisp.AppendLiteral("Display: ");
            hDisp.AppendFormatted(target.resolvedLayoutState.display.ToString());
            RenderEngine.TextBuffered(ref hDisp);

            ImGui.EndGroup();
        }
    }

    public class BoxModelVisualizer : VisualElement
    {
        public VisualElement target;

        private (float minW, float minH, float stepX, float stepY) GetBoxSizes()
        {
            float lineHeight = ImGui.GetTextLineHeight();
            float stepY = lineHeight * 1.25f;
            float stepX = Math.Max(lineHeight * 1.25f, ImGui.CalcTextSize("999.9").X + 16f); // 確保橫向能容納文字與 Padding
            
            float contentW = ImGui.CalcTextSize("9999.9 x 9999.9").X + 24f; // 給 content 加一點 padding
            float contentH = lineHeight * 2f;
            
            float minW = contentW + 6 * stepX;
            float minH = contentH + 6 * stepY;
            
            return (minW, minH, stepX, stepY);
        }

        protected override System.Numerics.Vector2 MeasureContent(LayoutConstraint constraint)
        {
            var (minW, minH, _, _) = GetBoxSizes();
            
            float width = float.IsPositiveInfinity(constraint.AvailableWidth) ? minW : Math.Max(minW, constraint.AvailableWidth);
            float height = float.IsPositiveInfinity(constraint.AvailableHeight) ? minH : Math.Max(minH, constraint.AvailableHeight);
            
            return new System.Numerics.Vector2(width, height);
        }

        public override void OnRender()
        {
            base.OnRender();
            if (target == null) return;

            var drawList = ImGui.GetWindowDrawList();
            
            var (boxWidth, boxHeight, stepX, stepY) = GetBoxSizes();
            
            var actualRectPos = layoutRect.position - RenderEngine.Context.CurrentRenderOffset;
            var center = new System.Numerics.Vector2(actualRectPos.X + layoutRect.width / 2f, actualRectPos.Y + layoutRect.height / 2f);
            
            // 置中繪製，不強迫填滿整個 layoutRect
            System.Numerics.Vector2 marginMin = new System.Numerics.Vector2(center.X - boxWidth / 2f, center.Y - boxHeight / 2f);
            System.Numerics.Vector2 marginMax = new System.Numerics.Vector2(center.X + boxWidth / 2f, center.Y + boxHeight / 2f);

            System.Numerics.Vector2 borderMin = new System.Numerics.Vector2(marginMin.X + stepX, marginMin.Y + stepY);
            System.Numerics.Vector2 borderMax = new System.Numerics.Vector2(marginMax.X - stepX, marginMax.Y - stepY);

            System.Numerics.Vector2 paddingMin = new System.Numerics.Vector2(borderMin.X + stepX, borderMin.Y + stepY);
            System.Numerics.Vector2 paddingMax = new System.Numerics.Vector2(borderMax.X - stepX, borderMax.Y - stepY);

            System.Numerics.Vector2 contentMin = new System.Numerics.Vector2(paddingMin.X + stepX, paddingMin.Y + stepY);
            System.Numerics.Vector2 contentMax = new System.Numerics.Vector2(paddingMax.X - stepX, paddingMax.Y - stepY);

            // Draw Rects
            uint marginColor = new ImTK.Color(0.9f, 0.6f, 0.3f, 0.5f).ToUInt32();
            uint borderColor = new ImTK.Color(0.9f, 0.8f, 0.2f, 0.5f).ToUInt32();
            uint paddingColor = new ImTK.Color(0.5f, 0.8f, 0.4f, 0.5f).ToUInt32();
            uint contentColor = new ImTK.Color(0.3f, 0.6f, 0.9f, 0.5f).ToUInt32();

            drawList.AddRectFilled(marginMin, marginMax, marginColor);
            drawList.AddRectFilled(borderMin, borderMax, borderColor);
            drawList.AddRectFilled(paddingMin, paddingMax, paddingColor);
            drawList.AddRectFilled(contentMin, contentMax, contentColor);
            
            // Draw Outlines
            uint lineCol = new ImTK.Color(1f, 1f, 1f, 0.8f).ToUInt32();
            drawList.AddRect(marginMin, marginMax, lineCol);
            drawList.AddRect(borderMin, borderMax, lineCol);
            drawList.AddRect(paddingMin, paddingMax, lineCol);
            drawList.AddRect(contentMin, contentMax, lineCol);

            // Function to draw text safely
            void DrawLabel(System.Numerics.Vector2 pos, string name, string val)
            {
                bool hasName = !string.IsNullOrEmpty(name);
                var h = new ImTKUtf8StringHandler(32, 2);
                if (hasName)
                {
                    h.AppendFormatted(name);
                    h.AppendLiteral(" ");
                }
                h.AppendFormatted(val);
                
                var size = RenderEngine.CalcTextSizeBuffered(ref h);
                var textPos = new System.Numerics.Vector2(pos.X - size.X / 2f, pos.Y - size.Y / 2f);
                
                var h2 = new ImTKUtf8StringHandler(32, 2);
                if (hasName)
                {
                    h2.AppendFormatted(name);
                    h2.AppendLiteral(" ");
                }
                h2.AppendFormatted(val);
                
                ImGui.SetCursorScreenPos(textPos);
                RenderEngine.TextBuffered(ref h2);
            }

            var rs = target.resolvedLayoutState;
            var tMargin = rs.margin;
            var tPadding = rs.padding;
            string borderStr = "-";
            if (target.resolvedStyle.TryGetFloat(StyleKey.BorderWidth.Hash, out float bw))
            {
                borderStr = bw.ToString("F1");
            }

            // Top
            DrawLabel(new System.Numerics.Vector2(center.X, marginMin.Y + stepY / 2f), "margin", tMargin.top.ToString("F1"));
            DrawLabel(new System.Numerics.Vector2(center.X, borderMin.Y + stepY / 2f), "border", borderStr);
            DrawLabel(new System.Numerics.Vector2(center.X, paddingMin.Y + stepY / 2f), "padding", tPadding.top.ToString("F1"));
            DrawLabel(center, $"{target.layoutRect.width:F1} x {target.layoutRect.height:F1}", "");

            // Left
            DrawLabel(new System.Numerics.Vector2(marginMin.X + stepX / 2f, center.Y), "", tMargin.left.ToString("F1"));
            DrawLabel(new System.Numerics.Vector2(borderMin.X + stepX / 2f, center.Y), "", borderStr);
            DrawLabel(new System.Numerics.Vector2(paddingMin.X + stepX / 2f, center.Y), "", tPadding.left.ToString("F1"));

            // Right
            DrawLabel(new System.Numerics.Vector2(marginMax.X - stepX / 2f, center.Y), "", tMargin.right.ToString("F1"));
            DrawLabel(new System.Numerics.Vector2(borderMax.X - stepX / 2f, center.Y), "", borderStr);
            DrawLabel(new System.Numerics.Vector2(paddingMax.X - stepX / 2f, center.Y), "", tPadding.right.ToString("F1"));

            // Bottom
            DrawLabel(new System.Numerics.Vector2(center.X, marginMax.Y - stepY / 2f), "", tMargin.bottom.ToString("F1"));
            DrawLabel(new System.Numerics.Vector2(center.X, borderMax.Y - stepY / 2f), "", borderStr);
            DrawLabel(new System.Numerics.Vector2(center.X, paddingMax.Y - stepY / 2f), "", tPadding.bottom.ToString("F1"));
        }
    }

    public enum StyleViewMode
    {
        All,
        ModifiedOnly
    }

    public class StyleVisualizer : VisualElement
    {
        private VisualElement m_target;
        private EnumDropdownDrawer<StyleViewMode> m_modeSelector;
        private VisualElement m_drawerContainer;

        public StyleVisualizer()
        {
            this.style.flexDirection = ImTK.UI.FlexDirection.Column;
            this.style.itemSpacing = new System.Numerics.Vector2(0, 5f);

            m_modeSelector = new EnumDropdownDrawer<StyleViewMode>();
            m_modeSelector.label = "View Mode";
            m_modeSelector.value = StyleViewMode.All;
            m_modeSelector.RegisterValueChangedCallback(evt => RebuildStyleDrawers());

            m_drawerContainer = new VisualElement();
            m_drawerContainer.style.flexDirection = ImTK.UI.FlexDirection.Column;
            m_drawerContainer.style.itemSpacing = new System.Numerics.Vector2(0, 2f);

            Add(m_modeSelector);
            Add(new ImTK.UI.Label("────────────────────────────────") { style = { colorFamily = ThemeColorFamily.Normal } });
            Add(m_drawerContainer);
        }

        public void SetTarget(VisualElement target)
        {
            m_target = target;
            RebuildStyleDrawers();
        }

        private void RebuildStyleDrawers()
        {
            m_drawerContainer.Clear();
            if (m_target == null) return;

            bool isModifiedOnly = m_modeSelector.value == StyleViewMode.ModifiedOnly;

            var styleType = m_target.style.GetType();
            var properties = styleType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var coreProps = new List<PropertyInfo>();
            var extProps = new List<PropertyInfo>();

            foreach (var prop in properties)
            {
                if (!prop.CanRead || !prop.CanWrite) continue;
                if (prop.DeclaringType == typeof(VisualElementStyle) || prop.DeclaringType == typeof(VisualElement.Style))
                {
                    coreProps.Add(prop);
                }
                else
                {
                    extProps.Add(prop);
                }
            }

            if (coreProps.Count > 0)
            {
                var coreHeader = new Label("Core Styles") { style = { textColor = ImTKTheme.GlobalTheme.normalColor.accent } };
                m_drawerContainer.Add(coreHeader);
                foreach (var p in coreProps) BuildDrawerForProperty(p, isModifiedOnly);
            }

            if (extProps.Count > 0)
            {
                var extHeader = new Label("Extended Styles") { style = { textColor = ImTKTheme.GlobalTheme.normalColor.accent, margin = new Thickness(10f, 0, 0, 0) } };
                m_drawerContainer.Add(extHeader);
                foreach (var p in extProps) BuildDrawerForProperty(p, isModifiedOnly);
            }
        }

        private void BuildDrawerForProperty(PropertyInfo prop, bool isModifiedOnly)
        {
            object propValue = null;
            try
            {
                propValue = prop.GetValue(m_target.style);
            }
            catch (Exception ex)
            {
                ImTKLog.Error(ex, $"Failed to get value for style property {prop.Name}");
                return;
            }

            if (isModifiedOnly)
            {
                if (propValue == null) return;
                var strVal = propValue.ToString();
                if (strVal == "Null" || strVal == "Unset" || strVal == "Auto") return;
            }

            var drawer = FieldDrawerFactory.Create()
                .FromType(prop.PropertyType)
                .Label(prop.Name)
                .AddModifiersFromMember(prop)
                .Build();

            if (drawer != null && drawer is VisualElement ve)
            {
                if (drawer is StyleColorDrawer colorDrawer)
                {
                    colorDrawer.defaultColorProvider = () =>
                    {
                        var name = prop.Name.ToLowerInvariant();
                        if (name.Contains("background") || name.Contains("surface"))
                            return ImGui.GetStyle().Colors[(int)ImGuiCol.ChildBg];
                        if (name.Contains("text"))
                            return ImGui.GetStyle().Colors[(int)ImGuiCol.Text];
                        if (name.Contains("border"))
                            return ImGui.GetStyle().Colors[(int)ImGuiCol.Border];
                        return Color.Black;
                    };
                }

                drawer.SetValueWithoutNotify(propValue);
                RegisterGenericCallback(ve, prop.PropertyType, prop);
                m_drawerContainer.Add(ve);
            }
        }

        private void RegisterGenericCallback(VisualElement element, Type valueType, PropertyInfo prop)
        {
            var method = GetType().GetMethod(nameof(RegisterGenericCallbackInternal), BindingFlags.NonPublic | BindingFlags.Instance);
            var genericMethod = method.MakeGenericMethod(valueType);
            genericMethod.Invoke(this, new object[] { element, prop });
        }

        private void RegisterGenericCallbackInternal<T>(VisualElement element, PropertyInfo prop)
        {
            element.RegisterCallback<ValueChangedEvent<T>>(evt => 
            {
                try
                {
                    prop.SetValue(m_target.style, evt.newValueObj);
                }
                catch (Exception ex)
                {
                    ImTKLog.Error(ex, $"Failed to set value for style property {prop.Name}");
                }
            });
        }
    }
}
