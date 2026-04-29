namespace DCRManagement.UI.Common;

public interface IView
{
    void ShowError(string message, string title);
    void ShowInfo(string message, string title);
    bool Confirm(string message, string title);
    void SetBusy(bool isBusy);
}