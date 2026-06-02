using System;
using System.Collections.Generic;
using ImTK.Sample.Framework;
using ImTK.UI;
using ImTK.Core;
using System.Numerics;

namespace ImTK.Sample.Scenarios.StylingDX
{
    public class StylingDXScenario : SampleScenarioBase
    {
        public override string Description => "Demonstrates the Developer Experience (DX) syntactic sugar for styling VisualElements, including ColorFamily, Spacing, Thickness, FontSize, and Color implicit casts.";

        public override string Category => "UI System";
        public override int Order => 20;

        public override string DocumentationPath => "Scenarios/StylingDX/README.md";

        public override Window Open()
        {
            return Window.Open<StylingDXWindow>();
        }
    }

    public class StylingDXWindow : Window
    {
        public StylingDXWindow() : base("Styling DX Demo")
        {
            // Set window basic styles
            style.padding = 20; // Implicitly converts to a 20px uniform padding (StyleThickness)
            style.itemSpacing = 15; // Implicitly converts to Vector2(15, 15) (StyleSpacing)
            style.colorFamily = ThemeColorFamily.Info; // Automatically sets surface, text, and border

            var description = new TextElement("This window uses VisualElement objects to demonstrate the new Styling DX.");
            // Use FontSize enum (StyleFontSize)
            description.style.fontSize = FontSize.H2;
            description.style.padding = new Thickness(0, 0, 0, 10);
            // The text color will be inherited from the window's Info colorFamily!

            var btnDanger = new Button("Delete Item");
            // Automatically maps Background, Hover, Active, and Text colors to the Danger theme!
            btnDanger.style.colorFamily = ThemeColorFamily.Danger; 
            btnDanger.style.padding = new Vector2(20, 10); // Horizontal 20, Vertical 10
            btnDanger.style.fontSize = 18; // Implicitly sets absolute pixel size and scales font

            var btnSuccess = new Button("Confirm Action");
            btnSuccess.style.colorFamily = ThemeColorFamily.Success;
            // Use hex code for explicit color override (StyleColor)
            btnSuccess.style.textColor = "#FFFF00"; 
            
            var customField = new Label("Hex Color Text");
            customField.style.backgroundColor = 0xFF333333; // uint color assignment
            customField.style.textColor = Color.Magenta; // ImTK.Core.Color assignment

            // Test Inheritable HighLevelToken
            var dangerArea = new VisualElement();
            dangerArea.style.padding = 10;
            // Set ColorFamily=Danger and mark it as inheritable!
            dangerArea.style.SetProperty(new StyleProperty { 
                category = StyleCategory.HighLevelToken, 
                key = StyleKey.ColorFamily.Hash, 
                dataType = StyleDataType.Enum, 
                enumValue = (int)ThemeColorFamily.Danger, 
                isInheritable = true 
            });

            var inheritedBtn1 = new Button("Inherited Danger 1");
            var inheritedBtn2 = new Button("Inherited Danger 2");
            var inheritedText = new TextElement("This text is also Danger colored.");
            
            dangerArea.Add(inheritedBtn1);
            dangerArea.Add(inheritedBtn2);
            dangerArea.Add(inheritedText);

            Add(description);
            Add(btnDanger);
            Add(btnSuccess);
            Add(customField);
            Add(dangerArea);
        }

        public override void OnRender()
        {
            // Let the VisualElements render themselves through the layout engine
        }
    }
}
