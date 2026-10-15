namespace MarkStart.Models;

public class Tarea
{
    public int IdTarea { get; set; }
    public string Nombre { get; set; }
    public string Explicacion { get; set; }
    public int IdCurso { get; set; }
    public string FotoTip { get; set; }
    public string Tip { get; set; }

    public Tarea()
    {
    }
}