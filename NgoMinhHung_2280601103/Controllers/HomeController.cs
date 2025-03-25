using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NgoMinhHung_2280601103.Models;
using NgoMinhHung_2280601103.Repository;

namespace NgoMinhHung_2280601103.Controllers;

public class HomeController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IProductRepository productRepository, ILogger<HomeController> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productRepository.GetAllAsync();
        return View(products);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
