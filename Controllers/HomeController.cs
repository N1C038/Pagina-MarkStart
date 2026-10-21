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
        BD bd = new BD();
        var usuario = HttpContext.Session.GetString("Usuario");
        if (string.IsNullOrEmpty(usuario))
        {
            usuario = "";
            ViewBag.Cursos = bd.obtener3Cursos();
        }
        else
        {
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
    public IActionResult Cursos(int IdStartup)
    {
        BD bd = new BD();
        List<Startup> startups = bd.obtenerStartupsUsuario(int.Parse(HttpContext.Session.GetString("IdUsuario") ?? "0"));
        if(startups.Count > 0)
        {
            ViewBag.Startups = startups;
            if (IdStartup == 0)
            {
                IdStartup = startups[0].IdStartup;
                ViewBag.NombreStartup = startups[0].Nombre;
            }
            else{
                ViewBag.NombreStartup = startups[IdStartup - 1].Nombre;
            }
            List<Curso> cursosComprados = bd.obtenerCursosStartup(IdStartup);
            List<Curso> cursosRecomendados = bd.obtener3Cursos();
            ViewBag.CursosComprados = cursosComprados;
            ViewBag.CursosRecomendados = cursosRecomendados;
        }
        else{
            List<Curso> cursosRecomendados = bd.obtener3Cursos();
            ViewBag.CursosRecomendados = cursosRecomendados;
        }
        return View();     
    }
    [HttpPost]
    public IActionResult Registro(string Nombre, string Apellido, string Contraseña, string Email, string Telefono, IFormFile FotoPerfil)
    {
        BD bd = new BD();

        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
        {
            ViewBag.Error = "El email debe contener @.";
            return View("Registro");
        }

        if (!string.IsNullOrWhiteSpace(Telefono) && Telefono.Any(ch => !char.IsDigit(ch)))
        {
            ViewBag.Error = "El teléfono solo puede contener números.";
            return View("Registro");
        }

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

        if (!string.IsNullOrEmpty(usuarioEncontrado.Nombre))
            HttpContext.Session.SetString("Nombre", usuarioEncontrado.Nombre);
        if (!string.IsNullOrEmpty(usuarioEncontrado.FotoPerfil))
            HttpContext.Session.SetString("FotoPerfil", usuarioEncontrado.FotoPerfil);

        return RedirectToAction("Index", "Home");
    }
    public IActionResult PasarCursos()
    {
        BD bd = new BD();
        List<Curso> cursos = bd.obtenerCursos();
        return View(cursos);
    }
}
