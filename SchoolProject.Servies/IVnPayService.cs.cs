using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Servies
{
    public interface IVnPayService
    {
        Task<string> CreatePaymentUrl(
            string userId,
            int feeId,
            string ipAddress);

        Task<(bool Success, int FeeId, string Message)>
            ProcessReturn(IQueryCollection query);

        Task<(bool Success, string Message)>
            ProcessIpn(IQueryCollection query);
    }
}
