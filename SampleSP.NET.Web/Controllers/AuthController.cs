using System.Collections.Generic;
using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SampleSP.NET.Web.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly Saml2Configuration _config;

        public AuthController(ILogger<AuthController> logger, Saml2Configuration config)
        {
            _logger = logger;
            _config = config;
        }

        [HttpGet("entrypoint")]
        public IActionResult Entrypoint(string returnUrl)
        {
            var binding = new Saml2PostBinding();
            binding.SetRelayStateQuery(new Dictionary<string, string>() {
                { "relayStateReturnUrl", returnUrl ?? Url.Content("~/greeting") }
            });
            return binding.Bind(new Saml2AuthnRequest(_config)).ToActionResult();
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
