namespace MarkStart.Models;
using Microsoft.Data.SqlClient;
using Dapper;
public class BD
{
    private string conexion = @"Server=localhost;Database=BD_Markstart;Integrated Security=True;TrustServerCertificate=True;";

    public void agregarUsuario (Usuario u)
    {
        string query = @"INSERT INTO Usuarios (Email, Nombre, EsAdmin, Apellido, Telefono, FotoPerfil, Contraseña)
                         VALUES (@Email, @Nombre, @EsAdmin, @Apellido, @Telefono, @FotoPerfil, @Contraseña)";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            connection.Execute(query, new
            {
                Email = u.Email,
                Nombre = u.Nombre,
                EsAdmin = u.EsAdmin,
                Apellido = u.Apellido,
                Telefono = u.Telefono,
                FotoPerfil = u.FotoPerfil,
                Contraseña = u.Contraseña
            });
        }
    }

    public Usuario encontrarUsuario(string Email, string Contraseña)
    {
        Usuario usuario = new Usuario();
        string query = @"SELECT IdUsuario, Email, Nombre, EsAdmin, Apellido, Telefono, FotoPerfil, Contraseña
                         FROM Usuarios
                         WHERE Email = @Email AND Contraseña = @Contraseña";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            usuario = connection.QueryFirstOrDefault<Usuario>(query, new { Email = Email, Contraseña = Contraseña });
        }
        return usuario;
    }

    public Usuario buscarUsuarioPorEmail(string Email)
    {
        Usuario usuario = new Usuario();
        string query = @"SELECT IdUsuario, Email, Nombre, EsAdmin, Apellido, Telefono, FotoPerfil, Contraseña
                         FROM Usuarios
                         WHERE Email = @Email";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            usuario = connection.QueryFirstOrDefault<Usuario>(query, new { Email = Email });
        }
        return usuario;
    }
    public Dictionary<Curso, Tarea> buscarCursosYTareasUsuario(int IdUsuario){
        Dictionary<Curso, Tarea> cursosYTareas = new Dictionary<Curso, Tarea>();
         string query = @"WITH TareasOrdenadas AS (
                    SELECT t.IdTarea, t.Nombre, t.Explicacion, t.FotoTip, t.Tip, t.IdCurso,
                        ROW_NUMBER() OVER (PARTITION BY t.IdCurso ORDER BY t.IdTarea) AS NumeroTarea
                    FROM Tareas t
                   )
                   SELECT c.IdCurso, c.CantTareas, c.Precio, c.Nombre, c.Descripcion, c.FotoCurso,
                       t.IdTarea, t.Nombre, t.Explicacion, t.FotoTip, t.Tip, t.IdCurso
                   FROM Usuarios u
                   INNER JOIN UsuariosXStartUp us ON u.IdUsuario = us.IdUsuario
                   INNER JOIN StartUps s ON us.IdStartup = s.IdStartup
                   INNER JOIN CursosComprados cc ON s.IdStartup = cc.IdStartup
                   INNER JOIN Cursos c ON cc.IdCurso = c.IdCurso
                   LEFT JOIN TareasOrdenadas t ON c.IdCurso = t.IdCurso AND t.NumeroTarea = cc.CantCompletadas + 1
                   WHERE u.IdUsuario = @IdUsuario";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            var result = connection.Query<Curso, Tarea, (Curso curso, Tarea tarea)>(query,
                (curso, tarea) => (curso, tarea),
                new { IdUsuario = IdUsuario },
                splitOn: "IdTarea");
           
            foreach (var item in result)
            {
                Tarea tarea = item.tarea;
                if (tarea != null && tarea.IdTarea == 0)
                {
                    tarea = null;
                }
                cursosYTareas.Add(item.curso, tarea);
            }
        }
        return cursosYTareas;
    }
    public List<Curso> obtenerCursos()
    {
        List<Curso> cursos = new List<Curso>();
        string query = "SELECT IdCurso, CantTareas, Precio, Nombre, Descripcion, FotoCurso FROM Cursos";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            cursos = connection.Query<Curso>(query).ToList();
        }
        return cursos;
    }
    public List<Curso> obtener3Cursos(){
        List<Curso> cursos = new List<Curso>();
        string query = "SELECT TOP 3 IdCurso, CantTareas, Precio, Nombre, Descripcion, FotoCurso FROM Cursos";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            cursos = connection.Query<Curso>(query).ToList();
        }
        return cursos;
    }
    public List<Startup> obtenerStartupsUsuario(int IdUsuario)
    {
        string query = @"SELECT s.IdStartup, s.Nombre, s.Problematica, s.NichoDeMercado, s.Producto
                         FROM StartUps s
                         INNER JOIN UsuariosXStartUp us ON s.IdStartup = us.IdStartup
                         WHERE us.IdUsuario = @IdUsuario";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            return connection.Query<Startup>(query, new { IdUsuario = IdUsuario }).ToList();
        }
    }
    public List<Curso> obtenerCursosStartup(int IdStartup)
    {
        List<Curso> cursos = new List<Curso>();
        string query = @"SELECT c.IdCurso, c.CantTareas, c.Precio, c.Nombre, c.Descripcion, c.FotoCurso
                         FROM Cursos c
                         INNER JOIN CursosComprados cc ON c.IdCurso = cc.IdCurso
                         WHERE cc.IdStartup = @IdStartup";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            cursos = connection.Query<Curso>(query, new { IdStartup = IdStartup }).ToList();
        }
        return cursos;
    }

    public List<Reseña> obtener3Reseñas()
    {
        List<Reseña> reseñas = new List<Reseña>();
        string query = @"SELECT TOP 3 r.IdReseña,
                                 u.Nombre,
                                 r.Texto AS Texto,
                                 u.FotoPerfil AS Imagen
                         FROM Reseñas r
                         INNER JOIN Usuarios u ON r.IdUsuario = u.IdUsuario
                         ORDER BY r.Calificacion DESC";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            reseñas = connection.Query<Reseña>(query).ToList();
        }
        return reseñas;
    }
    public List<Reseña> obtenerReseñasCurso(int IdCurso)
    {
        List<Reseña> reseñas = new List<Reseña>();
        string query = @"SELECT r.IdReseña,
                                u.Nombre,
                                r.Texto AS Texto,
                                u.FotoPerfil AS FotoPerfil,
                                r.Calificacion
                         FROM Reseñas r
                         INNER JOIN Usuarios u ON r.IdUsuario = u.IdUsuario
                         WHERE r.IdCurso = @IdCurso
                         ORDER BY r.Calificacion DESC";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            reseñas = connection.Query<Reseña>(query, new { IdCurso = IdCurso }).ToList();
        }
        return reseñas;
    }
    public Curso buscarCursoPorId(int IdCurso)
    {
        Curso curso = new Curso();
        string query = @"SELECT IdCurso, CantTareas, Precio, Nombre, Descripcion, FotoCurso
                         FROM Cursos
                         WHERE IdCurso = @IdCurso";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            curso = connection.QueryFirstOrDefault<Curso>(query, new { IdCurso = IdCurso });
        }
        return curso;
    }
    public List<Tarea> obtenerTareasCurso(int IdCurso)
    {
        List<Tarea> tareas = new List<Tarea>();
        string query = @"SELECT t.IdTarea, t.Nombre, t.Descripcion, t.IdCurso
                         FROM Tareas t
                         WHERE t.IdCurso = @IdCurso";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            tareas = connection.Query<Tarea>(query, new { IdCurso = IdCurso }).ToList();
        }
        return tareas;
    }
}