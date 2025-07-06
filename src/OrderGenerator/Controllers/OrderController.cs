
using Microsoft.AspNetCore.Mvc;
using OrderGenerator.Contracts.Interfaces;
using OrderGenerator.Contracts.Models;

namespace OrderGenerator.Controllers
{
    public class OrderController : Controller
    {
        private readonly IFixOrderClient _fixClient;

        public OrderController(IFixOrderClient fixClient)
        {
            _fixClient = fixClient;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(OrderModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Resposta = "Preencha corretamente os campos abaixo.";
                return View(model);
            }

            string resposta = await _fixClient.SendOrder(model);
            ViewBag.Resposta = resposta;
            return View();
        }
    }
}
