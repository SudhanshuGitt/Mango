using Mango.Services.EmailAPI.Data;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Mango.Services.EmailAPI.Services
{
    // we want to configure this API is whey api is running start will be invoked and if we stop Stop will be invoked
    // so we can add singleton dependency injection so in that way we ahve just one copy
    public class EmailService : IEmailService
    {
        private DbContextOptions<AppDbContext> _dbOptions;

        public EmailService(DbContextOptions<AppDbContext> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        // here we cannot use applicaton dbcontext using DI because that is scoped implementation
        // we need to registem implemention of email service that is singleton
        public async Task EmailCartAndLog(CartDto cartDto)
        {
            StringBuilder message = new StringBuilder();

            message.AppendLine("<br/> Cart Email Requested");
            message.AppendLine("<br/>Total " + cartDto.CartHeader.CartTotal);
            message.Append("<br/>");
            message.Append("<ul>");

            foreach (var item in cartDto.CartDetails)
            {
                message.Append("<li>");
                message.Append(item.Product.Name + " x " + item.Count);
                message.Append("</li>");
            }
            message.Append("</ul>");

            await LogAndEmail(message.ToString(), cartDto.CartHeader.Email);

        }

        public async Task RegisterUserEmailAndLog(string email)
        {
            string message = "User Registeration Successfull. <br/> Email : " + email;
            await LogAndEmail(message, "Admin@qwe.com");
        }

        private async Task<bool> LogAndEmail(string message, string email)
        {
            try
            {
                EmailLogger emailLog = new EmailLogger()
                {
                    Email = email,
                    EmailSent = DateTime.Now,
                    Message = message,
                };

                await using var _db = new AppDbContext(_dbOptions);

                await _db.EmailLoggers.AddAsync(emailLog);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
