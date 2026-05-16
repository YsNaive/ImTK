using System.Collections.Generic;
using ImTK.Core;

namespace ImTK.UI
{
    public class StyleSheet
    {
        private static StyleSheet s_global;
        public static StyleSheet Global
        {
            get
            {
                if (s_global == null)
                {
                    s_global = new StyleSheet();
                    DefaultStyles.Register(s_global);
                }
                return s_global;
            }
        }

        private Dictionary<int, StyleBlock> m_blocks = new Dictionary<int, StyleBlock>();

        public StyleBlock AddBlock(HashedString targetClass)
        {
            var block = new StyleBlock(targetClass);
            m_blocks[targetClass.Hash] = block;
            return block;
        }

        public bool TryGetBlock(HashedString targetClass, out StyleBlock block)
        {
            return m_blocks.TryGetValue(targetClass.Hash, out block);
        }
    }
}
