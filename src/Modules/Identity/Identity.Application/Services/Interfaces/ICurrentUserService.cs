using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Services.Interfaces
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        string Email { get; }
        string Role { get; }
        bool IsAuthenticated { get; }
        bool IsAdmin { get; }
        bool IsSeller { get; }
    }
}
