using System.Collections.Generic;

namespace Domain.Models.Auth
{
    public class AuthFailedResponse
    {
        public IEnumerable<string> ErrorMessages { get; set; }
    }
}