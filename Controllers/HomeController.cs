using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MarkStart.Models;
using Microsoft.Data.SqlClient;
using Dapper;
namespace MarkStart.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
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

    public IActionResult Registro()
    {
        return View();
    }

    public IActionResult InicioSesion()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Registro(string Nombre, string Apellido, string Usuario, string NombreUsuario, string Contraseña, string Email, int Telefono)
    {
        BD bd = new BD();
        Usuario u = new Usuario(Nombre, Apellido, nombreDeUsuario, Contraseña, 0, Email, Telefono);

        if (bd.buscarPorNombreUsuario(u.Usuario) == null)
        {
            bd.agregarUsuario(u);
            return RedirectToAction("InicioSesion", "Home");
        }

        ViewBag.error = "El nombre de usuario ya existe.";
        return View("Registro");
    }

    [HttpPost]
    public IActionResult InicioSesion(string Usuario, string Contraseña)
    {
        BD bd = new BD();
        Usuario usuarioEncontrado = bd.encontrarUsuario(Usuario, Contraseña);
        if (usuarioEncontrado == null)
        {
            ViewBag.Error = "Usuario o contraseña incorrectos.";
            return View();
        }

        HttpContext.Session.SetString("Usuario", usuarioEncontrado.Usuario);
        HttpContext.Session.SetString("Contraseña", usuarioEncontrado.Contraseña);

        return RedirectToAction("PaginaPrincipal", "Home");
    }
}
