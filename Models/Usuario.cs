namespace MarkStart.Models;
public class Usuario
{
    public string Nombre {get; set;}
    public string Apellido {get; set;}
    public string Usuario {get; set;}
    public string Contraseña {get; set;}
    public int Id {get; set;}
    public string Email {get; set;}
    public int Telefono {get; set;}
    
    public Usuario (string Nombre, string Apellido, string Usuario, string Contraseña, int Id, string Email, int Telefono)
    {
        this.Nombre = Nombre;
        this.Apellido = Apellido;
        this.Usuario = Usuario;
        this.Contraseña = Contraseña;
        this.Id = Id;
        this.Email = Email;
        this.Telefono = Telefono;
    }
    public Usuario ()
    {

    }
}