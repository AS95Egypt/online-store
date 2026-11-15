using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Application.Shipping;

/// <summary>A governorate and its current shipping fee for administrators.</summary>
public sealed record AdminGovernorateDto(int Id, string Name, decimal ShippingFee);

/// <summary>Request to update a governorate's shipping fee.</summary>
public sealed record UpdateGovernorateFeeRequest
{
    [Range(typeof(decimal), "0", "9999999999999999")]
    public required decimal ShippingFee { get; init; }
}

/// <summary>A shipping fee request failed validation.</summary>
public sealed class ShippingFeeValidationException : Exception
{
    public string Code { get; }
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ShippingFeeValidationException(
        string code,
        string message,
        IReadOnlyDictionary<string, string[]> errors)
        : base(message)
    {
        Code = code;
        Errors = errors;
    }
}
