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
        var usuario = HttpContext.Session.GetString("Usuario");
        if (string.IsNullOrEmpty(usuario))
        {
            usuario = "";
            ViewBag.Cursos = bd.obtener3Cursos();
        }
        else
        {
            BD bd = new BD();
            var idUsuarioSesion = HttpContext.Session.GetString("IdUsuario") ?? HttpContext.Session.GetString("Id");
            if (int.TryParse(idUsuarioSesion, out int idUsuario))
            {
                Dictionary<Curso, Tarea> cursosEncontrados = bd.buscarCursosYTareasUsuario(idUsuario);
                ViewBag.Cursos = cursosEncontrados;
            }
        }
        ViewBag.Usuario = usuario;
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
    public IActionResult Registro(string Nombre, string Apellido, string Contraseña, string Email, string Telefono, IFormFile FotoPerfil)
    {
        BD bd = new BD();
        string fotoRuta = "imagenes/default-user.svg";

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

        if (bd.buscarUsuarioPorEmail(Email) != null)
        {
            ViewBag.Error = "El email ya está registrado.";
            return View("Registro");
        }

        Usuario u = new Usuario
        {
            Email = Email,
            Nombre = Nombre,
            EsAdmin = false,
            Apellido = Apellido,
            Telefono = Telefono,
            FotoPerfil = fotoRuta,
            Contraseña = Contraseña
        };

        bd.agregarUsuario(u);
        return RedirectToAction("InicioSesion", "Home");
    }

    [HttpPost]
    public IActionResult InicioSesion(string Email, string Contraseña)
    {
        BD bd = new BD();
        Usuario usuarioEncontrado = bd.encontrarUsuario(Email, Contraseña);
        if (usuarioEncontrado == null)
        {
            ViewBag.Error = "Usuario o contraseña incorrectos.";
            return View();
        }

        HttpContext.Session.SetString("Usuario", usuarioEncontrado.Nombre);
        HttpContext.Session.SetString("IdUsuario", usuarioEncontrado.IdUsuario.ToString());
        HttpContext.Session.SetString("Id", usuarioEncontrado.IdUsuario.ToString());

        if (!string.IsNullOrEmpty(usuarioEncontrado.Nombre))
            HttpContext.Session.SetString("Nombre", usuarioEncontrado.Nombre);
        if (!string.IsNullOrEmpty(usuarioEncontrado.FotoPerfil))
            HttpContext.Session.SetString("FotoPerfil", usuarioEncontrado.FotoPerfil);

        return RedirectToAction("PaginaPrincipal", "Home");
    }
    public IActionResult PasarCursos()
    {
        BD bd = new BD();
        List<Curso> cursos = bd.obtenerCursos();
        return View(cursos);
    }
}
