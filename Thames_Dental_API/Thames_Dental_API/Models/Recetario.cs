namespace Thames_Dental_API.Models
{
    public class Recetario
    {

        public int Id { get; set; }

        public string Paciente { get; set; } = string.Empty;
        public string Medicamento { get; set; } = string.Empty;
        public string Instrucciones { get; set; } = string.Empty;


    }
}
