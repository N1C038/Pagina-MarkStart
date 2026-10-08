namespace MarkStart.Models;
using Microsoft.Data.SqlClient;
using Dapper;
public class BD
{
    private string conexion = @"Server=localhost;DataBase=TP05; Integrated Security=True; TrustServerCertificate=True;";
    public void agregarUsuario (Usuario u)
    {
        string query = "INSERT INTO Usuarios (Nombre,Apellido,NombreUsuario,Contraseña,Id,Email,Telefono,FotoPerfil) VALUES (@Nombre,@Apellido,@NombreUsuario,@Contraseña,@Id,@Email,@Telefono,@FotoPerfil)";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            connection.Execute(query, new { Nombre = u.Nombre, Apellido = u.Apellido, NombreUsuario = u.Usuario, Contraseña = u.Contraseña, Id = u.Id, Email = u.Email, Telefono = u.Telefono, FotoPerfil = u.FotoPerfil });
        }
    }

    public Usuario encontrarUsuario(string Email, string Contraseña)
    {
<<<<<<< HEAD
        string query = "SELECT Id as Id, Nombre as Nombre, Apellido as Apellido, NombreUsuario as Usuario, Contraseña as Contraseña, Email as Email, Telefono as Telefono, FotoPerfil as FotoPerfil FROM Usuarios WHERE NombreUsuario = @NombreUsuario AND Contraseña = @Contraseña";
=======
        string query = "SELECT id, nombre, apellido, usuario, clave, tipo FROM Usuarios WHERE Email = @Email AND Contraseña = @Contraseña";
>>>>>>> 2b324eade225909f5127bd912b87759dfecc2655
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            return connection.QueryFirstOrDefault<Usuario>(query, new { Email = Email, Contraseña = Contraseña });
        }
    }

    public Usuario buscarPorNombreUsuario(string NombreUsuario)
    {
        string query = "SELECT Id as Id, Nombre as Nombre, Apellido as Apellido, NombreUsuario as Usuario, Contraseña as Contraseña, Email as Email, Telefono as Telefono, FotoPerfil as FotoPerfil FROM Usuarios WHERE NombreUsuario = @NombreUsuario";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            return connection.QueryFirstOrDefault<Usuario>(query, new { NombreUsuario = NombreUsuario });
        }
    }
<<<<<<< HEAD
    public Dictionary<Curso, Tarea> buscarCursosYTareasUsuario(int IdUsuario){
        
        string query = @"SELECT c.IdCurso, c.Nombre AS NombreCurso, c.Descripcion, c.Precio, c.CantTareas, c.FotoCurso, 
                               t.Id AS IdTarea, t.Nombre AS NombreTarea, t.Explicacion, t.ImagenTip, t.Tip 
                        FROM Usuarios u 
                        INNER JOIN UsuariosStartup us ON u.Id = us.IdUsuario 
                        INNER JOIN Startups s ON us.IdStartup = s.IdStartup 
                        INNER JOIN CursosComprados cc ON s.IdStartup = cc.IdStartup 
                        INNER JOIN Cursos c ON cc.IdCurso = c.IdCurso 
                        INNER JOIN Tareas t ON c.IdCurso = t.IdCurso
                        WHERE u.Id = @IdUsuario AND t.Id = cc.UltimaTareaRealizada + 1";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            var result = connection.Query<Curso, Tarea, (Curso curso, Tarea tarea)>(query,
                (curso, tarea) => (curso, tarea),
                new { IdUsuario = IdUsuario },
                splitOn: "IdTarea");

            Dictionary<Curso, Tarea> cursosYTareas = new Dictionary<Curso, Tarea>();
            foreach (var item in result)
            {
                cursosYTareas.Add(item.curso, item.tarea);
            }
        }
        return cursosYTareas;
=======
    public List<Curso> obtenerCursos()
    {
        string query = "SELECT Nombre, Descripcion, Id, Precio, CantidadTareas, FotoCurso FROM Cursos";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            return connection.Query<Curso>(query).ToList();
        }
>>>>>>> d83cfeb524a1f9397366260a755d7eb9ee4dba86
    }