using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NewERP.Models;

namespace NewERP.Controllers;

public class DataController : Controller
{
    private readonly ILogger<DataController> _logger;

    public DataController(ILogger<DataController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

}
