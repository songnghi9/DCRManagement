namespace DCRManagement.Domain.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody);
    Task SendDCRStatusChangedAsync(string toEmail, string toName,
        string dcrNumber, string newStatus, string? comment);
}