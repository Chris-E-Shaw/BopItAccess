namespace BopItAccess.Installer;

/// <summary>Reviewable native text with a short, interrupting focus summary.</summary>
internal sealed class InstallerReviewTextBox : TextBox
{
    private long _focusGeneration;
    private long _announcedGeneration = -1;
    private bool _focusAnnouncementPending;

    internal InstallerReviewTextBox(string name, string shortcut)
    {
        AccessibleName = name;
        AccessibleDescription = string.Empty;
        FocusAnnouncement = $"{name} Multi line. Read-only. {shortcut}.";
    }

    internal string FocusAnnouncement { get; }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        _focusGeneration++;
        AnnounceFocus();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        _focusGeneration++;
        _focusAnnouncementPending = false;
        base.OnLostFocus(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        SuppressFocusAnnouncement();
        base.OnKeyDown(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        SuppressFocusAnnouncement();
        base.OnMouseDown(e);
    }

    private void SuppressFocusAnnouncement()
    {
        // Reviewing at a boundary or copying can leave the caret unchanged.
        // The user's action still takes priority over a queued focus summary,
        // including a later startup-focus confirmation for this same visit.
        _focusGeneration++;
        _announcedGeneration = _focusGeneration;
        _focusAnnouncementPending = false;
    }

    internal void AnnounceFocus()
    {
        if (!Focused || IsDisposed || Disposing || !IsHandleCreated ||
            _focusAnnouncementPending || _announcedGeneration == _focusGeneration) return;
        long focusGeneration = _focusGeneration;
        long speechGeneration = InstallerFeedback.PostedGeneration;
        int caret = SelectionStart;
        int selection = SelectionLength;
        _focusAnnouncementPending = true;
        try
        {
            // Replace the native focus speech after it arrives. Keep the full
            // native text/value available for cursor review, selection and copy.
            BeginInvoke((Action)(() =>
            {
                if (focusGeneration != _focusGeneration) return;
                _focusAnnouncementPending = false;
                if (!Focused || IsDisposed || Disposing || SelectionStart != caret ||
                    SelectionLength != selection || InstallerFeedback.PostedGeneration != speechGeneration) return;
                if (InstallerFeedback.Announce(this, FocusAnnouncement, important: true))
                    _announcedGeneration = focusGeneration;
            }));
        }
        catch (InvalidOperationException) when (IsDisposed || Disposing || !IsHandleCreated)
        { _focusAnnouncementPending = false; }
    }
}
