namespace TonyIdentity.Business.Models;

public class OperationResult
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}
