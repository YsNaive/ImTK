using System;
using System.Linq;

namespace ImTK.UI
{
    [CustomFieldDrawer(typeof(Enum), allowInheritType: true)]
    public class EnumDropdownDrawer<T> : DropdownDrawer<T> where T : struct, Enum
    {
        public EnumDropdownDrawer()
        {
            this.formatOption = opt => opt.ToString();
            
            // 由於泛型已約束為 Enum，我們能在建構時直接擷取所有列舉值，無需等待 value 賦值
            this.options = (T[])Enum.GetValues(typeof(T));
        }
    }
}
