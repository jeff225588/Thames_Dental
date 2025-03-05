namespace Thames_Dental_Web.Models
{
    public class ClientModel
    {

        public int ClienteID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string SegundoApellido { get; set; } = string.Empty;
        public string TipoIdentificacion { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string TelefonoPrincipal { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string DireccionExacta { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Genero { get; set; } = string.Empty;
        public string Ocupacion { get; set; } = string.Empty;
        public string ContactoEmergencia { get; set; } = string.Empty;
        public string TelefonoEmergencia { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public string Notas { get; set; } = string.Empty;

        public bool Estado { get; set; } = true;


        public bool nuevoEstado { get; set; }


        public List<RecetaRequest> Recetas { get; set; }

        public List<Autorizado> Autorizado { get; set; }

        public List<HistorialClinicoporid> HistorialClinicoporid { get; set; }

        public List<HistorialClinicoRequest> HistorialClinicoRequest { get; set; }

        public List<CotizacionModel> CotizacionModel { get; set; } = new List<CotizacionModel>();  // Asegúrate de que esté inicializado

        public List<Nota> Nota { get; set; } = new List<Nota>();


    }




    public class Nota
    {
        public int ClienteID { get; set; }
        public DateTime Fecha { get; set; }
        public string Detalle { get; set; } = string.Empty; // Detalle de la nota
        public string Cedula { get; set; } = string.Empty; // Campo para la cédula
    }


    public class Autorizado
    {
        public int ClienteID { get; set; }
        public string Cedula { get; set; } = string.Empty; // Cédula del cliente
        public string Nombre { get; set; } = string.Empty; // Nombre del autorizado
        public string TelefonoEmergencia { get; set; } = string.Empty; // Teléfono de emergencia
    }


    public class RecetaRequest
    {




        public int ClienteID { get; set; }
        public int Id { get; set; }
        public string Identificacion { get; set; } = string.Empty; // Cédula del cliente
        public string Medicamento { get; set; } = string.Empty;  // Medicamento recetado
        public string Instrucciones { get; set; } = string.Empty; // Instrucciones de la receta
    }



    public class CondicionMedica
    {
        public string Nombre { get; set; } = string.Empty; // Nombre de la condición
        public bool Seleccionado { get; set; } // Estado de la condición (true/false)
    }


    public class HistorialClinicoRequest
    {

        public int ClienteID { get; set; }
        public string CedulaPaciente { get; set; } = string.Empty; // Cédula del paciente
        public string TipoSangre { get; set; } = string.Empty;   // Tipo de sangre
        public string Anotaciones { get; set; } = string.Empty;  // Anotaciones generales
        public bool DolorMolestia { get; set; }    // Presenta algún dolor o molestia
        public bool Nervioso { get; set; }         // Se siente nervioso por el tratamiento
        public bool AtencionMedica { get; set; }   // Bajo atención médica en los últimos 2 años
        public bool Medicamentos { get; set; }     // Ha ingerido medicamentos en los últimos 2 años
        public bool Alergico { get; set; }         // Es alérgico a medicamentos
        public bool SangradoExcesivo { get; set; } // Sangrado excesivo que ha requerido atención
        public bool ReaccionAnestesica { get; set; } // Reacción alérgica con la anestesia

        // Condiciones médicas
        public bool FallaCardiaca { get; set; }
        public bool Infarto { get; set; }
        public bool AnginaPecho { get; set; }
        public bool PresionAlta { get; set; }
        public bool SoploCorazon { get; set; }
        public bool LesionesCardiacasCongenitas { get; set; }
        public bool ValvulasCardiacasArtificiales { get; set; }
        public bool Marcapasos { get; set; }
        public bool OperacionCorazon { get; set; }
        public bool Transplante { get; set; }
        public bool HerpesLabial { get; set; }
        public bool Anemia { get; set; }
        public bool DerrameCerebral { get; set; }
        public bool ProblemasRinon { get; set; }
        public bool UlcerasGastritis { get; set; }
        public bool EnfisemaPulmonar { get; set; }
        public bool TosFrecuente { get; set; }
        public bool VIHPositivo { get; set; }
        public bool EnfermedadVenera { get; set; }
        public bool Asma { get; set; }
        public bool Tuberculosis { get; set; }
        public bool FiebreReumatica { get; set; }
        public bool Sinusitis { get; set; }
        public bool Alergias { get; set; }
        public bool Diabetes { get; set; }
        public bool ProblemasTiroides { get; set; }
        public bool Radioterapia { get; set; }
        public bool Quimioterapia { get; set; }
        public bool Artritis { get; set; }
        public bool Reumatismo { get; set; }
        public bool TratamientoPsiquiatrico { get; set; }
        public bool DolorUnionMandibular { get; set; }
        public bool HepatitisA { get; set; }
        public bool HepatitisB { get; set; }
        public bool HepatitisC { get; set; }

        public bool ProblemasHepaticos { get; set; }
        public bool TransfusionSangre { get; set; }
        public bool EpilepsiaConvulsiones { get; set; }
        public bool Desmayos { get; set; }
        public bool Nerviosismo { get; set; }
        public bool AparicionHematomas { get; set; }
        public bool OtraCondicion { get; set; }

        public string MedicamentosUltimos2Meses { get; set; } = string.Empty; // Medicamentos en los últimos 2 meses
        public string Operaciones { get; set; } = string.Empty;   // Anotaciones sobre operaciones
    }


    public class HistorialClinicoporid
    {
        public int HistorialID { get; set; }
        public string CedulaPaciente { get; set; } = string.Empty;
        public int ClienteID { get; set; }
        public string TipoSangre { get; set; } = string.Empty;
        public string Anotaciones { get; set; } = string.Empty;

        // Propiedades convertidas de bit a string ("Sí"/"No")
        public string DolorMolestia { get; set; } = string.Empty;
        public string Nervioso { get; set; } = string.Empty;
        public string AtencionMedica { get; set; } = string.Empty;
        public string Medicamentos { get; set; } = string.Empty;
        public string Alergico { get; set; } = string.Empty;
        public string SangradoExcesivo { get; set; } = string.Empty;
        public string ReaccionAnestesica { get; set; } = string.Empty;

        public string MedicamentosUltimos2Meses { get; set; } = string.Empty;
        public string Operaciones { get; set; } = string.Empty;

        // Más propiedades convertidas de bit a string
        public string FallaCardiaca { get; set; } = string.Empty;
        public string Infarto { get; set; } = string.Empty;
        public string AnginaPecho { get; set; } = string.Empty;
        public string PresionAlta { get; set; } = string.Empty;
        public string SoploCorazon { get; set; } = string.Empty;
        public string LesionesCardiacasCongenitas { get; set; } = string.Empty;
        public string ValvulasCardiacasArtificiales { get; set; } = string.Empty;
        public string Marcapasos { get; set; } = string.Empty;
        public string OperacionCorazon { get; set; } = string.Empty;
        public string Transplante { get; set; } = string.Empty;
        public string HerpesLabial { get; set; } = string.Empty;
        public string Anemia { get; set; } = string.Empty;
        public string DerrameCerebral { get; set; } = string.Empty;
        public string ProblemasRinon { get; set; } = string.Empty;
        public string UlcerasGastritis { get; set; } = string.Empty;
        public string EnfisemaPulmonar { get; set; } = string.Empty;
        public string TosFrecuente { get; set; } = string.Empty;
        public string VIHPositivo { get; set; } = string.Empty;
        public string EnfermedadVenera { get; set; } = string.Empty;
        public string Asma { get; set; } = string.Empty;
        public string Tuberculosis { get; set; } = string.Empty;
        public string FiebreReumatica { get; set; } = string.Empty;
        public string Sinusitis { get; set; } = string.Empty;
        public string Alergias { get; set; } = string.Empty;
        public string Diabetes { get; set; } = string.Empty;
        public string ProblemasTiroides { get; set; } = string.Empty;
        public string Radioterapia { get; set; } = string.Empty;
        public string Quimioterapia { get; set; } = string.Empty;
        public string Artritis { get; set; } = string.Empty;
        public string Reumatismo { get; set; } = string.Empty;
        public string TratamientoPsiquiatrico { get; set; } = string.Empty;
        public string DolorUnionMandibular { get; set; } = string.Empty;
        public string HepatitisA { get; set; } = string.Empty;
        public string HepatitisB { get; set; } = string.Empty;
        public string HepatitisC { get; set; } = string.Empty;
        public string ProblemasHepaticos { get; set; } = string.Empty;
        public string TransfusionSangre { get; set; } = string.Empty;
        public string EpilepsiaConvulsiones { get; set; } = string.Empty;
        public string Desmayos { get; set; } = string.Empty;
        public string Nerviosismo { get; set; } = string.Empty;
        public string AparicionHematomas { get; set; } = string.Empty;

        public string OtraCondicion { get; set; } = string.Empty;
    }


    public class CotizacionModel
    {
        public int ClienteID { get; set; }
        public string TextoCotizacion { get; set; }
    }


    public class AgregarAutorizadoViewModel
    {
        public int ClienteID { get; set; }
        public string Nombre { get; set; }
        public string TelefonoEmergencia { get; set; }

        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }
    }
}



