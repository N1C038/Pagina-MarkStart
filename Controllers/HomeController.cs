using System.Diagnostics;
using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using MarkStart.Models;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
namespace MarkStart.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IWebHostEnvironment _env;

    public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
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
    public IActionResult Registro(string Nombre, string Apellido, string Usuario, string Contraseña, string Email, int Telefono, IFormFile FotoPerfil)
    {
        BD bd = new BD();
        string fotoRuta = null;

        if (FotoPerfil != null && FotoPerfil.Length > 0)
        {
            string carpeta = Path.Combine(_env.WebRootPath ?? "wwwroot", "imagenes");
            if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);
            string nombreUnico = Guid.NewGuid().ToString() + Path.GetExtension(FotoPerfil.FileName);
            string rutaCompleta = Path.Combine(carpeta, nombreUnico);
            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                FotoPerfil.CopyTo(stream);
            }
            fotoRuta = Path.Combine("imagenes", nombreUnico).Replace("\\","/");
        }

        Usuario u = new Usuario(Nombre, Apellido, Usuario, Contraseña, 0, Email, Telefono, fotoRuta);

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

        // Store additional fields for header display
        if (!string.IsNullOrEmpty(usuarioEncontrado.Nombre))
            HttpContext.Session.SetString("Nombre", usuarioEncontrado.Nombre);
        if (!string.IsNullOrEmpty(usuarioEncontrado.FotoPerfil))
            HttpContext.Session.SetString("FotoPerfil", usuarioEncontrado.FotoPerfil);

        return RedirectToAction("PaginaPrincipal", "Home");
    }
}
