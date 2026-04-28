using DCRManagement.Domain.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace DCRManagement.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    private string SmtpHost => _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
    private int SmtpPort => int.Parse(_configuration["Email:SmtpPort"] ?? "587");
    private string SenderEmail => _configuration["Email:SenderEmail"] ?? string.Empty;
    private string SenderName => _configuration["Email:SenderName"] ?? "DCR System";
    private string Password => _configuration["Email:Password"] ?? string.Empty;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(SenderName, SenderEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(SmtpHost, SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(SenderEmail, Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {To} | Subject: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            // Log but don't throw — email failure must not block the approval workflow
            _logger.LogError(ex, "Failed to send email to {To}", to);
        }
    }

    public async Task SendDCRStatusChangedAsync(string toEmail, string toName,
        string dcrNumber, string newStatus, string? comment)
    {
        var subject = $"[DCR System] {dcrNumber} — Status changed to {newStatus}";
        var html = $"""
            <h3>DCR Status Update</h3>
            <p>Dear {toName},</p>
            <p>The DCR <strong>{dcrNumber}</strong> has been updated to status: 
               <strong>{newStatus}</strong>.</p>
            {(comment is not null ? $"<p><em>Comment: {comment}</em></p>" : "")}
            <p>Please log in to the DCR system for details.</p>
            <hr/>
            <small>This is an automated notification. Do not reply.</small>
            """;

        await SendAsync(toEmail, subject, html);
    }
}