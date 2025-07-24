using Microsoft.AspNetCore.Mvc;
using OrderGenerator.WorkerService.Models;


namespace OrderGenerator.Controllers
{
    public class OrderController : Controller
    {
        private readonly RabbitMqPublisher _publisher;


        public OrderController( RabbitMqPublisher publisher)
        {
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
