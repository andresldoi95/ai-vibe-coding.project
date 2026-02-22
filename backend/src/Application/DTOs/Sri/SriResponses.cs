namespace SaaS.Application.DTOs.Sri;

/// <summary>
/// Response from SRI after submitting a document for validation
/// </summary>
public class SriSubmissionResponse
{
    /// <summary>
    /// Indicates if the submission was successful (document was received)
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// SRI confirmation message (e.g., "RECIBIDA")
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// List of errors returned by SRI during submission
    /// </summary>
    public List<SriError> Errors { get; set; } = new();

    /// <summary>
    /// Additional information from SRI
    /// </summary>
    public Dictionary<string, string> AdditionalInfo { get; set; } = new();
}

/// <summary>
/// Response from SRI when checking authorization status
/// </summary>
public class SriAuthorizationResponse
{
    /// <summary>
    /// Indicates if the document is authorized
    /// </summary>
    public bool IsAuthorized { get; set; }

    /// <summary>
    /// Authorization status (e.g., "AUTORIZADO", "NO AUTORIZADO", "EN PROCESAMIENTO")
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Authorization number from SRI (10 digits)
    /// </summary>
    public string? AuthorizationNumber { get; set; }

    /// <summary>
    /// Authorization date and time
    /// </summary>
    public DateTime? AuthorizationDate { get; set; }

    /// <summary>
    /// List of errors if authorization failed
    /// </summary>
    public List<SriError> Errors { get; set; } = new();

    /// <summary>
    /// Authorized XML content (includes SRI signature)
    /// </summary>
    public string? AuthorizedXml { get; set; }
}

/// <summary>
/// Represents an error returned by SRI
/// </summary>
public class SriError
{
    /// <summary>
    /// Error code from SRI
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Error message from SRI
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional error information
    /// </summary>
    public string? AdditionalInfo { get; set; }
}
