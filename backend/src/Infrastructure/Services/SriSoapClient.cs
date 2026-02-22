using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SaaS.Application.DTOs.Sri;
using SaaS.Application.Interfaces;

namespace SaaS.Infrastructure.Services;

/// <summary>
/// Implementation of SRI web service client using SOAP
/// </summary>
public class SriSoapClient : ISriWebServiceClient
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SriSoapClient> _logger;
    private readonly string _receptionEndpoint;
    private readonly string _authorizationEndpoint;

    public SriSoapClient(
        IConfiguration configuration,
        ILogger<SriSoapClient> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Get SRI endpoints from configuration (test environment by default)
        _receptionEndpoint = _configuration["Sri:ReceptionEndpoint"]
            ?? "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline";
        _authorizationEndpoint = _configuration["Sri:AuthorizationEndpoint"]
            ?? "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline";
    }

    public async Task<SriSubmissionResponse> SubmitDocumentAsync(
        string xmlContent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Submitting document to SRI...");

            // Convert XML to bytes
            var xmlBytes = Encoding.UTF8.GetBytes(xmlContent);
            _logger.LogDebug("Submitting XML to SRI (length: {Length} bytes)", xmlBytes.Length);

            // Create SOAP binding
            var binding = CreateSoapBinding();

            // Create endpoint address
            var endpoint = new EndpointAddress(_receptionEndpoint);

            // Create channel factory
            var factory = new ChannelFactory<IRecepcionComprobantesOfflineChannel>(binding, endpoint);

            // Create channel
            var channel = factory.CreateChannel();

            try
            {
                // Create the request with proper namespace and structure
                var request = new ValidarComprobanteRequest { xml = xmlBytes };

                // Submit the document
                var response = await channel.ValidarComprobanteAsync(request);

                // Parse the response
                var result = ParseSubmissionResponse(response);

                _logger.LogInformation(
                    "Document submission completed. Success: {Success}, Message: {Message}",
                    result.IsSuccess,
                    result.Message);

                return result;
            }
            finally
            {
                // Close the channel
                if (channel is ICommunicationObject commObj)
                {
                    try
                    {
                        if (commObj.State == CommunicationState.Faulted)
                            commObj.Abort();
                        else
                            commObj.Close();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error closing SOAP channel");
                        commObj.Abort();
                    }
                }

                factory.Close();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting document to SRI");
            return new SriSubmissionResponse
            {
                IsSuccess = false,
                Message = "Error comunicándose con el SRI",
                Errors = new List<SriError>
                {
                    new SriError
                    {
                        Code = "CONNECTION_ERROR",
                        Message = ex.Message
                    }
                }
            };
        }
    }

    public async Task<SriAuthorizationResponse> CheckAuthorizationAsync(
        string accessKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking authorization for access key: {AccessKey}", accessKey);

            // Create SOAP binding
            var binding = CreateSoapBinding();

            // Create endpoint address
            var endpoint = new EndpointAddress(_authorizationEndpoint);

            // Create channel factory
            var factory = new ChannelFactory<IAutorizacionComprobantesOfflineChannel>(binding, endpoint);

            // Create channel
            var channel = factory.CreateChannel();

            try
            {
                // Prepare the authorization check request
                var request = Message.CreateMessage(
                    binding.MessageVersion,
                    "autorizacionComprobante",
                    new StringBodyWriter(accessKey));

                // Check authorization
                var response = await channel.AutorizacionComprobanteAsync(request);

                // Parse the response
                var result = ParseAuthorizationResponse(response);

                _logger.LogInformation(
                    "Authorization check completed. Status: {Status}, IsAuthorized: {IsAuthorized}",
                    result.Status,
                    result.IsAuthorized);

                return result;
            }
            finally
            {
                // Close the channel
                if (channel is ICommunicationObject commObj)
                {
                    try
                    {
                        if (commObj.State == CommunicationState.Faulted)
                            commObj.Abort();
                        else
                            commObj.Close();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error closing SOAP channel");
                        commObj.Abort();
                    }
                }

                factory.Close();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authorization from SRI");
            return new SriAuthorizationResponse
            {
                IsAuthorized = false,
                Status = "ERROR",
                Errors = new List<SriError>
                {
                    new SriError
                    {
                        Code = "CONNECTION_ERROR",
                        Message = ex.Message
                    }
                }
            };
        }
    }

    private BasicHttpBinding CreateSoapBinding()
    {
        var binding = new BasicHttpBinding
        {
            MaxReceivedMessageSize = 10485760, // 10 MB
            SendTimeout = TimeSpan.FromMinutes(2),
            ReceiveTimeout = TimeSpan.FromMinutes(2),
            Security = new BasicHttpSecurity
            {
                Mode = BasicHttpSecurityMode.Transport
            }
        };

        return binding;
    }

    private SriSubmissionResponse ParseSubmissionResponse(Message response)
    {
        try
        {
            // Read raw XML body from the SOAP message
            var reader = response.GetReaderAtBodyContents();
            var rawXml = reader.ReadOuterXml();
            _logger.LogDebug("SRI submission response body: {Xml}", rawXml);

            var doc = XDocument.Parse(rawXml);

            // The response body is: <validarComprobanteResponse xmlns="...">
            //   <RespuestaRecepcionComprobante>
            //     <estado>RECIBIDA|DEVUELTA</estado>
            //     <comprobantes>
            //       <comprobante>
            //         <claveAcceso>...</claveAcceso>
            //         <mensajes><mensaje><identificador/><mensaje/><informacionAdicional/><tipo/></mensaje></mensajes>
            //       </comprobante>
            //     </comprobantes>
            //   </RespuestaRecepcionComprobante>
            // </validarComprobanteResponse>

            // Elements below the wrapper are unqualified (elementFormDefault="unqualified")
            var respuesta = doc.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "RespuestaRecepcionComprobante");

            if (respuesta == null)
            {
                _logger.LogWarning("RespuestaRecepcionComprobante element not found in: {Xml}", rawXml);
                return new SriSubmissionResponse
                {
                    IsSuccess = false,
                    Message = "Respuesta inválida del SRI",
                    Errors = new List<SriError>
                    {
                        new SriError { Code = "INVALID_RESPONSE", Message = "No se pudo parsear la respuesta del SRI" }
                    }
                };
            }

            var estado = respuesta.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "estado")?.Value ?? "ERROR";

            var isSuccess = estado.Equals("RECIBIDA", StringComparison.OrdinalIgnoreCase);

            // Extract validation messages from DEVUELTA responses
            var errors = new List<SriError>();
            foreach (var mensaje in doc.Descendants().Where(e => e.Name.LocalName == "mensaje" && e.HasElements))
            {
                var identificador = mensaje.Descendants().FirstOrDefault(e => e.Name.LocalName == "identificador")?.Value;
                var mensajeText = mensaje.Descendants().FirstOrDefault(e => e.Name.LocalName == "mensaje")?.Value;
                var infoAdicional = mensaje.Descendants().FirstOrDefault(e => e.Name.LocalName == "informacionAdicional")?.Value;
                var tipo = mensaje.Descendants().FirstOrDefault(e => e.Name.LocalName == "tipo")?.Value;

                errors.Add(new SriError
                {
                    Code = identificador ?? tipo ?? "ERROR",
                    Message = string.IsNullOrEmpty(infoAdicional) ? mensajeText ?? "" : $"{mensajeText}: {infoAdicional}"
                });

                _logger.LogWarning("SRI validation message [{Tipo}] {Id}: {Msg} - {Info}",
                    tipo, identificador, mensajeText, infoAdicional);
            }

            _logger.LogInformation("SRI estado: {Estado}, messages: {Count}", estado, errors.Count);

            return new SriSubmissionResponse
            {
                IsSuccess = isSuccess,
                Message = estado,
                Errors = errors
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing SRI submission response");
            return new SriSubmissionResponse
            {
                IsSuccess = false,
                Message = "Error al procesar la respuesta del SRI",
                Errors = new List<SriError>
                {
                    new SriError { Code = "PARSE_ERROR", Message = ex.Message }
                }
            };
        }
    }

    private SriAuthorizationResponse ParseAuthorizationResponse(Message response)
    {
        try
        {
            // Read the response XML
            var reader = response.GetReaderAtBodyContents();
            reader.MoveToContent();
            var xmlContent = reader.ReadOuterXml();

            _logger.LogDebug("SRI authorization response XML: {XmlContent}", xmlContent);

            var doc = XDocument.Parse(xmlContent);

            // Parse the response based on SRI's XML structure
            var autorizacion = doc.Descendants("autorizacion").FirstOrDefault();
            if (autorizacion == null)
            {
                // Document might still be processing
                var estado = doc.Descendants("estado")?.FirstOrDefault()?.Value;
                if (estado == "EN PROCESAMIENTO")
                {
                    return new SriAuthorizationResponse
                    {
                        IsAuthorized = false,
                        Status = "EN PROCESAMIENTO"
                    };
                }

                return new SriAuthorizationResponse
                {
                    IsAuthorized = false,
                    Status = "ERROR",
                    Errors = new List<SriError>
                    {
                        new SriError { Code = "INVALID_RESPONSE", Message = "No se encontró información de autorización" }
                    }
                };
            }

            var estadoAutorizacion = autorizacion.Element("estado")?.Value ?? "NO AUTORIZADO";
            var isAuthorized = estadoAutorizacion.Equals("AUTORIZADO", StringComparison.OrdinalIgnoreCase);

            var authResponse = new SriAuthorizationResponse
            {
                IsAuthorized = isAuthorized,
                Status = estadoAutorizacion,
                AuthorizationNumber = autorizacion.Element("numeroAutorizacion")?.Value,
                AuthorizedXml = autorizacion.Element("comprobante")?.Value
            };

            // Parse authorization date
            var fechaStr = autorizacion.Element("fechaAutorizacion")?.Value;
            if (!string.IsNullOrEmpty(fechaStr) && DateTime.TryParse(fechaStr, out var fecha))
            {
                authResponse.AuthorizationDate = fecha;
            }

            // Parse errors if not authorized
            if (!isAuthorized)
            {
                var mensajes = autorizacion.Descendants("mensaje");
                foreach (var mensaje in mensajes)
                {
                    authResponse.Errors.Add(new SriError
                    {
                        Code = mensaje.Element("identificador")?.Value ?? "UNKNOWN",
                        Message = mensaje.Element("mensaje")?.Value ?? "Error desconocido",
                        AdditionalInfo = mensaje.Element("informacionAdicional")?.Value
                    });
                }
            }

            return authResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing SRI authorization response");
            return new SriAuthorizationResponse
            {
                IsAuthorized = false,
                Status = "ERROR",
                Errors = new List<SriError>
                {
                    new SriError { Code = "PARSE_ERROR", Message = ex.Message }
                }
            };
        }
    }

    // Channel interfaces for SRI SOAP services
    [ServiceContract(Namespace = "http://ec.gob.sri.ws.recepcion")]
    private interface IRecepcionComprobantesOfflineChannel : IClientChannel
    {
        [OperationContract(Action = "")]
        Task<Message> ValidarComprobanteAsync(ValidarComprobanteRequest request);
    }

    [ServiceContract]
    private interface IAutorizacionComprobantesOfflineChannel : IClientChannel
    {
        [OperationContract(Action = "autorizacionComprobante")]
        Task<Message> AutorizacionComprobanteAsync(Message request);
    }

    // Message contracts for SRI SOAP requests (document/literal with unqualified elements)
    [MessageContract(WrapperName = "validarComprobante", WrapperNamespace = "http://ec.gob.sri.ws.recepcion", IsWrapped = true)]
    private class ValidarComprobanteRequest
    {
        [MessageBodyMember(Namespace = "")]
        public byte[]? xml { get; set; }
    }

    // (Response parsed as raw Message - see ParseSubmissionResponse)

    // Simple body writer for authorization check
    private class StringBodyWriter : BodyWriter
    {
        private readonly string _content;

        public StringBodyWriter(string content) : base(true)
        {
            _content = content;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement("claveAccesoComprobante");
            writer.WriteString(_content);
            writer.WriteEndElement();
        }
    }
}
