namespace MarkStart.Models;
using Microsoft.Data.SqlClient;
using Dapper;
public class BD
{
    private string conexion = @"Server=localhost;DataBase=TP05; Integrated Security=True; TrustServerCertificate=True;";
    public void agregarUsuario (Usuario u)
    {
        Console.WriteLine(u.NombreUsuario);
        string query = "INSERT INTO Usuarios (Nombre,Apellido,NombreUsuario,Contraseña,Id) VALUES (@Nombre,@Apellido,@NombreUsuario,@Contraseña,@Id)";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            connection.Execute(query, new {nombre = u.Nombre, apellido = u.Apellido, usuario = u.NombreUsuario, clave = u.Contraseña, tipo = u.Id});
        }
    }

    public Usuario encontrarUsuario(string NombreUsuario, string Contraseña)
    {
        string query = "SELECT id, nombre, apellido, usuario, clave, tipo FROM Usuarios WHERE NombreUsuario = @NombreUsuario AND Contraseña = @Contraseña";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            return connection.QueryFirstOrDefault<Usuario>(query, new { NombreUsuario = NombreUsuario, Contraseña = Contraseña });
        }
    }

    public Usuario buscarPorNombreUsuario(string NombreUsuario)
    {
        string query = "SELECT Nombre, apellido, NombreUsuario, Contraseña, Id FROM Usuarios WHERE NombreUsuario = @NombreUsuario";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            return connection.QueryFirstOrDefault<Usuario>(query, new { NombreUsuario = NombreUsuario });
        }
    }
    public List<Curso> obtenerCursos()
    {
        string query = "SELECT Nombre, Descripcion, Id, Precio, CantidadTareas, FotoCurso FROM Cursos";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            return connection.Query<Curso>(query).ToList();
        }
    }