using System.Collections.Generic;
using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore;
using ITfoxtec.Identity.Saml2.Schemas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
                { RELAY_STATE_RETURN_URL, returnUrl ?? Url.Content("~/greeting") }
            });
            return binding.Bind(new Saml2AuthnRequest(_config)
            {
                NameIdPolicy = new NameIdPolicy { AllowCreate = false, Format = "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress" }
            }).ToActionResult();
        }

        [HttpGet("assertion-consumer-service")]
        public IActionResult AssertionConsumerService()
        {
            return RedirectToAction();
        }

        [HttpGet("greeting")]
        public IActionResult Greeting()
        {
            return new OkObjectResult(new {
                Message="Hello World !"
            });
        }
    }
}
