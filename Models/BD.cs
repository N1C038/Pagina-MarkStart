namespace MarkStart.Models;
using Microsoft.Data.SqlClient;
using Dapper;
public class BD
{
    private string conexion = @"Server=localhost;DataBase=TP05; Integrated Security=True; TrustServerCertificate=True;";
    public void agregarUsuario (Usuario u)
    {
<<<<<<< HEAD
        string query = "INSERT INTO Usuarios (Nombre,Apellido,Contraseña,Id) VALUES (@Nombre,@Apellido,@Contraseña,@Id)";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            connection.Execute(query, new {nombre = u.Nombre, apellido = u.Apellido, clave = u.Contraseña, tipo = u.Id});
=======
        string query = "INSERT INTO Usuarios (Nombre,Apellido,NombreUsuario,Contraseña,Id,Email,Telefono,FotoPerfil) VALUES (@Nombre,@Apellido,@NombreUsuario,@Contraseña,@Id,@Email,@Telefono,@FotoPerfil)";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            connection.Execute(query, new { Nombre = u.Nombre, Apellido = u.Apellido, NombreUsuario = u.Usuario, Contraseña = u.Contraseña, Id = u.Id, Email = u.Email, Telefono = u.Telefono, FotoPerfil = u.FotoPerfil });
>>>>>>> 456a7160cce5fb72a2bf49670b4e6eff7b1a05ae
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

    public Usuario buscarPorNombreUsuario(string Email)
    {
<<<<<<< HEAD
        string query = "SELECT Nombre, apellido, Email, Contraseña, Id FROM Usuarios WHERE Email = @Email";
=======
        string query = "SELECT Id as Id, Nombre as Nombre, Apellido as Apellido, NombreUsuario as Usuario, Contraseña as Contraseña, Email as Email, Telefono as Telefono, FotoPerfil as FotoPerfil FROM Usuarios WHERE NombreUsuario = @NombreUsuario";
>>>>>>> 456a7160cce5fb72a2bf49670b4e6eff7b1a05ae
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            return connection.QueryFirstOrDefault<Usuario>(query, new { Email = Email });
        }
    }
    public Dictionary<Curso, Tarea> buscarCursosYTareasUsuario(int IdUsuario){
        Dictionary<Curso, Tarea> cursosYTareas = new Dictionary<Curso, Tarea>();
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

           
            foreach (var item in result)
            {
                cursosYTareas.Add(item.curso, item.tarea);
            }
        }
        return cursosYTareas;
    }
    public List<Curso> obtenerCursos()
    {
        string query = "SELECT Nombre, Descripcion, Id, Precio, CantidadTareas, FotoCurso FROM Cursos";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            return connection.Query<Curso>(query).ToList();
        }
    }
}