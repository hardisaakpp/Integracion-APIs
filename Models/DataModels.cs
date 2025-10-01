using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IntegracionKoach360.Models
{
    public class VentaData
    {
        [JsonPropertyName("AsesorCedula")]
        [Required] public string asesorCedula { get; set; } = string.Empty;
        
        [JsonPropertyName("AsesorCorreo")]
        [Required] public string asesorCorreo { get; set; } = string.Empty;
        
        [JsonPropertyName("AsesorNombre")]
        [Required] public string asesorNombre { get; set; } = string.Empty;
        
        [JsonPropertyName("ClaveApi")]
        [Required] public string claveApi { get; set; } = string.Empty;
        
        [JsonPropertyName("ClienteId")]
        [Required] public int clienteId { get; set; }
        
        [JsonPropertyName("FacturaFecha")]
        [Required] public DateTime facturaFecha { get; set; }
        
        [JsonPropertyName("FacturaHora")]
        [Required] public TimeSpan facturaHora { get; set; }
        
        [JsonPropertyName("FacturaNumero")]
        [Required] public string facturaNumero { get; set; } = string.Empty;
        
        [JsonPropertyName("LiderCedula")]
        [Required] public string liderCedula { get; set; } = string.Empty;
        
        [JsonPropertyName("FacturaOrigen")]
        [Required] public string facturaOrigen { get; set; } = string.Empty;
        
        [JsonPropertyName("LiderCorreo")]
        [Required] public string liderCorreo { get; set; } = string.Empty;
        
        [JsonPropertyName("LiderNombre")]
        [Required] public string liderNombre { get; set; } = string.Empty;
        
        [JsonPropertyName("LocalNombre")]
        [Required] public string localNombre { get; set; } = string.Empty;
        
        [JsonPropertyName("UsuarioApi")]
        [Required] public string usuarioApi { get; set; } = string.Empty;
        
        [JsonPropertyName("ValorTransaccion")]
        [Required] public decimal valorTransaccion { get; set; }
        
        [JsonPropertyName("CantidadUnidades")]
        [Required] public int cantidadUnidades { get; set; }
    }

    public class AsistenciaData
    {
        [JsonPropertyName("ClienteId")]
        [Required] public int clienteId { get; set; }
        
        [JsonPropertyName("AsesorNombre")]
        [Required] public string asesorNombre { get; set; } = string.Empty;
        
        [JsonPropertyName("AsesorCedula")]
        [Required] public string asesorCedula { get; set; } = string.Empty;
        
        [JsonPropertyName("AsesorCargo")]
        [Required] public string asesorCargo { get; set; } = string.Empty;
        
        [JsonPropertyName("AsesorCorreo")]
        [Required] public string asesorCorreo { get; set; } = string.Empty;
        
        [JsonPropertyName("Fecha")]
        [Required] public string fecha { get; set; } = string.Empty;
        
        [JsonPropertyName("Hora")]
        [Required] public string hora { get; set; } = string.Empty;
        
        [JsonPropertyName("LocalNombre")]
        [Required] public string localNombre { get; set; } = string.Empty;
        
        [JsonPropertyName("UsuarioApi")]
        [Required] public string usuarioApi { get; set; } = string.Empty;
        
        [JsonPropertyName("ClaveApi")]
        [Required] public string claveApi { get; set; } = string.Empty;
    }
}
