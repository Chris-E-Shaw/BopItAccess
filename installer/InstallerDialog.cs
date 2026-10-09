using System.Drawing;

namespace BopItAccess.Installer;

/// <summary>Native, keyboard accessible choices that also support installer gamepad navigation.</summary>
internal sealed class InstallerDialog : Form
{
    private readonly TextBox _message = new();
    private readonly Dictionary<DialogResult, Button> _buttons = new();
    private readonly System.Windows.Forms.Timer? _refresh;
    private bool _choiceAnnounced;

    internal InstallerDialog(string title, string message,
        IEnumerable<(string Label, DialogResult Result)> choices, DialogResult defaultChoice,
        Func<string>? refreshMessage = null)
    {
        Text = title;
        AccessibleRole = AccessibleRole.Dialog;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        MinimizeBox = false;
        MaximizeBox = false;
        Size = new Size(590, 300);
        MinimumSize = new Size(460, 260);
        Font = SystemFonts.MessageBoxFont;
        AutoScaleMode = AutoScaleMode.Dpi;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), RowCount = 2, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _message.Multiline = true;
        _message.ReadOnly = true;
        _message.ScrollBars = ScrollBars.Vertical;
        _message.Dock = DockStyle.Fill;
        _message.AccessibleName = "Message";
        _message.AccessibleDescription = "Read-only message. Use arrow keys to review it.";
        _message.Text = message;
        _message.TabIndex = 0;
        var actions = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
        int index = 1;
        foreach (var choice in choices)
        {
            var button = new Button { Text = choice.Label, AutoSize = true, DialogResult = choice.Result, TabIndex = index++ };
            button.AccessibleName = choice.Label.Replace("&", "", StringComparison.Ordinal);
            button.Click += (_, _) =>
            {
                // Post synchronously on the still-visible dialog. A queued
                // callback would lose its provider when ShowDialog returns.
                _choiceAnnounced = true;
                _refresh?.Stop();
                InstallerFeedback.Announce(this, button.AccessibleName + ".", important: true);
            };
            actions.Controls.Add(button);
            _buttons.Add(choice.Result, button);
        }
        layout.Controls.Add(_message, 0, 0);
        layout.Controls.Add(actions, 0, 1);
        Controls.Add(layout);
        if (_buttons.TryGetValue(defaultChoice, out var selected)) AcceptButton = selected;
        if (_buttons.TryGetValue(DialogResult.Cancel, out var cancel)) CancelButton = cancel;
        else if (_buttons.TryGetValue(DialogResult.No, out var no)) CancelButton = no;
        else if (_buttons.TryGetValue(DialogResult.OK, out var okay)) CancelButton = okay;
        Shown += (_, _) =>
        {
            selected?.Focus();
            InstallerFeedback.Announce(this, _message.Text, important: true);
        };
        if (refreshMessage is not null)
        {
            _refresh = new System.Windows.Forms.Timer { Interval = 100 };
            _refresh.Tick += (_, _) =>
            {
                string next = refreshMessage();
                if (next == _message.Text) return;
                _message.Text = next;
                InstallerFeedback.Announce(this, next, important: true);
            };
            _refresh.Start();
        }
    }

    internal static void ShowMessage(Form owner, string message, string title = "Bop It Access Installer")
    {
        using var dialog = new InstallerDialog(title, message, new[] { ("&Okay", DialogResult.OK) }, DialogResult.OK);
        dialog.ShowDialog(owner);
    }

    internal static bool Confirm(Form owner, string question, string title)
    {
        using var dialog = new InstallerDialog(title, question,
            new[] { ("&Yes", DialogResult.Yes), ("&No", DialogResult.No) }, DialogResult.No);
        return dialog.ShowDialog(owner) == DialogResult.Yes;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _refresh?.Stop();
        base.OnFormClosing(e);
        if (e.Cancel || _choiceAnnounced) return;
        // Escape/controller Back and the title-bar close button also dismiss
        // a dialog. Report that response even without a managed Click event.
        string response = _buttons.TryGetValue(DialogResult, out var selected)
            ? selected.AccessibleName + "." : "Dialog closed.";
        InstallerFeedback.Announce(this, response, important: true);
        _choiceAnnounced = true;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _refresh?.Dispose();
        base.Dispose(disposing);
    }
}
