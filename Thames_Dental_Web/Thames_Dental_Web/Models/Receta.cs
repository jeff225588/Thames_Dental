namespace Thames_Dental_Web.Models
{
    public class Receta
    {

        public int Id { get; set; }

        public string Paciente { get; set; } = string.Empty;
        public string Medicamento { get; set; } = string.Empty;
        public string Instrucciones { get; set; } = string.Empty;

    }
}
