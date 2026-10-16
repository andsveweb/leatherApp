using Resend;

namespace leatherApp.Data;

public class EmailService
{
    private readonly IResend _resend;

    public EmailService(IResend resend)
    {
        _resend = resend;
    }

    public async Task SendOrderConfirmation(string toEmail, int orderId, decimal total)
    {
        var message = new EmailMessage();
        message.From = "Andreas Leather Co. <onboarding@resend.dev>";
        message.To.Add(toEmail);
        message.Subject = $"Order confirmation #{orderId}";
        message.HtmlBody = $@"
            <div style='font-family: sans-serif; color: #4A1B0C;'>
                <h2>Tack för din beställning!</h2>
                <p>Din beställning <strong>#{orderId}</strong> har mottagits.</p>
                <p>Totalt: <strong>{total:#,0} kr</strong></p>
                <p>Vi hör av oss när den skickas.</p>
                <hr />
                <p style='color:#8A5A2B;'>Andreas Leather Co.</p>
            </div>";

        await _resend.EmailSendAsync(message);
    }
}