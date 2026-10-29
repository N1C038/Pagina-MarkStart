using System.Diagnostics;
using System;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MarkStart.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
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

    private void IniciarSesion(Usuario usuario)
    {
        HttpContext.Session.SetString("Usuario", usuario.Nombre);
        HttpContext.Session.SetString("IdUsuario", usuario.IdUsuario.ToString());

        if (!string.IsNullOrEmpty(usuario.Nombre))
            HttpContext.Session.SetString("Nombre", usuario.Nombre);
        if (!string.IsNullOrEmpty(usuario.FotoPerfil))
            HttpContext.Session.SetString("FotoPerfil", usuario.FotoPerfil);
    }

    [HttpGet("signin-google")]
    public IActionResult GoogleLogin()
    {
        var redirectUrl = Url.Action(nameof(GoogleResponse), "Home");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("signin-google-callback")]
    public async Task<IActionResult> GoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!result.Succeeded || result.Principal == null)
        {
            ViewBag.Error = "No se pudo iniciar sesión con Google.";
            return View("InicioSesion");
        }

        var claims = result.Principal.Claims.ToList();
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
            ?? claims.FirstOrDefault(c => c.Type == "email")?.Value;
        var nombre = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value
            ?? claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
            ?? (email != null ? email.Split('@')[0] : "UsuarioGoogle");
        var apellido = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? "";
        var foto = claims.FirstOrDefault(c => c.Type == "picture")?.Value ?? "imagenes/default-user.png";

        if (string.IsNullOrWhiteSpace(email))
        {
            ViewBag.Error = "No se pudo obtener el email de Google.";
            return View("InicioSesion");
        }

        BD bd = new BD();
        Usuario usuarioExistente = bd.buscarUsuarioPorEmail(email);

        if (usuarioExistente == null)
        {
            string contraseñaTemporal = "Google_" + Guid.NewGuid().ToString("N")[..12];
            Usuario nuevoUsuario = new Usuario
            {
                Email = email,
                Nombre = nombre,
                Apellido = apellido,
                Telefono = "",
                FotoPerfil = foto,
                Contraseña = contraseñaTemporal,
                EsAdmin = false
            };
            bd.agregarUsuario(nuevoUsuario);
            usuarioExistente = bd.encontrarUsuario(email, contraseñaTemporal) ?? bd.buscarUsuarioPorEmail(email);
        }

        if (usuarioExistente == null)
        {
            ViewBag.Error = "No se pudo crear el usuario con Google.";
            return View("InicioSesion");
        }

        IniciarSesion(usuarioExistente);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Index()
    { 
        BD bd = new BD();
        var usuario = HttpContext.Session.GetString("Usuario");
        if (string.IsNullOrEmpty(usuario))
        {
            usuario = "";
            ViewBag.Cursos = bd.obtener3Cursos();
            ViewBag.Reseñas = bd.obtener3Reseñas();
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
    public IActionResult PerfilUsuario(int IdUsuario){
        return RedirectToAction("CerrarSesion");
    }
    public IActionResult CerrarSesion(){
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }
    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Marketing()
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
        if(string.IsNullOrEmpty(HttpContext.Session.GetString("IdUsuario"))){
            return RedirectToAction("Index");
        }
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

        string fotoRuta = "imagenes/default-user.png";
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

        IniciarSesion(usuarioEncontrado);

        return RedirectToAction("Index", "Home");
    }
    public IActionResult DetalleCurso(int IdCurso)
    {
        BD bd = new BD();
        Curso curso = bd.buscarCursoPorId(IdCurso);
        ViewBag.IdCurso = curso.IdCurso;
        ViewBag.NombreCurso = curso.Nombre;
        ViewBag.DescripcionCurso = curso.Descripcion;
        ViewBag.PrecioCurso = curso.Precio;
        ViewBag.Reseñas = bd.obtenerReseñasCurso(curso.IdCurso);
        ViewBag.Tareas = bd.obtenerTareasCurso(curso.IdCurso);

        return View();
    }
    public IActionResult Tareas(int Id)
    {
        BD bd = new BD();
        Curso curso = bd.buscarCursoPorId(Id);

        if (curso == null)
        {
            return NotFound();
        }

        ViewBag.Curso = curso;
        ViewBag.Tareas = bd.obtenerTareasCurso(Id);
        ViewBag.IdCurso = curso.IdCurso;

        return View();
    }
}
