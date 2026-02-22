using SaaS.Application.DTOs.Sri;

namespace SaaS.Application.Interfaces;

/// <summary>
/// Service for interacting with SRI web services (SOAP)
/// </summary>
public interface ISriWebServiceClient
{
    /// <summary>
    /// Submits an electronic document to SRI for validation
    /// </summary>
    /// <param name="xmlContent">The signed XML content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SRI submission response with validation result</returns>
    Task<SriSubmissionResponse> SubmitDocumentAsync(string xmlContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks the authorization status of a previously submitted document
    /// </summary>
    /// <param name="accessKey">The 49-digit access key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SRI authorization response with authorization status</returns>
    Task<SriAuthorizationResponse> CheckAuthorizationAsync(string accessKey, CancellationToken cancellationToken = default);
}
