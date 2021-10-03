using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore;
using ITfoxtec.Identity.Saml2.Schemas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SampleSP.NET.Web.Helpers;

namespace SampleSP.NET.Web.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private const string RELAY_STATE_RETURN_URL = "ReturnUrl";
        private readonly ILogger<AuthController> _logger;
        private readonly Saml2Configuration _config;

        public AuthController(ILogger<AuthController> logger, IOptions<Saml2Configuration> configAccessor)
        {
            _logger = logger;
            _config = configAccessor.Value;
        }

        [HttpGet("entrypoint")]
        public IActionResult Entrypoint(string returnUrl)
        {
            var binding = new Saml2PostBinding();
            binding.SetRelayStateQuery(new Dictionary<string, string>() {
                { RELAY_STATE_RETURN_URL, returnUrl ?? Url.Content("~/auth/greeting") }
            });
            return binding.Bind(new Saml2AuthnRequest(_config)
            {
                NameIdPolicy = new NameIdPolicy { AllowCreate = false, Format = "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress" }
            }).ToActionResult();
        }

        [HttpPost("assertion-consumer-service")]
        public async Task<IActionResult> AssertionConsumerService()
        {
            var binding = new Saml2PostBinding();
            var authResponse = new Saml2AuthnResponse(_config);

            binding.ReadSamlResponse(Request.ToGenericHttpRequest(), authResponse);
            if (authResponse.Status != Saml2StatusCodes.Success)
            {
                throw new AuthenticationException($"SAML Response status: {authResponse.Status}");
            }
            binding.Unbind(Request.ToGenericHttpRequest(), authResponse);

            await authResponse.CreateSession(HttpContext, claimsTransform: (claimsPrincipal) => ClaimsTransform.Transform(claimsPrincipal));
            return Redirect(extractRelayState(binding));
        }

        private string extractRelayState(Saml2PostBinding binding)
        {
            var relayState = binding.GetRelayStateQuery();
            return relayState.ContainsKey(RELAY_STATE_RETURN_URL) ? relayState[RELAY_STATE_RETURN_URL] : Url.Content("~/auth/greeting");
        }

        [HttpGet("greeting")]
        public IActionResult Greeting()
        {
            return new OkObjectResult(new {
                Message = "Hello World !"
            });
        }
    }
}
