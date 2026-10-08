namespace MarkStart.Models;

public class Tarea
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Explicacion { get; set; }
    public int IdCurso { get; set; }
    public string ImagenTip { get; set; }
    public string Tip { get; set; }

    Tarea(int id, string nombre, string explicacion, int idCurso, string imagenTip, string tip)
    {
        Id = id;
        Nombre = nombre;
        Explicacion = explicacion;
        IdCurso = idCurso;
        ImagenTip = imagenTip;
        Tip = tip;
    }
    Tarea()
    {

    }
}