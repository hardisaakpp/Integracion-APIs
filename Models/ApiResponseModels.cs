namespace IntegracionKoach360.Models
{
    public class AuthResponse
    {
        public string token { get; set; } = string.Empty;
        public Persona persona { get; set; } = new Persona();
    }

    public class Persona
    {
        public int personaId { get; set; }
        public string nombre { get; set; } = string.Empty;
        public string apellido { get; set; } = string.Empty;
        public string correo { get; set; } = string.Empty;
        public int rolId { get; set; }
        public bool estado { get; set; }
        public int clienteId { get; set; }
        public string tipoUsuario { get; set; } = string.Empty;
    }

    public class VentasResponse
    {
        public string mensaje { get; set; } = string.Empty;
        public int ventasExitosas { get; set; }
        public int ventasFallidas { get; set; }
        public string[] errores { get; set; } = Array.Empty<string>();
    }

    public class AsistenciaResponse
    {
        public string[] mensajes { get; set; } = Array.Empty<string>();
        public Estadisticas estadisticas { get; set; } = new Estadisticas();
    }

    public class Estadisticas
    {
        public int AsistenciasCreadas { get; set; }
        public int AsistenciasEliminadas { get; set; }
        public int AsistenciasActualizadas { get; set; }
        public int PersonasCreadas { get; set; }
        public int CargosCreados { get; set; }
    }
}
