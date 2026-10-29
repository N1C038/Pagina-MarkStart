namespace MarkStart.Models;

public class Reseña
{
    public int IdReseña { get; set; }
    public string Nombre { get; set; }
    public string Texto { get; set; }
    public string FotoPerfil { get; set; }
    public int Calificacion { get; set; }

    public Reseña()
    {
    }
}