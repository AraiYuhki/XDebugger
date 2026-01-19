namespace Xeon.XDebugger.Model
{
    public class DefaultInitializePageModel : PageModel
    {
        private enum SampleMode
        {
            Alpha,
            Beta,
            Gamma
        }

        private const int DropdownOptionLow = 10;
        private const int DropdownOptionMid = 20;
        private const int DropdownOptionHigh = 30;

        private LabelModel statusLabel;
        private LabelModel refreshLabel;
        private DisableGroupModel disableGroup;
        private FoldingGroupModel foldingGroup;

        private string inputText = "Sample Text";
        private float numberValue = 1.5f;
        private float sliderValue = 0.5f;
        private int intSliderValue = 3;
        private bool toggleValue = true;
        private int dropdownIndex = 1;
        private int dropdownOptionValue = DropdownOptionMid;
        private SampleMode enumValue = SampleMode.Alpha;
        private int refreshCount;

        public DefaultInitializePageModel() : base("Initial Page")
        {
        }

        protected override void InitializeInternal()
        {
            AddLabel("PageModel Feature Check");
            refreshLabel = AddLabel($"Refresh Count : {refreshCount}");
            statusLabel = AddLabel(BuildStatusLabel());

            AddButton("Update Status Label", UpdateStatusLabel);
            AddButton("Refresh Controls", Refresh);

            AddPageLinkButton<MockNavigationPageModel>("Mock Navigation Page");
            AddPageLinkButton<SystemPageModel>("System Info");

            using (HorizontalScope("Layout Scope"))
            {
                using (VerticalScope("Left Column"))
                {
                    AddLabel("Left Scope - Label A");
                    AddLabel("Left Scope - Label B");
                }

                using (VerticalScope("Right Column"))
                {
                    AddLabel("Right Scope - Label A");
                    AddLabel("Right Scope - Label B");
                }
            }

            AddText("Input Text", inputText, value =>
            {
                inputText = value;
                UpdateStatusLabel();
            });

            AddNumber("Number", numberValue, 0.25f, value =>
            {
                numberValue = value;
                UpdateStatusLabel();
            });

            AddSlider("Float Slider", sliderValue, 0f, 1f, 2, value =>
            {
                sliderValue = value;
                UpdateStatusLabel();
            });

            AddIntSlider("Int Slider", intSliderValue, 0, 10, value =>
            {
                intSliderValue = value;
                UpdateStatusLabel();
            });

            AddToggle("Toggle", toggleValue, value =>
            {
                toggleValue = value;
                UpdateStatusLabel();
            });

            var dropdownLabels = new[] { "Low", "Mid", "High" };
            var dropdownOptions = new[] { DropdownOptionLow, DropdownOptionMid, DropdownOptionHigh };
            AddDropdown("Dropdown", dropdownIndex, dropdownLabels, dropdownOptions, value =>
            {
                dropdownOptionValue = value;
                UpdateStatusLabel();
            });

            AddEnumDropdown("Enum Dropdown", enumValue, value =>
            {
                enumValue = value;
                UpdateStatusLabel();
            });

            AddButton("Toggle Disable Group", () =>
            {
                if (disableGroup == null)
                    return;
                disableGroup.IsDisabled = !disableGroup.IsDisabled;
            });

            using (DisableScope(out disableGroup, "Disable Group"))
            {
                AddLabel("Disable Group - Label");
                AddButton("Disable Group Button", () =>
                {
                    refreshCount++;
                    refreshLabel.SetText($"Refresh Count : {refreshCount}");
                });
            }

            AddButton("Toggle Folding Group", () =>
            {
                if (foldingGroup == null)
                    return;
                foldingGroup.IsFolding = !foldingGroup.IsFolding;
            });

            using (FoldingScope(out foldingGroup, "Folding Group"))
            {
                AddLabel("Folding Group - Label");
                AddButton("Folding Group Button", () =>
                {
                    refreshCount++;
                    refreshLabel.SetText($"Refresh Count : {refreshCount}");
                });
            }
        }

        private void UpdateStatusLabel()
        {
            statusLabel?.SetText(BuildStatusLabel());
        }

        private string BuildStatusLabel()
        {
            return $"Status : Text={inputText}, Number={numberValue:0.##}, " +
                $"Slider={sliderValue:0.##}, IntSlider={intSliderValue}, " +
                $"Toggle={toggleValue}, Dropdown={dropdownOptionValue}, Enum={enumValue}";
        }
    }
}
