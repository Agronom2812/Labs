using Gtk;

namespace ConsolePaint.Dialogs;

public sealed class RectangleDialog : Dialog
    {
        private readonly Entry _widthEntry;
        private readonly Entry _heightEntry;

        public float Width { get; private set; }
        public float Height { get; private set; }

        public RectangleDialog(Window parent) : base("Rectangle Parameters", parent, DialogFlags.Modal)
        {
            SetDefaultSize(300, 200);
            BorderWidth = 10;

            var contentBox = new Box(Orientation.Vertical, 5);

            var widthBox = new Box(Orientation.Horizontal, 5);
            widthBox.PackStart(new Label("Width:"), false, false, 0);
            _widthEntry = new Entry();
            widthBox.PackEnd(_widthEntry, true, true, 0);

            var heightBox = new Box(Orientation.Horizontal, 5);
            heightBox.PackStart(new Label("Height:"), false, false, 0);
            _heightEntry = new Entry();
            heightBox.PackEnd(_heightEntry, true, true, 0);

            contentBox.PackStart(widthBox, true, true, 0);
            contentBox.PackStart(heightBox, true, true, 0);

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
            if (float.TryParse(_widthEntry.Text, out float width) &&
                float.TryParse(_heightEntry.Text, out float height) &&
                width > 0 && height > 0)
            {
                Width = width;
                Height = height;
                Respond(ResponseType.Ok);
            }
            else
            {
                ShowErrorDialog("Width and height must be positive numbers");
            }
        }

        private void OnCancelClicked(object? sender, EventArgs args)
        {
            Respond(ResponseType.Cancel);
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
    }
