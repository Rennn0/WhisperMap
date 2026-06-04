using System.Text.Json.Serialization;

namespace XcLib.Shared.Payment.FlittImpl.Docs;

public record GetOrderStatusResponseData
{
    [JsonPropertyName("order_id")] public required string OrderId { get; init; }

    [JsonPropertyName("merchant_id")] public long MerchantId { get; init; }

    [JsonPropertyName("amount")] public string? Amount { get; init; }

    [JsonPropertyName("currency")] public Currency Currency { get; init; }

    [JsonPropertyName("order_status")] public FlittOrderStatus OrderStatus { get; init; }

    [JsonPropertyName("response_status")] public ResponseStatus ResponseStatus { get; init; }

    [JsonPropertyName("signature")] public string? Signature { get; init; }

    [JsonPropertyName("tran_type")] public TransactionType? TranType { get; init; }

    [JsonPropertyName("rrn")] public string? Rrn { get; init; }

    [JsonPropertyName("masked_card")] public string? MaskedCard { get; init; }

    [JsonPropertyName("sender_cell_phone")]
    public string? SenderCellPhone { get; init; }

    [JsonPropertyName("sender_account")] public string? SenderAccount { get; init; }

    [JsonPropertyName("card_bin")] public int? CardBin { get; init; }

    [JsonPropertyName("card_type")] public string? CardType { get; init; }

    [JsonPropertyName("approval_code")] public string? ApprovalCode { get; init; }

    [JsonPropertyName("response_code")] public string? ResponseCode { get; init; }

    [JsonPropertyName("response_description")]
    public string? ResponseDescription { get; init; }

    [JsonPropertyName("reversal_amount")] public string? ReversalAmount { get; init; }

    [JsonPropertyName("settlement_amount")]
    public string? SettlementAmount { get; init; }

    [JsonPropertyName("settlement_currency")]
    public string? SettlementCurrency { get; init; }

    [JsonIgnore]
    public Currency? SettlementCurrencyEnum =>
        Enum.TryParse(SettlementCurrency, true, out Currency c)
            ? c
            : null;

    [JsonPropertyName("order_time")] public string? OrderTime { get; init; }

    [JsonPropertyName("settlement_date")] public string? SettlementDate { get; init; }

    [JsonPropertyName("eci")] public string? Eci { get; init; }

    [JsonPropertyName("fee")] public string? Fee { get; init; }

    [JsonPropertyName("payment_system")] public string? PaymentSystem { get; init; }

    [JsonPropertyName("sender_email")] public string? SenderEmail { get; init; }

    [JsonPropertyName("payment_id")] public long? PaymentId { get; init; }

    [JsonPropertyName("actual_amount")] public string? ActualAmount { get; init; }

    [JsonPropertyName("actual_currency")] public string? ActualCurrency { get; init; }

    [JsonIgnore] public Currency? ActualCurrencyEnum => Enum.TryParse(ActualCurrency, true, out Currency c) ? c : null;

    [JsonPropertyName("product_id")] public string? ProductId { get; init; }

    [JsonPropertyName("merchant_data")] public string? MerchantData { get; init; }

    [JsonPropertyName("verification_status")]
    public string? VerificationStatus { get; init; }

    [JsonIgnore]
    public VerificationStatus? VerificationStatusEnum =>
        Enum.TryParse(VerificationStatus, true, out VerificationStatus v) ? v : null;

    [JsonPropertyName("rectoken")] public string? Rectoken { get; init; }

    [JsonPropertyName("rectoken_lifetime")]
    public string? RectokenLifetime { get; init; }

    [JsonPropertyName("fee_oplata")] public string? FeeOplata { get; init; }

    [JsonPropertyName("parent_order_id")] public string? ParentOrderId { get; init; }

    [JsonPropertyName("additional_info")] public string? AdditionalInfoRaw { get; init; }
    [JsonPropertyName("response_signature_string")] public string? SignatureStringRaw { get; init; }
}