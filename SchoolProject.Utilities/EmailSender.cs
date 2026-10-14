using Microsoft.AspNetCore.Identity;
using SchoolProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Utilities
{
    public class EmailSender : IEmailSender<ApplicationUser>
    {
        public Task SendConfirmationLinkAsync(
            ApplicationUser user,
            string email,
            string confirmationLink)
        {
            return Task.CompletedTask;
        }

        public Task SendPasswordResetLinkAsync(
            ApplicationUser user,
            string email,
            string resetLink)
        {
            return Task.CompletedTask;
        }

        public Task SendPasswordResetCodeAsync(
            ApplicationUser user,
            string email,
            string resetCode)
        {
            return Task.CompletedTask;
        }
    }
}
