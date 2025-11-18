namespace AppVidaMinisterio.Models
{
    public class Semana
    {
        public string? SemanaFecha { get; set; }
        public string? TextoBiblico { get; set; }  
        public string? Presidente { get; set; } = "Presidente: ";
        public bool IsVisit { get; set; } = false; // Indica si la semana es visita del Superintendente de circuito
        public string? DiscursoVisita { get; set; }
        public bool IsUpdate { get; set; } = false; // Indica si en la semana se pone el informe del cuerpo gobernante
        public string? VideoInforme { get; set; }
        public bool CleaningDuty { get; set; } = true; // Indica si hay que hacer limpieza en el Salón del Reino
        public string? Grupos { get; set; } = "Aseo Grupos: ";

        public Canciones Cancion { get; init; }
        public Oradores Orador { get; init; }
        public TesorosDeLaBiblia Tesoros { get; init; }
        public SeamosMejoresMaestros MejoresMaestros { get; init; }
        public NuestraVidaCristiana VidaCristiana { get; init; }

        public Semana()
        {
            Cancion = new Canciones();
            Orador = new Oradores();
            Tesoros = new TesorosDeLaBiblia();
            MejoresMaestros = new SeamosMejoresMaestros();
            VidaCristiana = new NuestraVidaCristiana();
        }

        public class Canciones
        {
            public string? Inicio { get; set; }
            public string? Intermedio { get; set; }
            public string? Final { get; set; }
        }

        public class Oradores
        {
            public string? Inicial { get; set; } = "Oración inicial: ";
            public string? Final { get; set; } = "Oración final: ";
        }

        public class TesorosDeLaBiblia
        {
            public string? TituloTesoros { get; set; }
            public string? OradorTesoros { get; set; }

            public string? ConductorPerlas { get; set; }

            public string? DetallesLecturaBiblica { get; set; }
            public string? LectorBiblia { get; set; }
        }

        public class SeamosMejoresMaestros
        {
            public string? Asignacion1 { get; set; }
            public string? Asignado1 { get; set; }
            public string? Detalles1 { get; set; }

            public string? Asignacion2 { get; set; }
            public string? Asignado2 { get; set; }
            public string? Detalles2 { get; set; }

            public string? Asignacion3 { get; set; }
            public string? Asignado3 { get; set; }
            public string? Detalles3 { get; set; }

            public string? Asignacion4 { get; set; }
            public string? Asignado4 { get; set; }
            public string? Detalles4 { get; set; }
        }

        public class NuestraVidaCristiana
        {
            public string? Asignacion1 { get; set; }
            public string? Asignado1 { get; set; }
            public string? Detalles1 { get; set; }

            public string? Asignacion2 { get; set; }
            public string? Asignado2 { get; set; }
            public string? Detalles2 { get; set; }

            public string? EstudioBiblico { get; set; }
            public string? ConductorYLectorEstudioBiblico { get; set; } = "Conductor: \nLector: ";
            public string? DetallesEstudioBiblico { get; set; }
        }
    }
}
