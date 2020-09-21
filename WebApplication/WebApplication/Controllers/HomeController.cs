using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _webApiScope = string.Empty;
        private readonly string _webApiBaseAddress = string.Empty;

        public HomeController(ITokenAcquisition tokenAcquisition, IConfiguration configuration, IHttpClientFactory clientFactory, ILogger<HomeController> logger)
        {
            _logger = logger;
            _tokenAcquisition = tokenAcquisition;
            _clientFactory = clientFactory;
            _webApiScope = configuration["CallApi:Scope"];
            _webApiBaseAddress = configuration["CallApi:BaseAddress"];
        }

        public IActionResult Index()
        {
            return View();
        }

        [AuthorizeForScopes(ScopeKeySection = "CallApi:Scope")]
        public async Task<IActionResult> Privacy()
        {
            var scopes = new string[] { _webApiScope };
            var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);

            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await client.GetAsync($"{_webApiBaseAddress}/weatherforecast");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                ViewData["ApiResult"] = await response.Content.ReadAsStringAsync();
            }
            else
            {
                ViewData["ApiResult"] = $"Invalid status code in the HttpResponseMessage: {response.StatusCode}.";
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
