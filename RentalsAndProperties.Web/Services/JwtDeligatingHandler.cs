using System.Net.Http.Headers;

namespace RentalsAndProperties.Web.Services
{
    public class JwtDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor ContextAccessor;

        public JwtDelegatingHandler(IHttpContextAccessor contextAccessor)
            => ContextAccessor = contextAccessor;

        protected override async Task<HttpResponseMessage> SendAsync(
                 HttpRequestMessage request,
                 CancellationToken cancellationToken)
        {
            var context = ContextAccessor.HttpContext;

            if (context?.Session != null)
            {
                var token = context.Session.GetString("JwtToken");

                if (!string.IsNullOrWhiteSpace(token))
                {
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
