
using System.Security.Claims;

namespace MockApi.Runtime.Session
{
    public class ClaimsAppSession : IAppSession
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public ClaimsPrincipal Principal =>
    _contextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());

        public ClaimsAppSession(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var userIdClaim = Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim?.Value))
                {
                    return null;
                }

                Guid userId;
                if (!Guid.TryParse(userIdClaim?.Value, out userId))
                {
                    return null;
                }

                return userId;
            }
        }
    }
}
