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

    public Usuario encontrarUsuario(string NombreUsuario, string Contraseña)
    {
        string query = "SELECT Id as Id, Nombre as Nombre, Apellido as Apellido, NombreUsuario as Usuario, Contraseña as Contraseña, Email as Email, Telefono as Telefono, FotoPerfil as FotoPerfil FROM Usuarios WHERE NombreUsuario = @NombreUsuario AND Contraseña = @Contraseña";
        using (SqlConnection connection = new SqlConnection(conexion))
        {
            return connection.QueryFirstOrDefault<Usuario>(query, new { NombreUsuario = NombreUsuario, Contraseña = Contraseña });
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