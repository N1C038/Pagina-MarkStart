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
        string query = @"SELECT IdUsuario, Email, Nombre, EsAdmin, Apellido, Telefono, FotoPerfil, Contraseña
                         FROM Usuarios
                         WHERE Email = @Email AND Contraseña = @Contraseña";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            return connection.QueryFirstOrDefault<Usuario>(query, new { Email = Email, Contraseña = Contraseña });
        }
    }

    public Usuario buscarUsuarioPorEmail(string Email)
    {
        string query = @"SELECT IdUsuario, Email, Nombre, EsAdmin, Apellido, Telefono, FotoPerfil, Contraseña
                         FROM Usuarios
                         WHERE Email = @Email";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            return connection.QueryFirstOrDefault<Usuario>(query, new { Email = Email });
        }
    }
    public Dictionary<Curso, Tarea> buscarCursosYTareasUsuario(int IdUsuario){
        Dictionary<Curso, Tarea> cursosYTareas = new Dictionary<Curso, Tarea>();
        string query = @"SELECT c.IdCurso, c.CantTareas, c.Precio, c.Nombre, c.Descripcion, c.FotoCurso,
                               t.IdTarea, t.Nombre, t.Explicacion, t.FotoTip, t.Tip, t.IdCurso 
                        FROM Usuarios u 
                        INNER JOIN UsuariosXStartUp us ON u.IdUsuario = us.IdUsuario 
                        INNER JOIN StartUps s ON us.IdStartup = s.IdStartup 
                        INNER JOIN CursosComprados cc ON s.IdStartup = cc.IdStartup 
                        INNER JOIN Cursos c ON cc.IdCurso = c.IdCurso 
                        INNER JOIN Tareas t ON c.IdCurso = t.IdCurso
                        WHERE u.IdUsuario = @IdUsuario AND t.IdTarea = cc.CantCompletadas + 1";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            var result = connection.Query<Curso, Tarea, (Curso curso, Tarea tarea)>(query,
                (curso, tarea) => (curso, tarea),
                new { IdUsuario = IdUsuario },
                splitOn: "IdTarea");

           
            foreach (var item in result)
            {
                cursosYTareas.Add(item.curso, item.tarea);
            }
        }
        return cursosYTareas;
    }
    public List<Curso> obtenerCursos()
    {
        string query = "SELECT IdCurso, CantTareas, Precio, Nombre, Descripcion, FotoCurso FROM Cursos";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            return connection.Query<Curso>(query).ToList();
        }
    }
}