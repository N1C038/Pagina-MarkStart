namespace MarkStart.Models;
public class Curso
{
    public string Nombre {get; set;}
    public string Descripcion {get; set;}
    public int Id {get; set;}
    public int Precio {get; set;}
    public int CantidadTareas {get; set;}
    public string FotoCurso {get; set;}
    
    public Curso (string Nombre, string Descripcion, int Id, int Duracion, string Modalidad)
    {
        this.Nombre = Nombre;
        this.Descripcion = Descripcion;
        this.Id = Id;
        this.Precio = Precio;
        this.CantidadTareas = CantidadTareas;
        this.FotoCurso = FotoCurso;
    }
    public Curso ()
    {

    }
}