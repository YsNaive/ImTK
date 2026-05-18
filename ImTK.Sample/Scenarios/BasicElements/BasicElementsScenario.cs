using System;
using ImTK.Sample.Framework;
using ImTK.UI;

namespace ImTK.Sample.Scenarios.BasicElements
{
    public class BasicElementsScenario : ISampleScenario
    {
        public string ScenarioName => "Basic Elements";
        public string Description => "Demonstrates TextElement, CheckBox, and TextField.";
        public string DocumentationPath => "Scenarios/BasicElements/README.md";

        public void Open()
        {
            Window.Open<BasicElementsDemoWindow>();
        }
    }

    public class BasicElementsDemoWindow : Window
    {
        public BasicElementsDemoWindow() : base("Basic Elements Demo")
        {
            var textElement = new TextElement("這是一個 TextElement");

            var checkBox = new CheckBox("同意條款", false);
            var checkResultText = new TextElement("目前的狀態: False");
            checkBox.onValueChanged += evt => checkResultText.text = $"目前的狀態: {evt.newValue}";

            var textField = new TextField("使用者名稱", "User");
            var textResultText = new TextElement("您輸入了: User");
            textField.onValueChanged += evt => textResultText.text = $"您輸入了: {evt.newValue}";

            Add(textElement);
            Add(new TextElement("-----------------"));
            Add(checkBox);
            Add(checkResultText);
            Add(new TextElement("-----------------"));
            Add(textField);
            Add(textResultText);
        }
    }
}
