using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using IntegracionKoach360.Interfaces;
using IntegracionKoach360.Models;

namespace IntegracionKoach360.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILoggingService _loggingService;
        private readonly ConfiguracionApp _config;
        private string? _currentToken;
        private DateTime _lastTokenRenewal = DateTime.MinValue;

        public ApiService(HttpClient httpClient, ILoggingService loggingService, ConfiguracionApp config)
        {
            _httpClient = httpClient;
            _loggingService = loggingService;
            _config = config;
        }

        public async Task<string?> GetTokenAsync()
        {
            try
            {
                var authData = new
                {
                    usuario = _config.usuario,
                    password = _config.password
                };

                var json = JsonSerializer.Serialize(authData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _loggingService.Information("Autenticando en: {BaseUrl}/api/Auth/login", _config.baseUrl);

                var response = await _httpClient.PostAsync($"{_config.baseUrl}/api/Auth/login", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _loggingService.Error("Error al obtener token: {StatusCode}", response.StatusCode);
                    _loggingService.Error("Respuesta: {ResponseBody}", responseBody);
                    return null;
                }

                var result = JsonSerializer.Deserialize<AuthResponse>(responseBody);
                return result?.token ?? string.Empty;
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error al obtener token: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<bool> RenewTokenIfNeededAsync()
        {
            // Renovar token si no existe o si han pasado más de 50 minutos (el token dura 1 hora)
            if (string.IsNullOrEmpty(_currentToken) || 
                DateTime.Now.Subtract(_lastTokenRenewal).TotalMinutes > 50)
            {
                _currentToken = await GetTokenAsync();
                if (!string.IsNullOrEmpty(_currentToken))
                {
                    _lastTokenRenewal = DateTime.Now;
                    _loggingService.Information("Token obtenido/renovado exitosamente");
                    return true;
                }
                return false;
            }
            return true;
        }

        public async Task<bool> SendVentasAsync(VentaData[] ventas)
        {
            try
            {
                if (!await RenewTokenIfNeededAsync())
                {
                    _loggingService.Error("No se pudo obtener el token de autenticación");
                    return false;
                }

                var json = JsonSerializer.Serialize(ventas, new JsonSerializerOptions { WriteIndented = false });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var request = new HttpRequestMessage(HttpMethod.Post, $"{_config.baseUrl}/api/Ventas/cargaVentasV1");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _currentToken);
                request.Content = content;

                _loggingService.Information("Enviando {Count} venta(s) a: {BaseUrl}/api/Ventas/cargaVentasV1", 
                    ventas.Length, _config.baseUrl);

                var response = await _httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _loggingService.Error("Error al enviar ventas. Status: {StatusCode}", response.StatusCode);
                    _loggingService.Error("Error: {ResponseBody}", responseBody);
                    return false;
                }

                _loggingService.Information("Ventas enviadas exitosamente");

                // Deserializar y mostrar respuesta
                try
                {
                    var result = JsonSerializer.Deserialize<VentasResponse>(responseBody);
                    if (result != null)
                    {
                        _loggingService.Information("Respuesta: {Response}", 
                            JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
                    }
                }
                catch
                {
                    _loggingService.Information("Respuesta recibida (no deserializable): {ResponseBody}", responseBody);
                }

                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error al enviar ventas: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> SendAsistenciasAsync(AsistenciaData[] asistencias)
        {
            try
            {
                if (!await RenewTokenIfNeededAsync())
                {
                    _loggingService.Error("No se pudo obtener el token de autenticación");
                    return false;
                }

                var json = JsonSerializer.Serialize(asistencias, new JsonSerializerOptions { WriteIndented = false });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var request = new HttpRequestMessage(HttpMethod.Post, $"{_config.baseUrl}/api/AsistenciaReal/cargaAsistenciaRealV1");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _currentToken);
                request.Content = content;

                _loggingService.Information("Enviando {Count} asistencia(s) a: {BaseUrl}/api/AsistenciaReal/cargaAsistenciaRealV1", 
                    asistencias.Length, _config.baseUrl);

                var response = await _httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _loggingService.Error("Error al enviar asistencias. Status: {StatusCode}", response.StatusCode);
                    _loggingService.Error("Error: {ResponseBody}", responseBody);
                    return false;
                }

                _loggingService.Information("Asistencias enviadas exitosamente");

                // Deserializar y mostrar respuesta
                try
                {
                    var result = JsonSerializer.Deserialize<AsistenciaResponse>(responseBody);
                    if (result != null)
                    {
                        _loggingService.Information("Respuesta: {Response}", 
                            JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
                    }
                }
                catch
                {
                    _loggingService.Information("Respuesta recibida (no deserializable): {ResponseBody}", responseBody);
                }

                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error al enviar asistencias: {Message}", ex.Message);
                return false;
            }
        }
    }
}
