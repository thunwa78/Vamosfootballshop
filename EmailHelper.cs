using MailKit.Net.Smtp;
using MimeKit;


public class EmailHelper
{
    public static void SendVerificationEmail(string toEmail, string username, string code)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("VAMOS Football SHOP", "vamosfootballshop@gmail.com")); // ใส่อีเมลจริง
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = "Reset Password Code";

        email.Body = new TextPart("plain")
        {
            Text = $"Hi {username},\n\nYour password reset verification code is: {code}"
        };

        using (var smtp = new MailKit.Net.Smtp.SmtpClient())
        {
            smtp.Connect("smtp.gmail.com", 587, false);
            smtp.Authenticate("vamosfootballshop@gmail.com", "izglcoyfguhjeibl"); // ใช้ App Password
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
