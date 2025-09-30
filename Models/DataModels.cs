using System.ComponentModel.DataAnnotations;

namespace IntegracionKoach360.Models
{
    public class VentaData
    {
        [Required] public string asesorCedula { get; set; } = string.Empty;
        [Required] public string asesorCorreo { get; set; } = string.Empty;
        [Required] public string asesorNombre { get; set; } = string.Empty;
        [Required] public string claveApi { get; set; } = string.Empty;
        [Required] public int clienteId { get; set; }
        [Required] public string facturaFecha { get; set; } = string.Empty;
        [Required] public string facturaHora { get; set; } = string.Empty;
        [Required] public string facturaNumero { get; set; } = string.Empty;
        [Required] public string liderCedula { get; set; } = string.Empty;
        [Required] public string facturaOrigen { get; set; } = string.Empty;
        [Required] public string liderCorreo { get; set; } = string.Empty;
        [Required] public string liderNombre { get; set; } = string.Empty;
        [Required] public string localNombre { get; set; } = string.Empty;
        [Required] public string usuarioApi { get; set; } = string.Empty;
        [Required] public decimal valorTransaccion { get; set; }
        [Required] public int cantidadUnidades { get; set; }
    }

    public class AsistenciaData
    {
        [Required] public int clienteId { get; set; }
        [Required] public string asesorNombre { get; set; } = string.Empty;
        [Required] public string asesorCedula { get; set; } = string.Empty;
        [Required] public string asesorCargo { get; set; } = string.Empty;
        [Required] public string asesorCorreo { get; set; } = string.Empty;
        [Required] public string fecha { get; set; } = string.Empty;
        [Required] public string hora { get; set; } = string.Empty;
        [Required] public string localNombre { get; set; } = string.Empty;
        [Required] public string usuarioApi { get; set; } = string.Empty;
        [Required] public string claveApi { get; set; } = string.Empty;
    }
}
