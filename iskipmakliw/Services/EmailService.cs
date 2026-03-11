using iskipmakliw.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

namespace iskipmakliw.Services
{
    public class EmailService
    {
        private readonly string _smtpHost = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _fromEmail = "yhujiinn@gmail.com";
        private readonly string _fromPassword = "vgxx fish qxgq liut";
        private readonly string _fromName = "OakMart";
        private readonly ApplicationDbContext _context;

        public EmailService(ApplicationDbContext context)
        {
            _context = context;
        }
        public bool SendSellerStatusEmail(string toEmail, string status, string? declinedReason, string username)
        {
            try
            {
                string subject = status switch
                {
                    "Approved" => "🎉 Your Seller Application Has Been Approved!",
                    "Declined" => "Your Seller Application Status Update",
                    _ => "Your Seller Application Is Under Review"
                };

                var statusColor = status switch
                {
                    "Approved" => "#16a34a",
                    "Declined" => "#dc2626",
                    _ => "#667eea"
                };

                var statusIcon = status switch
                {
                    "Approved" => "✅",
                    "Declined" => "❌",
                    _ => "⏳"
                };

                var statusHeading = status switch
                {
                    "Approved" => "Congratulations! Your application has been approved.",
                    "Declined" => "Unfortunately, your application was not approved.",
                    _ => "Your application is currently under review."
                };

                var statusMessage = status switch
                {
                    "Approved" => "Your seller application has been reviewed and accepted. To activate your seller account, please proceed with your subscription payment.",
                    "Declined" => "After reviewing your application, we are unable to approve your seller account at this time.",
                    _ => "Our team is currently reviewing your seller application. We will notify you once a decision has been made."
                };

                // Declined reason — only if declined and reason exists
                var declinedBlock = (status == "Declined" && !string.IsNullOrEmpty(declinedReason))
                    ? $@"
        <div style='background-color: #fef2f2; border-left: 4px solid #dc2626; padding: 14px 16px; margin: 20px 0;'>
            <strong style='color: #991b1b;'>Reason for Decline:</strong>
            <p style='margin: 6px 0 0; color: #7f1d1d; font-size: 13px;'>{declinedReason}</p>
        </div>"
                    : "";

                // Approved: subscription CTA | Declined: final notice | Pending: timeline info
                var nextStepsBlock = status switch
                {
                    "Approved" => @"
        <div style='background-color: #f0fdf4; border-left: 4px solid #16a34a; padding: 14px 16px; margin: 20px 0;'>
            <strong style='color: #166534;'>💳 One More Step — Subscribe to Start Selling</strong>
            <p style='margin: 6px 0 0; color: #15803d; font-size: 13px;'>
                Your account is approved but not yet active. Please complete your subscription 
                payment to unlock your Seller Dashboard and start listing products.
            </p>
        </div>
        <div style='text-align: center; margin: 28px 0;'>
            <a href='#' style='background-color: #16a34a; color: #ffffff; text-decoration: none;
               padding: 14px 36px; border-radius: 6px; font-weight: bold; font-size: 15px;
               display: inline-block; letter-spacing: 0.3px;'>
                Pay Subscription Now
            </a>
            <p style='margin: 12px 0 0; color: #999; font-size: 12px;'>
                Click the button above to proceed to the payment page.
            </p>
        </div>",

                    "Declined" => @"
        <div style='background-color: #fef2f2; border-left: 4px solid #dc2626; padding: 14px 16px; margin: 20px 0;'>
            <strong style='color: #991b1b;'>⚠️ Application Closed</strong>
            <p style='margin: 6px 0 0; color: #7f1d1d; font-size: 13px;'>
                This application has been reviewed and will not be processed further. 
                Thank you for your interest.
            </p>
        </div>",

                    _ => @"
        <div style='background-color: #f0f4ff; border-left: 4px solid #667eea; padding: 14px 16px; margin: 20px 0;'>
            <strong style='color: #3730a3;'>📋 What happens next?</strong>
            <p style='margin: 6px 0 0; color: #4338ca; font-size: 13px;'>
                Our team typically reviews applications within 1–3 business days. 
                You will receive another email once a decision has been made.
            </p>
        </div>"
                };

                string body = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>

        <h2 style='color: #333;'>Seller Application Update</h2>
        <p>Hi <strong>{username}</strong>, here is an update regarding your seller application.</p>

        <!-- Status Box -->
        <div style='background-color: #f4f4f4; padding: 20px; text-align: center; margin: 20px 0;'>
            <p style='margin: 0; color: #666; font-size: 14px;'>Application Status</p>
            <h1 style='color: {statusColor}; font-size: 48px; margin: 8px 0;'>{statusIcon}</h1>
            <h2 style='color: {statusColor}; font-size: 24px; margin: 0; letter-spacing: 1px;'>{status.ToUpper()}</h2>
        </div>

        <!-- Status Summary -->
        <div style='background-color: #f9f9f9; border-left: 4px solid {statusColor}; padding: 14px 16px; margin: 20px 0;'>
            <strong style='color: {statusColor};'>{statusHeading}</strong>
            <p style='margin: 6px 0 0; color: #555; font-size: 13px;'>{statusMessage}</p>
        </div>

        {declinedBlock}

        {nextStepsBlock}

        <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>
        <p style='color: #666; font-size: 12px;'>This is an automated message, please do not reply.</p>
    </div>
</body>
</html>";

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(_fromEmail, _fromName);
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;
                    using (SmtpClient smtp = new SmtpClient(_smtpHost, _smtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(_fromEmail, _fromPassword);
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }
        public bool SendSuccessDeliveryEmail(string toEmail, int purchasedProductId, string source)
        {
            try
            {
                string subject = "Thank you for ordering!";
                if(source == "normal")
                {
                    var purchased = _context.PurchasedProduct
                    .Include(p => p.ProductVariants)
                        .ThenInclude(pv => pv.Product)
                    .Include(p => p.Billings)
                    .Include(p => p.Users)
                    .FirstOrDefault(p => p.Id == purchasedProductId);
                    if (purchased == null) return false;

                    var productName = purchased.ProductVariants?.Product?.Name ?? "Product";
                    var variantName = purchased.ProductVariants?.Product?.Name ?? "Product";
                    var quantity = purchased.Quantity;
                    var price = purchased.Price ?? 0;
                    var shipping = purchased.ShippingFee ?? 0;
                    var grandTotal = price + shipping;
                    var orderDate = purchased.PurchasedDate.ToString("MMMM dd, yyyy hh:mm tt");
                    var orderRef = $"ORD-{purchased.PurchasedDate:yyyyMMdd}-{purchased.Id}";
                    var firstName = purchased.Users?.Username ?? toEmail;
                    var paymentMethod = purchased.PaymentMethod ?? "Cash on Delivery";
                    var billing = purchased.Billings;
                    var billingAddr = billing != null
                        ? $"{billing.Name}, {billing.Address}, {billing.City}, {billing.Country}"
                        : "N/A";

                    string body = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>

        <h2 style='color: #333;'>Order Confirmation</h2>
        <p>Hi <strong>{firstName}</strong>, thank you for your purchase! Your order has been confirmed and is now being prepared for delivery.</p>

        <!-- Order Reference Box -->
        <div style='background-color: #f4f4f4; padding: 20px; text-align: center; margin: 20px 0;'>
            <p style='margin: 0; color: #666; font-size: 14px;'>Order Reference</p>
            <h1 style='color: #667eea; font-size: 32px; margin: 8px 0; letter-spacing: 4px;'>{orderRef}</h1>
            <p style='margin: 0; color: #666; font-size: 13px;'>{orderDate}</p>
        </div>

        <!-- What happens next -->
        <div style='background-color: #f0f4ff; border-left: 4px solid #667eea; padding: 14px 16px; margin: 20px 0;'>
            <strong style='color: #3730a3;'>🚚 What happens next?</strong>
            <p style='margin: 6px 0 0; color: #4338ca; font-size: 13px;'>
                Your order is being packed and will be handed to our delivery team. 
                Please make sure someone is available at the delivery address to receive it.
            </p>
        </div>

        <!-- Order Details -->
        <table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>
            <thead>
                <tr style='background-color: #f4f4f4;'>
                    <th style='text-align: left; padding: 10px; font-size: 13px; color: #666; border-bottom: 2px solid #ddd;'>Product</th>
                    <th style='text-align: center; padding: 10px; font-size: 13px; color: #666; border-bottom: 2px solid #ddd;'>Qty</th>
                    <th style='text-align: right; padding: 10px; font-size: 13px; color: #666; border-bottom: 2px solid #ddd;'>Total</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td style='padding: 12px 10px; border-bottom: 1px solid #ddd;'>
                        <strong style='color: #333;'>{productName}</strong>
                        {(string.IsNullOrEmpty(variantName) ? "" : $"<br><span style='font-size: 12px; color: #999;'>{variantName}</span>")}
                    </td>
                    <td style='padding: 12px 10px; text-align: center; color: #333; border-bottom: 1px solid #ddd;'>{quantity}</td>
                    <td style='padding: 12px 10px; text-align: right; color: #333; border-bottom: 1px solid #ddd;'>&#8369;{price:N2}</td>
                </tr>
            </tbody>
            <tfoot>
                <tr>
                    <td colspan='2' style='padding: 8px 10px; text-align: right; color: #666; font-size: 13px;'>Shipping Fee:</td>
                    <td style='padding: 8px 10px; text-align: right; color: #333; font-size: 13px;'>&#8369;{shipping:N2}</td>
                </tr>
                <tr>
                    <td colspan='2' style='padding: 8px 10px; text-align: right; font-weight: bold; color: #333;'>Grand Total:</td>
                    <td style='padding: 8px 10px; text-align: right; font-weight: bold; color: #667eea; font-size: 18px;'>&#8369;{grandTotal:N2}</td>
                </tr>
            </tfoot>
        </table>

        <!-- Delivery Address -->
        <p><strong>Delivery Address:</strong><br>
            <span style='color: #666;'>{billingAddr}</span>
        </p>

        <p><strong>Payment Method:</strong> <span style='color: #666;'>{paymentMethod}</span></p>

        <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>
        <p style='color: #666; font-size: 12px;'>This is an automated message, please do not reply.</p>
    </div>
</body>
</html>";

                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress(_fromEmail, _fromName);
                        mail.To.Add(toEmail);
                        mail.Subject = subject;
                        mail.Body = body;
                        mail.IsBodyHtml = true;
                        using (SmtpClient smtp = new SmtpClient(_smtpHost, _smtpPort))
                        {
                            smtp.Credentials = new NetworkCredential(_fromEmail, _fromPassword);
                            smtp.EnableSsl = true;
                            smtp.Send(mail);
                        }
                    }
                    return true;
                }
                else
                {
                    var customOrder = _context.CustomizationOrders
                        .Include(c => c.Users)
                        .FirstOrDefault(c => c.Id == purchasedProductId);

                    if (customOrder == null) return false;

                    var firstName = customOrder.Users?.Username ?? toEmail;
                    var orderDate = customOrder.DateCreated.ToString("MMMM dd, yyyy hh:mm tt");
                    var orderRef = $"CUSTOM-{customOrder.DateCreated:yyyyMMdd}-{customOrder.Id}";
                    var paymentMethod = customOrder.ModeOfPayment ?? "N/A";
                    var price = customOrder.Price ?? 0;

                    string body = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>

        <h2 style='color: #333;'>Customization Order Confirmed</h2>
        <p>Hi <strong>{firstName}</strong>, your customization order has been received and is now being reviewed by the seller.</p>

        <!-- Order Reference Box -->
        <div style='background-color: #f4f4f4; padding: 20px; text-align: center; margin: 20px 0;'>
            <p style='margin: 0; color: #666; font-size: 14px;'>Order Reference</p>
            <h1 style='color: #667eea; font-size: 32px; margin: 8px 0; letter-spacing: 4px;'>{orderRef}</h1>
            <p style='margin: 0; color: #666; font-size: 13px;'>{orderDate}</p>
        </div>

        <!-- What happens next -->
        <div style='background-color: #f0f4ff; border-left: 4px solid #667eea; padding: 14px 16px; margin: 20px 0;'>
            <strong style='color: #3730a3;'>🎨 What happens next?</strong>
            <p style='margin: 6px 0 0; color: #4338ca; font-size: 13px;'>
                The seller will review your customization details and get in touch with you through the chat. 
                Final pricing and delivery timeline will be confirmed once the seller approves your order.
            </p>
        </div>

        <!-- Customization Details -->
        <h3 style='color: #333; margin: 20px 0 10px;'>Customization Details</h3>
        <table style='width: 100%; border-collapse: collapse; margin: 0 0 20px;'>
            <tbody>
                <tr style='background-color: #f9f9f9;'>
                    <td style='padding: 10px 12px; font-size: 13px; color: #666; width: 40%; border-bottom: 1px solid #ddd;'><strong>Model</strong></td>
                    <td style='padding: 10px 12px; font-size: 13px; color: #333; border-bottom: 1px solid #ddd;'>{customOrder.Model ?? "N/A"}</td>
                </tr>
                <tr>
                    <td style='padding: 10px 12px; font-size: 13px; color: #666; border-bottom: 1px solid #ddd;'><strong>Color</strong></td>
                    <td style='padding: 10px 12px; font-size: 13px; color: #333; border-bottom: 1px solid #ddd;'>{customOrder.Color ?? "N/A"}</td>
                </tr>
                <tr style='background-color: #f9f9f9;'>
                    <td style='padding: 10px 12px; font-size: 13px; color: #666; border-bottom: 1px solid #ddd;'><strong>Texture</strong></td>
                    <td style='padding: 10px 12px; font-size: 13px; color: #333; border-bottom: 1px solid #ddd;'>{customOrder.Texture ?? "N/A"}</td>
                </tr>
                <tr>
                    <td style='padding: 10px 12px; font-size: 13px; color: #666; border-bottom: 1px solid #ddd;'><strong>Scale</strong></td>
                    <td style='padding: 10px 12px; font-size: 13px; color: #333; border-bottom: 1px solid #ddd;'>{customOrder.Scale ?? "N/A"}</td>
                </tr>
                <tr style='background-color: #f9f9f9;'>
                    <td style='padding: 10px 12px; font-size: 13px; color: #666; border-bottom: 1px solid #ddd;'><strong>Width</strong></td>
                    <td style='padding: 10px 12px; font-size: 13px; color: #333; border-bottom: 1px solid #ddd;'>{customOrder.Width ?? "N/A"}</td>
                </tr>
                <tr>
                    <td style='padding: 10px 12px; font-size: 13px; color: #666; border-bottom: 1px solid #ddd;'><strong>Height</strong></td>
                    <td style='padding: 10px 12px; font-size: 13px; color: #333; border-bottom: 1px solid #ddd;'>{customOrder.Height ?? "N/A"}</td>
                </tr>
                <tr style='background-color: #f9f9f9;'>
                    <td style='padding: 10px 12px; font-size: 13px; color: #666; border-bottom: 1px solid #ddd;'><strong>Quoted Price</strong></td>
                    <td style='padding: 10px 12px; font-size: 13px; font-weight: bold; color: #667eea; border-bottom: 1px solid #ddd;'>
                        {(price > 0 ? $"&#8369;{price:N2}" : "To be confirmed by seller")}
                    </td>
                </tr>
            </tbody>
        </table>

        <p><strong>Payment Method:</strong> <span style='color: #666;'>{paymentMethod}</span></p>
        <p><strong>Payment Status:</strong> <span style='color: #666;'>{customOrder.PaymentStatus ?? "Pending"}</span></p>

        <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>
        <p style='color: #666; font-size: 12px;'>This is an automated message, please do not reply.</p>
    </div>
</body>
</html>";

                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress(_fromEmail, _fromName);
                        mail.To.Add(toEmail);
                        mail.Subject = subject;
                        mail.Body = body;
                        mail.IsBodyHtml = true;
                        using (SmtpClient smtp = new SmtpClient(_smtpHost, _smtpPort))
                        {
                            smtp.Credentials = new NetworkCredential(_fromEmail, _fromPassword);
                            smtp.EnableSsl = true;
                            smtp.Send(mail);
                        }
                    }
                    return true;
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }
        public bool SendSuccessDropOffEmail(string toEmail, int purchasedProductId)
        {
            try
            {
                string subject = "Thank you for ordering!";

                var purchased = _context.PurchasedProduct
                    .Include(p => p.ProductVariants)
                        .ThenInclude(pv => pv.Product)
                    .Include(p => p.Billings)
                    .Include(p => p.Users)
                    .FirstOrDefault(p => p.Id == purchasedProductId);

                if (purchased == null) return false;

                var productName = purchased.ProductVariants?.Product?.Name ?? "Product";
                var variantName = purchased.ProductVariants?.Product?.Name ?? "Product";
                var quantity = purchased.Quantity;
                var price = purchased.Price ?? 0;
                var shipping = purchased.ShippingFee ?? 0;
                var grandTotal = price + shipping;
                var orderDate = purchased.PurchasedDate.ToString("MMMM dd, yyyy hh:mm tt");
                var orderRef = $"ORD-{purchased.PurchasedDate:yyyyMMdd}-{purchased.Id}";
                var firstName = purchased.Users?.Username ?? toEmail;
                var paymentMethod = purchased.PaymentMethod ?? "Cash on Delivery";
                var billing = purchased.Billings;
                var billingAddr = billing != null
                    ? $"{billing.Name}, {billing.Address}, {billing.City}, {billing.Country}"
                    : "N/A";

                string body = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>

        <h2 style='color: #333;'>Order Confirmation</h2>
        <p>Hi <strong>{firstName}</strong>, thank you for your purchase! Your order is confirmed and a rider will drop it off to you shortly.</p>

        <!-- Order Reference Box -->
        <div style='background-color: #f4f4f4; padding: 20px; text-align: center; margin: 20px 0;'>
            <p style='margin: 0; color: #666; font-size: 14px;'>Order Reference</p>
            <h1 style='color: #667eea; font-size: 32px; margin: 8px 0; letter-spacing: 4px;'>{orderRef}</h1>
            <p style='margin: 0; color: #666; font-size: 13px;'>{orderDate}</p>
        </div>

        <!-- Order Details -->
        <table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>
            <thead>
                <tr style='background-color: #f4f4f4;'>
                    <th style='text-align: left; padding: 10px; font-size: 13px; color: #666; border-bottom: 2px solid #ddd;'>Product</th>
                    <th style='text-align: center; padding: 10px; font-size: 13px; color: #666; border-bottom: 2px solid #ddd;'>Qty</th>
                    <th style='text-align: right; padding: 10px; font-size: 13px; color: #666; border-bottom: 2px solid #ddd;'>Total</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td style='padding: 12px 10px; border-bottom: 1px solid #ddd;'>
                        <strong style='color: #333;'>{productName}</strong>
                        {(string.IsNullOrEmpty(variantName) ? "" : $"<br><span style='font-size: 12px; color: #999;'>{variantName}</span>")}
                    </td>
                    <td style='padding: 12px 10px; text-align: center; color: #333; border-bottom: 1px solid #ddd;'>{quantity}</td>
                    <td style='padding: 12px 10px; text-align: right; color: #333; border-bottom: 1px solid #ddd;'>&#8369;{price:N2}</td>
                </tr>
            </tbody>
            <tfoot>
                <tr>
                    <td colspan='2' style='padding: 8px 10px; text-align: right; color: #666; font-size: 13px;'>Shipping Fee:</td>
                    <td style='padding: 8px 10px; text-align: right; color: #333; font-size: 13px;'>&#8369;{shipping:N2}</td>
                </tr>
                <tr>
                    <td colspan='2' style='padding: 8px 10px; text-align: right; font-weight: bold; color: #333;'>Grand Total:</td>
                    <td style='padding: 8px 10px; text-align: right; font-weight: bold; color: #667eea; font-size: 18px;'>&#8369;{grandTotal:N2}</td>
                </tr>
            </tfoot>
        </table>

        <!-- Delivery Address -->
        <p><strong>Delivery Address:</strong><br>
            <span style='color: #666;'>{billingAddr}</span>
        </p>

        <p><strong>Payment Method:</strong> <span style='color: #666;'>{paymentMethod}</span></p>

        <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>
        <p style='color: #666; font-size: 12px;'>This is an automated message, please do not reply.</p>
    </div>
</body>
</html>";

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(_fromEmail, _fromName);
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;
                    using (SmtpClient smtp = new SmtpClient(_smtpHost, _smtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(_fromEmail, _fromPassword);
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }
        public bool SendSuccessEmail(string toEmail, int purchasedProductId, string paymentMethod)
        {
            try
            {
                string subject = "Thank you for ordering!";

                // Fetch order details from DB
                var purchased = _context.PurchasedProduct
                    .Include(p => p.ProductVariants)
                        .ThenInclude(pv => pv.Product)
                    .Include(p => p.Billings)
                    .Include(p => p.Users)
                    .FirstOrDefault(p => p.Id == purchasedProductId);

                if (purchased == null) return false;

                var productName = purchased.ProductVariants?.Product?.Name ?? "Product";
                var variantName = purchased.ProductVariants?.Product?.Name ?? "Product";
                var quantity = purchased.Quantity;
                var price = purchased.Price ?? 0;
                var shipping = purchased.ShippingFee ?? 0;
                var grandTotal = price + shipping;
                var orderDate = purchased.PurchasedDate.ToString("MMMM dd, yyyy hh:mm tt") ?? DateTime.Now.ToString("MMMM dd, yyyy hh:mm tt");
                var orderRef = $"ORD-{purchased.PurchasedDate:yyyyMMdd}-{purchased.Id}";
                var firstName = purchased.Users?.Username ?? toEmail;
                var billing = purchased.Billings;
                var billingAddr = billing != null
                    ? $"{billing.Name}, {billing.Address}, {billing.City}, {billing.Country}"
                    : "N/A";

                string body = $@"
        <html>
        <body style='font-family: Arial, sans-serif;'>
            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>

                <!-- Header -->
                <h2 style='color: #333;'>Order Confirmation</h2>
                <p>Thank you for your purchase! Your payment was successful and your order is now being processed.</p>

                <!-- Order Reference Box (mirrors the OTP box in your existing template) -->
                <div style='background-color: #f4f4f4; padding: 20px; text-align: center; margin: 20px 0;'>
                    <p style='margin: 0; color: #666; font-size: 14px;'>Order Reference</p>
                    <h1 style='color: #667eea; font-size: 32px; margin: 8px 0; letter-spacing: 4px;'>{orderRef}</h1>
                    <p style='margin: 0; color: #666; font-size: 13px;'>{orderDate}</p>
                </div>

                <!-- Order Details -->
                <table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>
                    <thead>
                        <tr style='background-color: #f4f4f4;'>
                            <th style='text-align: left; padding: 10px; font-size: 13px; color: #666; border-bottom: 2px solid #ddd;'>Product</th>
                            <th style='text-align: center; padding: 10px; font-size: 13px; color: #666; border-bottom: 2px solid #ddd;'>Qty</th>
                            <th style='text-align: right; padding: 10px; font-size: 13px; color: #666; border-bottom: 2px solid #ddd;'>Total</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td style='padding: 12px 10px; border-bottom: 1px solid #ddd;'>
                                <strong style='color: #333;'>{productName}</strong>
                                {(string.IsNullOrEmpty(variantName) ? "" : $"<br><span style='font-size: 12px; color: #999;'>{variantName}</span>")}
                            </td>
                            <td style='padding: 12px 10px; text-align: center; color: #333; border-bottom: 1px solid #ddd;'>{quantity}</td>
                            <td style='padding: 12px 10px; text-align: right; color: #333; border-bottom: 1px solid #ddd;'>&#8369;{price:N2}</td>
                        </tr>
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colspan='2' style='padding: 8px 10px; text-align: right; color: #666; font-size: 13px;'>Shipping Fee:</td>
                            <td style='padding: 8px 10px; text-align: right; color: #333; font-size: 13px;'>&#8369;{shipping:N2}</td>
                        </tr>
                        <tr>
                            <td colspan='2' style='padding: 8px 10px; text-align: right; font-weight: bold; color: #333;'>Grand Total:</td>
                            <td style='padding: 8px 10px; text-align: right; font-weight: bold; color: #667eea; font-size: 18px;'>&#8369;{grandTotal:N2}</td>
                        </tr>
                    </tfoot>
                </table>

                <!-- Billing Address -->
                <p><strong>Billing Address:</strong><br>
                    <span style='color: #666;'>{billingAddr}</span>
                </p>

                <p><strong>Payment Method:</strong> <span style='color: #666;'>{paymentMethod}</span></p>

                <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>
                <p style='color: #666; font-size: 12px;'>This is an automated message, please do not reply.</p>
            </div>
        </body>
        </html>";

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(_fromEmail, _fromName);
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;
                    using (SmtpClient smtp = new SmtpClient(_smtpHost, _smtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(_fromEmail, _fromPassword);
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }
        public bool SendVerificationCode(string toEmail, string code)
        {
            try
            {
                string subject = "Your Verification Code";
                string body = GetEmailBody(code);

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(_fromEmail, _fromName);
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient(_smtpHost, _smtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(_fromEmail, _fromPassword);
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }

        private string GetEmailBody(string code)
        {
            return $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #333;'>Email Verification</h2>
                    <p>Thank you for registering! Please use the following code to verify your email address:</p>
                    
                    <div style='background-color: #f4f4f4; padding: 20px; text-align: center; margin: 20px 0;'>
                        <h1 style='color: #667eea; font-size: 48px; margin: 0; letter-spacing: 10px;'>{code}</h1>
                    </div>
                    
                    <p><strong>This code will expire in 10 minutes.</strong></p>
                    <p>If you didn't request this code, please ignore this email.</p>
                    
                    <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>
                    <p style='color: #666; font-size: 12px;'>This is an automated message, please do not reply.</p>
                </div>
            </body>
            </html>
        ";
        }
        public bool SendPasswordResetEmail(string toEmail, string resetLink, string userName)
        {
            try
            {
                string subject = "Password Reset Request";
                string body = GetPasswordResetEmailBody(resetLink, userName);
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(_fromEmail, _fromName);
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;
                    using (SmtpClient smtp = new SmtpClient(_smtpHost, _smtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(_fromEmail, _fromPassword);
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Password reset email sending failed: {ex.Message}");
                return false;
            }
        }
        private string GetPasswordResetEmailBody(string resetLink, string userName)
        {
            return $@"
    <html>
    <body style='font-family: Arial, sans-serif;'>
        <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
            <h2 style='color: #333;'>Password Reset Request</h2>
            <p>Hi {userName},</p>
            <p>We received a request to reset your password. Click the button below to proceed:</p>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{resetLink}' 
                   style='display: inline-block; padding: 12px 30px; background-color: #667eea; color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                    Reset Password
                </a>
            </div>
            
            <p>Or copy and paste this link in your browser:</p>
            <p style='word-break: break-all; background-color: #f4f4f4; padding: 10px; border-radius: 3px;'>
                <code>{resetLink}</code>
            </p>
            
            <p><strong>This link will expire in 15 minutes.</strong></p>
            <p>If you didn't request this, please ignore this email and your password will remain unchanged.</p>
            
            <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>
            <p style='color: #666; font-size: 12px;'>This is an automated message, please do not reply.</p>
        </div>
    </body>
    </html>
";
        }
    }
}
