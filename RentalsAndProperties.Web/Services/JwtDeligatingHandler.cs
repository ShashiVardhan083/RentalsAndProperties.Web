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
                
                var token = context?.User?.Claims.FirstOrDefault(c => c.Type == "JwtToken")?.Value; 

                if (!string.IsNullOrWhiteSpace(token))
                {

                request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

            return await base.SendAsync(request, cancellationToken); //CancellationToken is a mechanism to gracefully stop an ongoing async operation when it’s no longer needed.
        }
    }
}
