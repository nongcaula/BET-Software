using Azure.Core;
using BET.Areas.Identity.Pages.Account;
using BET.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;
using SendGrid.Helpers.Mail;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace BET.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        readonly string baseurl = "https://localhost:7200/";
        private readonly HttpClient _httpClient;
        public HomeController(ILogger<HomeController> logger)
        {
            _httpClient = GetHttpClient();
            _logger = logger;
        }

        public async Task<ActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                List<Product> products = new List<Product>();

                HttpResponseMessage res = await _httpClient.GetAsync("api/Product");
                if (res.IsSuccessStatusCode)
                {
                    var productsResponse = res.Content.ReadAsStringAsync().Result;
                    products = JsonConvert.DeserializeObject<List<Product>>(productsResponse);
                }
                return View(products.Where(x => x.IsDiscontinued == false));
            }
            else
            {
                string loginUrl = ReturnLoginUrl();
                return Redirect(loginUrl);
            }
        }

        private string ReturnLoginUrl()
        {
            var request = HttpContext.Request;
            var loginUrl = request.Scheme + "://" + request.Host.Value + "/Identity/Account/Login";
            return loginUrl;
        }

        private async Task<Product> GetProductById(int id)
        {
            Product product = new();
            HttpResponseMessage res = await _httpClient.GetAsync("api/Product/" + id);
            if (res.IsSuccessStatusCode)
            {
                var productsResponse = res.Content.ReadAsStringAsync().Result;
                product = JsonConvert.DeserializeObject<Product>(productsResponse);
            }
            return product;
        }

        private HttpClient GetHttpClient()
        {
            var apiKey = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("XApiKey");
            var client = new HttpClient();
            client.BaseAddress = new Uri(baseurl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("XApiKey", apiKey.Value); // API Key
            return client;
        }

        [HttpPost]
        public async Task<string> AddtoCart(int id, int quantity)
        {
            if (User.Identity.IsAuthenticated) // Remove this line if in future the user doesn't need to be authenticated to use the product
            {
                var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var product = await GetProductById(id);
                product.Quantity = quantity;
                HttpContext.Session.SetString(id.ToString(), JsonConvert.SerializeObject(product));
                CartItemCounter();
                return "Item added to Cart";
            }
            return "Uauthorized";
        }
      
        public bool RemoveItemFromCart(int id)
        {
            try
            {
                HttpContext.Session.Remove(id.ToString());
                CartItemCounter();
                return true;
            } catch (Exception ex)
            {
                return false;
            }
        }

        private void ClearCart()
        {
            var keys = HttpContext.Session.Keys;
            foreach (var key in keys)
            {
                HttpContext.Session.Remove(key);
            }
        }
        public ActionResult ViewCart()
        {
            List<Product> products = GetProducts();
            return View(products);
        }

        public int GetRunningTotal()
        {
            var keys = HttpContext.Session.Keys;
            var total = 0;
            foreach (var key in keys)
            {
                var product = JsonConvert.DeserializeObject<Product>(HttpContext.Session.GetString(key));
                total += (int)product.Quantity * (int)product.Price;
            }
            return total;
        }
        private List<Product> GetProducts()
        {
            var keys = HttpContext.Session.Keys;
            List<Product> products = new();
            foreach (var key in keys)
            {
                products.Add(JsonConvert.DeserializeObject<Product>(HttpContext.Session.GetString(key)));
            }
            return products;
        }

        public async Task<ActionResult> CheckOut()
        {
            if (User.Identity.IsAuthenticated)
            {
                var transactions = GetTransactions();
                string json = JsonConvert.SerializeObject(transactions);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage res = await _httpClient.PostAsync("api/Transactions", httpContent);
                if (res.IsSuccessStatusCode)
                {
                    ClearCart();
                    return RedirectToAction(nameof(Index));
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                string loginUrl = ReturnLoginUrl();
                return Redirect(loginUrl);
            }
        }
        private List<Transaction> GetTransactions()
        {
            List<Transaction> transactions = new();
            foreach (var product in GetProducts())
            {
                var trn = new Transaction
                {
                    TransactionDate = DateTime.Now,
                    ProductId = product.Id,
                    Quantity = product.Quantity,
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                };
                transactions.Add(trn);
            }
            return transactions;
        }

        public int CartItemCounter()
        {
            var keys = HttpContext.Session.Keys;
            var count = 0;
            foreach (var key in keys)
            {
                var product = JsonConvert.DeserializeObject<Product>(HttpContext.Session.GetString(key));
                count += (int)product.Quantity;
            }
            return count;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}