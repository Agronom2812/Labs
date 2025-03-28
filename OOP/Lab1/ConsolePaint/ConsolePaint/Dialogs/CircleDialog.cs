using Gtk;

namespace ConsolePaint.Dialogs;

public sealed class CircleDialog : Dialog
    {
        private readonly Entry _radiusEntry;

        public float Radius { get; private set; }

        public CircleDialog(Window parent) : base("Circle Parameters", parent, DialogFlags.Modal)
        {
            SetDefaultSize(300, 150);
            BorderWidth = 10;

            var contentBox = new Box(Orientation.Vertical, 5);

            var radiusBox = new Box(Orientation.Horizontal, 5);
            radiusBox.PackStart(new Label("Radius:"), false, false, 0);
            _radiusEntry = new Entry();
            radiusBox.PackEnd(_radiusEntry, true, true, 0);

            contentBox.PackStart(radiusBox, true, true, 0);

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
            if (float.TryParse(_radiusEntry.Text, out float radius) && radius > 0)
            {
                Radius = radius;
                Respond(ResponseType.Ok);
            }
            else
            {
                ShowErrorDialog("Radius must be a positive number");
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
