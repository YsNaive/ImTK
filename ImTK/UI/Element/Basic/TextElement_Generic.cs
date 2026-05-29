using System;
using Hexa.NET.ImGui;
using ImTK.Core;

namespace ImTK.UI
{
    public class TextElement<TStyle> : VisualElement<TStyle> where TStyle : IVisualElementStyle, new()
    {
        private bool m_enableWordWrap = true;
        public bool enableWordWrap
        {
            get => m_enableWordWrap;
            set
            {
                if (m_enableWordWrap != value)
                {
                    m_enableWordWrap = value;
                    MarkMeasureDirty();
                    MarkArrangeDirty();
                }
            }
        }

        protected NativeUtf8Buffer m_textBuffer = new NativeUtf8Buffer();
        protected string m_cachedText = string.Empty;

        public string text
        {
            get 
            {
                if (m_cachedText == null)
                {
                    m_cachedText = m_textBuffer.ToString();
                }
                return m_cachedText;
            }
            set 
            {
                if (m_cachedText == value) return;
                
                m_cachedText = value ?? string.Empty;
                m_textBuffer.SetString(m_cachedText);
                
                MarkMeasureDirty();
                MarkArrangeDirty();
            }
        }

        public void SetTextBuffered(ref ImTKUtf8StringHandler handler)
        {
            if (m_textBuffer.Length == handler.WrittenCount)
            {
                unsafe
                {
                    if (new ReadOnlySpan<byte>((byte*)m_textBuffer.Data, m_textBuffer.Length).SequenceEqual(handler.WrittenSpan))
                    {
                        handler.Dispose();
                        return; // Unchanged
                    }
                }
            }
            
            m_textBuffer.SetText(handler.WrittenSpan);
            m_cachedText = null;
            handler.Dispose();
            
            MarkMeasureDirty();
            MarkArrangeDirty();
        }

        public void SetTextBuffered(ReadOnlySpan<byte> utf8Text)
        {
            if (m_textBuffer.Length == utf8Text.Length)
            {
                unsafe
                {
                    if (new ReadOnlySpan<byte>((byte*)m_textBuffer.Data, m_textBuffer.Length).SequenceEqual(utf8Text))
                    {
                        return; // Unchanged
                    }
                }
            }

            m_textBuffer.SetText(utf8Text);
            m_cachedText = null;
            
            MarkMeasureDirty();
            MarkArrangeDirty();
        }
        
        public void SetTextBuffered(string text)
        {
            this.text = text;
        }

        protected override System.Numerics.Vector2 MeasureContent(LayoutConstraint constraint)
        {
            if (m_textBuffer.IsEmpty) return System.Numerics.Vector2.Zero;
            
            if (enableWordWrap && constraint.WidthMode != MeasureMode.Undefined && constraint.AvailableWidth > 0)
            {
                System.Numerics.Vector2 size;
                unsafe {
                    size = ImGui.CalcTextSize((byte*)m_textBuffer.Data, (byte*)null, false, constraint.AvailableWidth);
                }
                return size;
            }
            else
            {
                System.Numerics.Vector2 size;
                unsafe {
                    size = ImGui.CalcTextSize((byte*)m_textBuffer.Data);
                }
                return new System.Numerics.Vector2(size.X, ImGui.GetTextLineHeight());
            }
        }
    }
}
