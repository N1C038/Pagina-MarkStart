namespace MarkStart.Models;
public class Curso
{
    public int IdCurso { get; set; }
    public int CantTareas { get; set; }
    public double Precio { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public string FotoCurso { get; set; }

    public Curso()
    {
    }
}