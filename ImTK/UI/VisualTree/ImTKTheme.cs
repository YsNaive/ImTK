using System;
using System.Numerics;
using ImGuiNET;

namespace ImTK.UI
{
    public class ImTKTheme
    {
        public ImTKTheme parent { get; set; }

        private Color? m_background1;
        private Color? m_background2;
        private Color? m_textPrimary;
        private Color? m_primaryColor;
        private float? m_drawerLabelWidth;

        public Color Background1
        {
            get
            {
                if (m_background1.HasValue) return m_background1.Value;
                if (parent != null) return parent.Background1;
                return new Color(0.1f, 0.1f, 0.1f, 1f); // Absolute fallback
            }
            set => m_background1 = value;
        }

        public Color Background2
        {
            get
            {
                if (m_background2.HasValue) return m_background2.Value;
                if (parent != null) return parent.Background2;
                return new Color(0.15f, 0.15f, 0.15f, 1f); // Absolute fallback
            }
            set => m_background2 = value;
        }

        public Color TextPrimary
        {
            get
            {
                if (m_textPrimary.HasValue) return m_textPrimary.Value;
                if (parent != null) return parent.TextPrimary;
                return Color.White; // Absolute fallback
            }
            set => m_textPrimary = value;
        }

        public Color PrimaryColor
        {
            get
            {
                if (m_primaryColor.HasValue) return m_primaryColor.Value;
                if (parent != null) return parent.PrimaryColor;
                return new Color(0.2f, 0.5f, 0.8f, 1f); // Absolute fallback
            }
            set => m_primaryColor = value;
        }

        public float DrawerLabelWidth
        {
            get
            {
                if (m_drawerLabelWidth.HasValue) return m_drawerLabelWidth.Value;
                if (parent != null) return parent.DrawerLabelWidth;
                return 150.0f; // Absolute fallback
            }
            set => m_drawerLabelWidth = value;
        }

        // --- Default Themes ---

        private static ImTKTheme s_defaultDark;
        public static ImTKTheme DefaultDark
        {
            get
            {
                if (s_defaultDark == null)
                {
                    s_defaultDark = new ImTKTheme()
                    {
                        m_background1 = new Color(0.06f, 0.06f, 0.06f, 1.0f),
                        m_background2 = new Color(0.11f, 0.11f, 0.11f, 1.0f),
                        m_textPrimary = new Color(1.0f, 1.0f, 1.0f, 1.0f),
                        m_primaryColor = new Color(0.15f, 0.35f, 0.65f, 1.0f),
                        m_drawerLabelWidth = 150.0f
                    };
                }
                return s_defaultDark;
            }
        }

        private static ImTKTheme s_defaultLight;
        public static ImTKTheme DefaultLight
        {
            get
            {
                if (s_defaultLight == null)
                {
                    s_defaultLight = new ImTKTheme()
                    {
                        m_background1 = new Color(0.9f, 0.9f, 0.9f, 1.0f),
                        m_background2 = new Color(0.8f, 0.8f, 0.8f, 1.0f),
                        m_textPrimary = new Color(0.0f, 0.0f, 0.0f, 1.0f),
                        m_primaryColor = new Color(0.4f, 0.6f, 0.9f, 1.0f),
                        m_drawerLabelWidth = 150.0f
                    };
                }
                return s_defaultLight;
            }
        }
    }
}
