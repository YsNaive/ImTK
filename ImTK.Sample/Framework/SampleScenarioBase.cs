using System;
using System.Collections.Generic;

namespace ImTK.Sample.Framework
{
    /// <summary>
    /// 提供 ISampleScenario 的基礎實作，可自動推導文件路徑與預設屬性，減少開發樣板代碼。
    /// </summary>
    public abstract class SampleScenarioBase : ISampleScenario
    {
        public virtual string ScenarioName
        {
            get
            {
                // 如果未提供名稱，預設將型別名稱的 "Scenario" 字尾拿掉
                string name = GetType().Name;
                if (name.EndsWith("Scenario"))
                {
                    name = name.Substring(0, name.Length - "Scenario".Length);
                }
                return name;
            }
        }

        public abstract string Description { get; }

        public virtual string DocumentationPath
        {
            get
            {
                // 自動推導路徑:
                // 假設型別為 ImTK.Sample.Scenarios.CategoryName.ScenarioName
                // 路徑預設為 Scenarios/CategoryName/README.md
                // 如果沒有巢狀目錄，則 fallback
                string ns = GetType().Namespace ?? "";
                string prefix = "ImTK.Sample.";
                if (ns.StartsWith(prefix))
                {
                    string path = ns.Substring(prefix.Length).Replace('.', '/');
                    return $"{path}/README.md";
                }

                return "";
            }
        }

        public virtual string Category => "Uncategorized";

        public virtual int Order => 100;

        public virtual IEnumerable<Type> SeeAlso => Array.Empty<Type>();

        public abstract void Open();
    }
}
