namespace DCRManagement.UI.Common;

public class BaseForm : Form, IView
{
    public virtual void ShowError(string message, string title)
    {
        if (InvokeRequired)
            Invoke(() => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error));
        else
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public virtual void ShowInfo(string message, string title)
    {
        if (InvokeRequired)
            Invoke(() => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information));
        else
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public virtual bool Confirm(string message, string title)
    {
        if (InvokeRequired)
            return (bool)Invoke(new Func<bool>(() => 
                MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    == DialogResult.Yes))!;
        else
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                == DialogResult.Yes;
    }

    public virtual void SetBusy(bool isBusy) =>
        Cursor = isBusy ? Cursors.WaitCursor : Cursors.Default;

    protected void InvokeIfRequired(Action action)
    {
        if (InvokeRequired) Invoke(action);
        else action();
    }

    protected async Task RunAsync(Func<Task> action, string operationName)
    {
        SetBusy(true);
        try { await action(); }
        catch (Exception ex)
        {
            ShowError($"Error during {operationName}: {ex.Message}", "Error");
        }
        finally { SetBusy(false); }
    }
}