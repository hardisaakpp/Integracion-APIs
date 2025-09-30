using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IntegracionKoach360.Models
{
    public class VentaData
    {
        [JsonPropertyName("asesor_cedula")]
        [Required] public string asesorCedula { get; set; } = string.Empty;
        
        [JsonPropertyName("asesor_correo")]
        [Required] public string asesorCorreo { get; set; } = string.Empty;
        
        [JsonPropertyName("asesor_nombre")]
        [Required] public string asesorNombre { get; set; } = string.Empty;
        
        [JsonPropertyName("clave_api")]
        [Required] public string claveApi { get; set; } = string.Empty;
        
        [JsonPropertyName("cliente_id")]
        [Required] public int clienteId { get; set; }
        
        [JsonPropertyName("factura_fecha")]
        [Required] public string facturaFecha { get; set; } = string.Empty;
        
        [JsonPropertyName("factura_hora")]
        [Required] public string facturaHora { get; set; } = string.Empty;
        
        [JsonPropertyName("factura_numero")]
        [Required] public string facturaNumero { get; set; } = string.Empty;
        
        [JsonPropertyName("lider_cedula")]
        [Required] public string liderCedula { get; set; } = string.Empty;
        
        [JsonPropertyName("factura_origen")]
        [Required] public string facturaOrigen { get; set; } = string.Empty;
        
        [JsonPropertyName("lider_correo")]
        [Required] public string liderCorreo { get; set; } = string.Empty;
        
        [JsonPropertyName("lider_nombre")]
        [Required] public string liderNombre { get; set; } = string.Empty;
        
        [JsonPropertyName("local_nombre")]
        [Required] public string localNombre { get; set; } = string.Empty;
        
        [JsonPropertyName("usuario_api")]
        [Required] public string usuarioApi { get; set; } = string.Empty;
        
        [JsonPropertyName("valor_transaccion")]
        [Required] public decimal valorTransaccion { get; set; }
        
        [JsonPropertyName("cantidad_unidades")]
        [Required] public int cantidadUnidades { get; set; }
    }

    public class AsistenciaData
    {
        [JsonPropertyName("cliente_id")]
        [Required] public int clienteId { get; set; }
        
        [JsonPropertyName("asesor_nombre")]
        [Required] public string asesorNombre { get; set; } = string.Empty;
        
        [JsonPropertyName("asesor_cedula")]
        [Required] public string asesorCedula { get; set; } = string.Empty;
        
        [JsonPropertyName("asesor_cargo")]
        [Required] public string asesorCargo { get; set; } = string.Empty;
        
        [JsonPropertyName("asesor_correo")]
        [Required] public string asesorCorreo { get; set; } = string.Empty;
        
        [JsonPropertyName("fecha")]
        [Required] public string fecha { get; set; } = string.Empty;
        
        [JsonPropertyName("hora")]
        [Required] public string hora { get; set; } = string.Empty;
        
        [JsonPropertyName("local_nombre")]
        [Required] public string localNombre { get; set; } = string.Empty;
        
        [JsonPropertyName("usuario_api")]
        [Required] public string usuarioApi { get; set; } = string.Empty;
        
        [JsonPropertyName("clave_api")]
        [Required] public string claveApi { get; set; } = string.Empty;
    }
}
