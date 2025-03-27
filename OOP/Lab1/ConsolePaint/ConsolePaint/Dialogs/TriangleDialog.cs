using Gtk;

namespace ConsolePaint.Dialogs;

public class TriangleDialog : Dialog
    {
        private readonly Entry _firstSideEntry;
        private readonly Entry _secondSideEntry;
        private readonly Entry _thirdSideEntry;

        public float FirstSide { get; private set; }
        public float SecondSide { get; private set; }
        public float ThirdSide { get; private set; }

        public TriangleDialog(Window parent) : base("Triangle Parameters", parent, DialogFlags.Modal)
        {
            SetDefaultSize(300, 200);
            BorderWidth = 10;

            // Main container
            var contentBox = new Box(Orientation.Vertical, 5);

            // First side
            var firstSideBox = new Box(Orientation.Horizontal, 5);
            firstSideBox.PackStart(new Label("First side:"), false, false, 0);
            _firstSideEntry = new Entry();
            firstSideBox.PackEnd(_firstSideEntry, true, true, 0);

            // Second side
            var secondSideBox = new Box(Orientation.Horizontal, 5);
            secondSideBox.PackStart(new Label("Second side:"), false, false, 0);
            _secondSideEntry = new Entry();
            secondSideBox.PackEnd(_secondSideEntry, true, true, 0);

            // Third side
            var thirdSideBox = new Box(Orientation.Horizontal, 5);
            thirdSideBox.PackStart(new Label("Third side:"), false, false, 0);
            _thirdSideEntry = new Entry();
            thirdSideBox.PackEnd(_thirdSideEntry, true, true, 0);

            // Add all to main container
            contentBox.PackStart(firstSideBox, true, true, 0);
            contentBox.PackStart(secondSideBox, true, true, 0);
            contentBox.PackStart(thirdSideBox, true, true, 0);

            // Buttons
            var okButton = new Button("OK");
            okButton.Clicked += OnOkClicked;

            var cancelButton = new Button("Cancel");
            cancelButton.Clicked += OnCancelClicked;

            var buttonBox = new Box(Orientation.Horizontal, 5);
            buttonBox.PackEnd(cancelButton, false, false, 0);
            buttonBox.PackEnd(okButton, false, false, 0);

            contentBox.PackEnd(buttonBox, false, false, 0);

            ContentArea.Add(contentBox);
            ShowAll();
        }

        private void OnOkClicked(object? sender, EventArgs args)
        {
            if (float.TryParse(_firstSideEntry.Text, out float firstSide) &&
                float.TryParse(_secondSideEntry.Text, out float secondSide) &&
                float.TryParse(_thirdSideEntry.Text, out float thirdSide))
            {
                if (IsValidTriangle(firstSide, secondSide, thirdSide))
                {
                    FirstSide = firstSide;
                    SecondSide = secondSide;
                    ThirdSide = thirdSide;
                    Respond(ResponseType.Ok);
                    return;
                }
            }

            ShowErrorDialog("Invalid triangle parameters");
        }

        private static bool IsValidTriangle(float a, float b, float c)
        {
            return a > 0 && b > 0 && c > 0 &&
                   a + b > c &&
                   a + c > b &&
                   b + c > a;
        }

        private void ShowErrorDialog(string message)
        {
            var errorDialog = new MessageDialog(
                this,
                DialogFlags.Modal,
                MessageType.Error,
                ButtonsType.Ok,
                message);

            errorDialog.Run();
            errorDialog.Destroy();
        }

        private void OnCancelClicked(object? sender, EventArgs args)
        {
            Respond(ResponseType.Cancel);
        }
    }
