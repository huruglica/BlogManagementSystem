using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace BlogManagementSystemTest.Helper
{
    public class HttpContextAccessorHelper
    {
        public HttpContextAccessor HttpContextAccessor { get; }

        public HttpContextAccessorHelper()
        {
            HttpContextAccessor = new HttpContextAccessor();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Username"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.NameIdentifier, "64dcd34fe55c1e2ee8460991")
            };
            var identity = new ClaimsIdentity(claims, "mock");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal,
                Session = new DummySessioin()
            };

            HttpContextAccessor.HttpContext = httpContext;
        }
    }
}
