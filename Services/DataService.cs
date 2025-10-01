using System.Data;
using Microsoft.Data.SqlClient;
using IntegracionKoach360.Interfaces;
using IntegracionKoach360.Models;

namespace IntegracionKoach360.Services
{
    public class DataService : IDataService
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILoggingService _loggingService;
        private readonly ConfiguracionApp _config;

        public DataService(IDatabaseService databaseService, ILoggingService loggingService, ConfiguracionApp config)
        {
            _databaseService = databaseService;
            _loggingService = loggingService;
            _config = config;
        }

        public async Task<VentaData[]?> LoadVentasAsync()
        {
            return await _databaseService.GetVentasAsync();
        }

        public async Task<AsistenciaData[]?> LoadAsistenciasAsync()
        {
            return await _databaseService.GetAsistenciasAsync();
        }

        public Task<VentaData[]> ValidateAndCompleteVentasAsync(VentaData[] ventas)
        {
            var ventasValidas = new List<VentaData>();

            foreach (var venta in ventas)
            {
                // Completar campos desde configuración si están vacíos
                if (venta.clienteId == 0) venta.clienteId = _config.clienteId;
                if (string.IsNullOrEmpty(venta.usuarioApi)) venta.usuarioApi = _config.usuarioApi;
                if (string.IsNullOrEmpty(venta.claveApi)) venta.claveApi = _config.claveApi;

                // Validar campos requeridos
                if (ValidateVenta(venta))
                {
                    ventasValidas.Add(venta);
                }
                else
                {
                    _loggingService.Warning("Venta inválida - Factura: {FacturaNumero}", venta.facturaNumero);
                }
            }

            return Task.FromResult(ventasValidas.ToArray());
        }

        public Task<AsistenciaData[]> ValidateAndCompleteAsistenciasAsync(AsistenciaData[] asistencias)
        {
            var asistenciasValidas = new List<AsistenciaData>();

            foreach (var asistencia in asistencias)
            {
                // Completar campos desde configuración si están vacíos
                if (asistencia.clienteId == 0) asistencia.clienteId = _config.clienteId;
                if (string.IsNullOrEmpty(asistencia.usuarioApi)) asistencia.usuarioApi = _config.usuarioApi;
                if (string.IsNullOrEmpty(asistencia.claveApi)) asistencia.claveApi = _config.claveApi;

                // Validar campos requeridos
                if (ValidateAsistencia(asistencia))
                {
                    asistenciasValidas.Add(asistencia);
                }
                else
                {
                    _loggingService.Warning("Asistencia inválida - Nombre: {Nombre}, Cédula: {Cedula}, Cargo: {Cargo}, Local: {Local}, Correo: {Correo}", 
                        asistencia.asesorNombre ?? "NULL", 
                        asistencia.asesorCedula ?? "NULL",
                        asistencia.asesorCargo ?? "NULL",
                        asistencia.localNombre ?? "NULL",
                        asistencia.asesorCorreo ?? "NULL");
                }
            }

            return Task.FromResult(asistenciasValidas.ToArray());
        }

        private static bool ValidateVenta(VentaData venta)
        {
            return !string.IsNullOrEmpty(venta.asesorCedula) &&
                   !string.IsNullOrEmpty(venta.asesorNombre) &&
                   !string.IsNullOrEmpty(venta.facturaNumero) &&
                   venta.facturaFecha != DateTime.MinValue &&
                   !string.IsNullOrEmpty(venta.localNombre) &&
                   !string.IsNullOrEmpty(venta.usuarioApi) &&
                   !string.IsNullOrEmpty(venta.claveApi) &&
                   venta.clienteId > 0 &&
                   venta.valorTransaccion > 0;
        }

        private static bool ValidateAsistencia(AsistenciaData asistencia)
        {
            return !string.IsNullOrEmpty(asistencia.asesorCedula) &&
                   !string.IsNullOrEmpty(asistencia.asesorNombre) &&
                   !string.IsNullOrEmpty(asistencia.asesorCargo) &&
                   !string.IsNullOrEmpty(asistencia.asesorCorreo) &&
                   !string.IsNullOrEmpty(asistencia.fecha) &&
                   !string.IsNullOrEmpty(asistencia.hora) &&
                   !string.IsNullOrEmpty(asistencia.localNombre) &&
                   !string.IsNullOrEmpty(asistencia.usuarioApi) &&
                   !string.IsNullOrEmpty(asistencia.claveApi) &&
                   asistencia.clienteId > 0;
        }
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly ILoggingService _loggingService;
        private readonly ConfiguracionApp _config;

        public DatabaseService(ILoggingService loggingService, ConfiguracionApp config)
        {
            _loggingService = loggingService;
            _config = config;
        }

        public async Task<VentaData[]> GetVentasAsync()
        {
            var ventas = new List<VentaData>();

            string query = @"
                SELECT
                    factura_numero      = V.Documento,
                    factura_fecha       = CONVERT(VARCHAR, V.Fecha, 112),
                    factura_hora        = CONVERT(VARCHAR(8), V.Hora, 108),
                    factura_origen      = CASE V.TipoDocumento
                                            WHEN 'FC' THEN 'FAC'
                                            WHEN 'NC' THEN 'NC'
                                            ELSE 'ND'
                                            END,
                    asesor_nombre       = CASE V.TipoDocumento
                                            WHEN 'FC' THEN ISNULL(E.Apellido1 + ' ' + E.Apellido2 + ' ' + E.Nombre1 + ' ' + E.Nombre2, LTRIM(RTRIM(V.Vendedor)))
                                            WHEN 'NC' THEN 'NA'
                                            ELSE 'ND'
                                            END,
                    asesor_cedula       = CASE V.TipoDocumento
                                            WHEN 'FC' THEN ISNULL(E.Identificacion, LTRIM(RTRIM(V.CodVendedor)))
                                            WHEN 'NC' THEN 'NA'
                                            ELSE 'ND'
                                            END,
                    asesor_correo       = '-',
                    lider_nombre        = CASE V.CodTienda
                                            WHEN 'RL-PSC'  THEN 'GOMEZ IMBAQUINGO SILVIA ANDREA'
                                            WHEN 'RL-QSS2' THEN 'CUEVA MESA CHRISTIAN ANTONIO'
                                            WHEN 'RL-SCA'  THEN 'TUTILLO BENAVIDES DIGNA EVELIN'
                                        END,
                    lider_cedula        = CASE V.CodTienda
                                            WHEN 'RL-PSC'  THEN '1722759469'
                                            WHEN 'RL-QSS2' THEN '1716633001'
                                            WHEN 'RL-SCA'  THEN '1715033674'
                                        END,
                    lider_correo        = CASE V.CodTienda
                                            WHEN 'RL-PSC'  THEN 'rolportjefe@mabeltrading.com.ec'
                                            WHEN 'RL-QSS2' THEN 'rolquisurjefe@mabeltrading.com.ec'
                                            WHEN 'RL-SCA'  THEN 'scala@mabeltrading.com.ec'
                                        END,
                    local_nombre        = V.Tienda,
                    valor_transaccion   = CONVERT(DECIMAL(18,2), V.Neto),
                    cantidad_unidades   = V.UnidadPorFactura
                FROM Analitica..DWH_VENTASGENERAL_VIEW V
                LEFT JOIN BISTAGING..STG_EMPLEADOS E 
                    ON V.CodVendedor = E.CodVendedor AND E.IdEmpresa = 1
                WHERE
                    V.Fecha = CAST(GETDATE() AS DATE)
                    AND V.Hora <= CONVERT(TIME, GETDATE())
                    AND V.CodVendedor NOT IN ('114', '1150')
                    AND V.CodTienda IN ('RL-PSC', 'RL-QSS2', 'RL-SCA')
                ORDER BY
                    V.CodTienda,
                    V.Fecha,
                    V.Documento";

            try
            {
                using var connection = new SqlConnection(_config.database.connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(query, connection);
                command.CommandTimeout = _config.database.commandTimeout;

                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    ventas.Add(new VentaData
                    {
                        facturaNumero = reader["factura_numero"].ToString() ?? string.Empty,
                        facturaFecha = DateTime.ParseExact(reader["factura_fecha"].ToString() ?? "", "yyyyMMdd", null),
                        facturaHora = TimeSpan.Parse(reader["factura_hora"].ToString() ?? "00:00:00"),
                        facturaOrigen = reader["factura_origen"].ToString() ?? string.Empty,
                        asesorNombre = reader["asesor_nombre"].ToString() ?? string.Empty,
                        asesorCedula = reader["asesor_cedula"].ToString() ?? string.Empty,
                        asesorCorreo = reader["asesor_correo"].ToString() ?? string.Empty,
                        liderNombre = reader["lider_nombre"].ToString() ?? string.Empty,
                        liderCedula = reader["lider_cedula"].ToString() ?? string.Empty,
                        liderCorreo = reader["lider_correo"].ToString() ?? string.Empty,
                        localNombre = reader["local_nombre"].ToString() ?? string.Empty,
                        valorTransaccion = Convert.ToDecimal(reader["valor_transaccion"]),
                        cantidadUnidades = Convert.ToInt32(reader["cantidad_unidades"]),
                        // Los campos de API se completan automáticamente después
                        clienteId = _config.clienteId,
                        usuarioApi = _config.usuarioApi,
                        claveApi = _config.claveApi
                    });
                }

                _loggingService.Information("Consulta de ventas ejecutada: {Count} registros obtenidos", ventas.Count);


                // LOG TEMPORAL
                var ventasConProblemas = ventas.Where(v => 
                    v.clienteId == 0 || 
                    string.IsNullOrEmpty(v.liderNombre) || 
                    string.IsNullOrEmpty(v.liderCedula) ||
                    string.IsNullOrEmpty(v.liderCorreo)
                ).ToList();
                
                if (ventasConProblemas.Any())
                {
                    _loggingService.Warning("Se encontraron {Count} ventas con datos incompletos", ventasConProblemas.Count);
                    foreach (var venta in ventasConProblemas.Take(5))
                    {
                        _loggingService.Warning("Venta problemática: Factura={Factura}, ClienteId={ClienteId}, Lider={Lider}, LiderCedula={LiderCedula}, LiderCorreo={LiderCorreo}",
                            venta.facturaNumero, venta.clienteId, venta.liderNombre, venta.liderCedula, venta.liderCorreo);
                    }
                }
                // FIN LOG TEMPORAL

                return ventas.ToArray();
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error al consultar ventas desde BD: {Message}", ex.Message);
                return Array.Empty<VentaData>();
            }
        }

        public async Task<AsistenciaData[]> GetAsistenciasAsync()
        {
            var asistencias = new List<AsistenciaData>();

            string query = @"
                DECLARE @hoy DATE = CAST(GETDATE() AS DATE);
                DECLARE @hace7 DATE = DATEADD(DAY, -7, @hoy);

                WITH AsistenciasUnicas AS (
                    SELECT DISTINCT
                        USERID,
                        Fecha = CONVERT(DATE, CHECKTIME),
                        Hora = CONVERT(TIME, CHECKTIME)
                    FROM ElRayoBiometricos.dbo.VistaRegistrosT
                    WHERE CHECKTIME BETWEEN @hace7 AND @hoy
                )

                SELECT
                    asesor_nombre   = k.kli_txt_nombre,
                    asesor_cedula   = k.kli_txt_cedula,
                    asesor_cargo    = k.kli_txt_cargo,
                    asesor_correo   = ISNULL(k.kli_txt_correo, 'nomail@kliente.com'),
                    fecha           = CONVERT(VARCHAR, A.Fecha, 112),
                    hora            = CONVERT(VARCHAR, A.Hora, 108),
                    local_nombre    = k.kli_txt_tienda
                FROM AsistenciasUnicas A
                JOIN BDDNOMINAMABEL19..Tbl_DatosPersonales P 
                    ON A.USERID COLLATE SQL_Latin1_General_CP1_CI_AS = P.strDPe_numtarjeta COLLATE SQL_Latin1_General_CP1_CI_AS
                INNER JOIN plataforma_web.dbo.tmp_kliente AS k 
                    ON k.kli_txt_cedula COLLATE SQL_Latin1_General_CP1_CI_AS = P.strDPe_Cedula 
                    AND k.kli_sts_estado=1
                WHERE
                    k.kli_txt_cargo IN ('ASESOR DE VENTAS', 'ASESOR VARIOS')
                    AND k.id_empresa = 1
                    AND k.id_tienda IS NOT NULL
                ORDER BY
                    k.id_tienda,
                    k.kli_txt_cedula,
                    A.Fecha,
                    A.Hora";

            try
            {
                using var connection = new SqlConnection(_config.database.connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(query, connection);
                command.CommandTimeout = _config.database.commandTimeout;

                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    var correo = reader["asesor_correo"].ToString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(correo))
                    {
                        correo = "nomail@kliente.com";
                    }

                    asistencias.Add(new AsistenciaData
                    {
                        asesorNombre = reader["asesor_nombre"].ToString() ?? string.Empty,
                        asesorCedula = reader["asesor_cedula"].ToString() ?? string.Empty,
                        asesorCargo = reader["asesor_cargo"].ToString() ?? string.Empty,
                        asesorCorreo = correo,
                        fecha = reader["fecha"].ToString() ?? string.Empty,
                        hora = reader["hora"].ToString() ?? string.Empty,
                        localNombre = reader["local_nombre"].ToString() ?? string.Empty,
                        // Los campos de API se completan automáticamente
                        clienteId = _config.clienteId,
                        usuarioApi = _config.usuarioApi,
                        claveApi = _config.claveApi
                    });
                }

                _loggingService.Information("Consulta de asistencias ejecutada: {Count} registros obtenidos", asistencias.Count);
                return asistencias.ToArray();
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error al consultar asistencias desde BD: {Message}", ex.Message);
                return Array.Empty<AsistenciaData>();
            }
        }
    }
}