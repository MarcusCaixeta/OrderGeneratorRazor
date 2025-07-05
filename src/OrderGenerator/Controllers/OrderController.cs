
using Microsoft.AspNetCore.Mvc;
using OrderGenerator.Models;
using OrderGenerator.Fix;
using OrderGenerator.Interfaces;

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
        public IActionResult Index(OrderModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string resposta = _fixClient.SendOrder(model);
            ViewBag.Resposta = resposta;
            return View();
        }
    }
}
