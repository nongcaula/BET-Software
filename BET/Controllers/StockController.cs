using Azure.Core;
using BET.Models;
using EllipticCurve.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace BET.Controllers
{
    public class StockController : Controller
    {
        // GET: StockController
        string baseurl = "https://localhost:7200/";

        public async Task<ActionResult> Index()
        {
            try
            {
                List<Product> products = new List<Product>();
                var apiKey = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("XApiKey");
                var client = GetHttpClient(apiKey);
                HttpResponseMessage res = await client.GetAsync("api/Product");
                if (res.IsSuccessStatusCode)
                {
                    var productsResponse = res.Content.ReadAsStringAsync().Result;
                    products = JsonConvert.DeserializeObject<List<Product>>(productsResponse);
                }
                return View(products);
            }
            catch (Exception ex)
            {
                ViewBag.error = ex.Message;
                return RedirectToAction(nameof(Error));
            }
        }
        // GET: StockController/Create
        public ActionResult Create()
        {
            return View();
        }
        public async Task<ActionResult> Edit(int id)
        {
            var product = new Product();
            var apiKey = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("XApiKey");
            var client = GetHttpClient(apiKey);
            HttpResponseMessage res = await client.GetAsync("api/Product/" + id);
            if (res.IsSuccessStatusCode)
            {
                var productResponse = res.Content.ReadAsStringAsync().Result;
                product = JsonConvert.DeserializeObject<Product>(productResponse);
            }
            return View(product);
        }

        // POST: StockController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, Product productForm, IFormCollection collection)
        {
            HttpResponseMessage res = new HttpResponseMessage();
            try
            {
                var result = "Update could not be completed";
                if (Request.Form.Files.Count > 0)
                {
                    using (var binaryReader = new BinaryReader(Request.Form.Files[0].OpenReadStream()))
                    {
                        var file = binaryReader.ReadBytes((int)Request.Form.Files[0].Length);
                        string base64ImageRepresentation = Convert.ToBase64String(file);
                        productForm.Img = base64ImageRepresentation;
                    }
                }
                var apiKey = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("XApiKey");
                var client = GetHttpClient(apiKey);
                string json = JsonConvert.SerializeObject(productForm);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                if (id == 0)
                {
                    res = await client.PostAsync("api/Product", httpContent);
                }
                else
                {
                    res = await client.PutAsync("api/Product/" + id, httpContent);
                }
                if (res.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.error = result;
                return RedirectToAction(nameof(Error));
            }
            catch (Exception ex)
            {
                ViewBag.error = ex.Message;
                return RedirectToAction(nameof(Error));
            }
        }

        public ActionResult Error()
        {
            return View();
        }

        // GET: StockController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var apiKey = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("XApiKey");
            var client = GetHttpClient(apiKey);
            HttpResponseMessage res = await client.DeleteAsync("api/Product/" + id);
            if (res.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ViewBag.error = res.ReasonPhrase;
                return RedirectToAction(nameof(Error));
            }
        }

        private HttpClient GetHttpClient(IConfigurationSection apiKey)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(baseurl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("XApiKey", apiKey.Value); // API Key
            return client;
        }
    }
}
