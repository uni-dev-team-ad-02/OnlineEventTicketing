using OnlineEventTicketing.Data.Entity;
using System.Net;
using System.Net.Mail;

namespace OnlineEventTicketing.Helpers
{
    public static class EmailHelper
    {
        public static async Task<bool> SendPurchaseInitiatedEmailAsync(
            ApplicationUser user,
            Event eventItem,
            int ticketQuantity,
            decimal totalAmount,
            IConfiguration configuration,
            ILogger logger)
        {
            var subject = $"Payment Processing - {eventItem.Title}";
            var body = GeneratePurchaseInitiatedEmailBody(user, eventItem, ticketQuantity, totalAmount);

            return await SendEmailAsync(user.Email!, subject, body, configuration, logger);
        }

        public static async Task<bool> SendTicketConfirmationEmailAsync(
            ApplicationUser user,
            Ticket ticket,
            Event eventItem,
            IConfiguration configuration,
            ILogger logger)
        {
            var subject = $"Your Ticket for {eventItem.Title}";
            var body = GenerateTicketEmailBody(user, ticket, eventItem);

            return await SendEmailAsync(user.Email!, subject, body, configuration, logger);
        }

        public static async Task<bool> SendTicketPurchaseConfirmationAsync(
            ApplicationUser user,
            Ticket ticket,
            Event eventItem,
            IConfiguration configuration,
            ILogger logger)
        {
            return await SendTicketConfirmationEmailAsync(user, ticket, eventItem, configuration, logger);
        }

        public static async Task<bool> SendEmailAsync(
            string toEmail,
            string subject,
            string body,
            IConfiguration configuration,
            ILogger logger)
        {
            try
            {
                var smtpHost = configuration["Email:SmtpHost"];
                var smtpPort = configuration.GetValue<int>("Email:SmtpPort", 587);
                var fromEmail = configuration["Email:FromEmail"];
                var fromName = configuration["Email:FromName"];
                var username = configuration["Email:Username"];
                var password = configuration["Email:Password"];
                var enableSsl = configuration.GetValue<bool>("Email:EnableSsl", true);

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(fromEmail) ||
                    string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    logger.LogWarning("Email configuration is incomplete. Email not sent.");
                    return false;
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = enableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName ?? "Online Event Ticketing"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                return false;
            }
        }

        private static string GeneratePurchaseInitiatedEmailBody(ApplicationUser user, Event eventItem, int ticketQuantity, decimal totalAmount)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .purchase-details {{ background-color: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .footer {{ background-color: #6c757d; color: white; padding: 15px; text-align: center; }}
        .processing {{ background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Payment Processing</h1>
        </div>

        <div class='content'>
            <p>Dear {user.FirstName} {user.LastName},</p>

            <p>Thank you for your ticket purchase! We are currently processing your payment.</p>

            <div class='purchase-details'>
                <h3>Purchase Details</h3>
                <p><strong>Event:</strong> {eventItem.Title}</p>
                <p><strong>Date:</strong> {eventItem.Date:MMM dd, yyyy 'at' h:mm tt}</p>
                <p><strong>Location:</strong> {eventItem.Location}</p>
                <p><strong>Tickets:</strong> {ticketQuantity}</p>
                <p><strong>Total Amount:</strong> {totalAmount:C}</p>
            </div>

            <div class='processing'>
                <h4>⏳ Payment Processing</h4>
                <p>Your payment is being processed through our secure payment system. This usually takes a few moments.</p>
                <p><strong>Once payment is confirmed, you will receive your ticket(s) with QR codes via email.</strong></p>
            </div>

            <p>You can track your order status by logging into your account. If you have any questions, please don't hesitate to contact us.</p>
        </div>

        <div class='footer'>
            <p>&copy; 2024 Online Event Ticketing. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private static string GenerateTicketEmailBody(ApplicationUser user, Ticket ticket, Event eventItem)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Your Ticket for {eventItem.Title}</title>
</head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background-size: cover; background-position: center;'>

    <table role='presentation' border='0' cellpadding='0' cellspacing='0' width='100%'>
    <tr>
        <td style='padding: 40px 20px;'>
            <table align='center' border='0' cellpadding='0' cellspacing='0' width='100%' style='max-width: 380px; border-collapse: collapse;'>
                <tr>
                    <td class='fallback-bg' style='background: linear-gradient(135deg, #1f1f47, #2c2c68); border: 1px solid rgba(255, 255, 255, 0.15); border-radius: 20px;'>

                        <table role='presentation' border='0' cellpadding='0' cellspacing='0' width='100%'>
                            <!-- Title -->
                            <tr>
                                <td style='padding: 30px 30px 20px 30px; text-align: center;'>
                                    <p style='text-transform: uppercase; font-size: 13px; color: #a0aec0; margin: 0; letter-spacing: 2px;'>You are attending</p>
                                    <h1 style='font-size: 30px; font-weight: 700; color: #ffffff; margin: 12px 0;'>{eventItem.Title}</h1>
                                </td>
                            </tr>

                            <!-- Date & Time -->
                            <tr>
                                <td style='padding: 10px 30px;'>
                                    <table role='presentation' border='0' cellpadding='0' cellspacing='0' width='100%'>
                                        <tr>
                                            <td width='50%' valign='top'>
                                                <p style='font-size: 13px; color: #a0aec0; margin: 0;'>DATE</p>
                                                <p style='font-size: 16px; color: #f9fafb; font-weight: 600; margin: 6px 0;'>{eventItem.Date:MMM dd, yyyy}</p>
                                            </td>
                                            <td width='50%' valign='top'>
                                                <p style='font-size: 13px; color: #a0aec0; margin: 0;'>TIME</p>
                                                <p style='font-size: 16px; color: #f9fafb; font-weight: 600; margin: 6px 0;'>{eventItem.Date:h:mm tt}</p>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan='2' style='padding-top: 15px;'>
                                                <p style='font-size: 13px; color: #a0aec0; margin: 0;'>VENUE</p>
                                                <p style='font-size: 16px; color: #f9fafb; font-weight: 600; margin: 6px 0;'>{eventItem.Location}</p>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>

                            <!-- Divider -->
                            <tr>
                                <td style='padding: 20px 30px;'>
                                    <div style='border-top: 2px dashed rgba(255, 255, 255, 0.2);'></div>
                                </td>
                            </tr>

                            <!-- QR + User Info -->
                            <tr>
                                <td align='center' style='padding: 0 30px 30px 30px;'>
                                    <div style='background: #ffffff; padding: 20px; border-radius: 10px; display: inline-block;'>
                                        <p style='font-size: 24px; font-family: monospace; color: #333; margin: 0; text-align: center; word-break: break-all;'>{ticket.QrCode}</p>
                                    </div>
                                    <p style='font-size: 17px; color: #ffffff; font-weight: 600; margin: 18px 0 0 0;'>{user.FirstName} {user.LastName}</p>
                                    <p style='font-size: 12px; font-family: ""Courier New"", monospace; color: #a0aec0; margin: 5px 0 0 0;'>ID: {ticket.Id}</p>
                                </td>
                            </tr>
                        </table>

                    </td>
                </tr>
            </table>
        </td>
    </tr>
</table>

</body>
</html>";
        }

        private static string GenerateTicketConfirmationEmailBody(ApplicationUser user, Ticket ticket, Event eventItem)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .ticket-details {{ background-color: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .footer {{ background-color: #6c757d; color: white; padding: 15px; text-align: center; }}
        .qr-code {{ text-align: center; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Ticket Purchase Confirmation</h1>
        </div>

        <div class='content'>
            <p>Dear {user.FirstName} {user.LastName},</p>

            <p>Thank you for your purchase! Your ticket has been confirmed. Here are the details:</p>

            <div class='ticket-details'>
                <h3>Event Information</h3>
                <p><strong>Event:</strong> {eventItem.Title}</p>
                <p><strong>Date:</strong> {eventItem.Date:MMM dd, yyyy 'at' h:mm tt}</p>
                <p><strong>Location:</strong> {eventItem.Location}</p>
                <p><strong>Category:</strong> {eventItem.Category}</p>

                <h3>Ticket Information</h3>
                <p><strong>Ticket ID:</strong> {ticket.Id}</p>
                <p><strong>Price:</strong> {ticket.Price:C}</p>
                <p><strong>Seat Number:</strong> {(string.IsNullOrEmpty(ticket.SeatNumber) ? "General Admission" : ticket.SeatNumber)}</p>
                <p><strong>Purchase Date:</strong> {ticket.PurchaseDate:MMM dd, yyyy 'at' h:mm tt}</p>
                <p><strong>Status:</strong> {ticket.Status}</p>

                <div class='qr-code'>
                    <p><strong>QR Code:</strong> {ticket.QrCode}</p>
                    <p><small>Present this QR code at the event entrance</small></p>
                </div>
            </div>

            <p>Please save this email for your records. You can also access your tickets anytime by logging into your account.</p>

            <p>If you have any questions, please don't hesitate to contact us.</p>

            <p>We look forward to seeing you at the event!</p>
        </div>

        <div class='footer'>
            <p>&copy; 2024 Online Event Ticketing. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}