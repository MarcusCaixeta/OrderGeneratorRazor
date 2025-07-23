using Microsoft.AspNetCore.Mvc;
using OrderGenerator.WorkerService.Models;
using OrderGenerator.WorkerService.Interfaces;
using QuickFix.Fields;


namespace OrderGenerator.Controllers
{
    public class OrderController : Controller
    {
        private readonly IFixOrderClient _fixClient;
        private readonly RabbitMqPublisher _publisher;


        public OrderController(IFixOrderClient fixClient, RabbitMqPublisher publisher)
        {
            _fixClient = fixClient;
            _publisher = publisher;
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

            await _publisher.PublishAsync(model);

            ViewBag.Resposta = "Ordem enfileirada com sucesso";
            return View();
        }
    }
}
