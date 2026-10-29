using System.Diagnostics;
using System;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MarkStart.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using System.Net.Http.Headers;

namespace MarkStart.Controllers;

public class AskAiRequest
{
    public string? Prompt { get; set; }
}

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _env = env;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
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

        if(string.IsNullOrEmpty(HttpContext.Session.GetString("IdUsuario"))){
            return RedirectToAction("Index");
        }

        if (curso == null)
        {
            return NotFound();
        }

        ViewBag.Curso = curso;
        ViewBag.Tareas = bd.obtenerTareasCurso(Id);
        ViewBag.IdCurso = curso.IdCurso;

        ViewBag.NombreCurso = curso.Nombre;
        ViewBag.DescripcionCurso = curso.Descripcion;


        return View();
    }

    private string? GetApiKey()
    {
        var keys = new[]
        {
            _configuration["Groq:ApiKey"],
            _configuration["Groq__ApiKey"],
            _configuration["GrokCloud:ApiKey"],
            _configuration["GrokCloud__ApiKey"],
            _configuration["OpenAI:ApiKey"],
            _configuration["OpenAI__ApiKey"],
            Environment.GetEnvironmentVariable("GROQ_API_KEY"),
            Environment.GetEnvironmentVariable("GROK_API_KEY"),
            Environment.GetEnvironmentVariable("GROKCLOUD_API_KEY"),
            Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
            Environment.GetEnvironmentVariable("OpenAI_API_KEY")
        };

        return keys
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Select(k => k.Trim())
            .FirstOrDefault();
    }

    private string GetEndpoint()
    {
        return _configuration["Groq:Endpoint"]
            ?? _configuration["GrokCloud:Endpoint"]
            ?? _configuration["OpenAI:Endpoint"]
            ?? "https://api.groq.com/openai/v1/chat/completions";
    }

    private string GetModel()
    {
        return _configuration["Groq:Model"]
            ?? _configuration["GrokCloud:Model"]
            ?? _configuration["OpenAI:Model"]
            ?? "llama-3.1-8b-instant";
    }

    [HttpPost]
    public async Task<IActionResult> AskAI([FromBody] AskAiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Prompt))
        {
            return BadRequest(new { error = "La pregunta es obligatoria." });
        }

        var apiKey = GetApiKey();
        var model = GetModel();
        var endpoint = GetEndpoint();

        var placeholderValues = new[]
        {
            "TU_API_KEY_DE_OPENAI",
            "TU_API_KEY",
            "YOUR_API_KEY",
            "API_KEY_AQUI",
            "INSERT_YOUR_KEY_HERE",
            "TU_API_KEY_DE_GROKCLOUD"
        };

        if (string.IsNullOrWhiteSpace(apiKey) || placeholderValues.Any(v => string.Equals(v, apiKey, StringComparison.OrdinalIgnoreCase)))
        {
            return StatusCode(500, new { error = "Falta configurar la API key de GrokCloud/OpenAI. Agregala la clave en appsettings.json, user-secrets o variable de entorno GROK_API_KEY." });
        }

        using var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage response;
        string content;

        try
        {
            var payload = new
            {
                model,
                messages = new[]
                {
                    new { role = "system", content = "Responde como asesor de marketing y emprendimiento, en español, breve y útil." },
                    new { role = "user", content = request.Prompt }
                },
                temperature = 0.7,
                max_tokens = 400
            };

            response = await client.PostAsJsonAsync(endpoint, payload);
            content = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar la API de IA. Endpoint: {Endpoint}. Model: {Model}", endpoint, model);
            return StatusCode(500, new { error = $"No se pudo conectar con la IA: {ex.Message}" });
        }

        if (!response.IsSuccessStatusCode)
        {
            string message = "No pude contactar con la IA.";

            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("error", out var errorElement))
                {
                    if (errorElement.TryGetProperty("message", out var messageElement))
                    {
                        message = messageElement.GetString() ?? message;
                    }
                    else if (errorElement.ValueKind == JsonValueKind.String)
                    {
                        message = errorElement.GetString() ?? message;
                    }
                }
            }
            catch
            {
                message = content;
            }

            return StatusCode((int)response.StatusCode, new { error = message });
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            var choices = doc.RootElement.GetProperty("choices");
            var firstChoice = choices[0];
            var message = firstChoice.GetProperty("message");

            string? answer = null;

            if (message.TryGetProperty("content", out var contentElement))
            {
                if (contentElement.ValueKind == JsonValueKind.String)
                {
                    answer = contentElement.GetString();
                }
                else if (contentElement.ValueKind == JsonValueKind.Array)
                {
                    answer = string.Concat(contentElement.EnumerateArray()
                        .Select(e => e.TryGetProperty("text", out var textElement)
                            ? textElement.GetString() ?? ""
                            : ""));
                }
            }

            return Json(new { answer = answer ?? "No pude generar una respuesta." });
        }
        catch
        {
            return StatusCode(500, new { error = "La respuesta de la IA no tiene el formato esperado." });
        }
    }
}
