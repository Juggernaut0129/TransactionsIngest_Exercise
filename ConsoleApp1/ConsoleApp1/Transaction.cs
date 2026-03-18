using System.ComponentModel.DataAnnotations;

public class Transaction
{
    //Fields
    [Required]
    public int TransactionId { get; set; }
    private string _cardNumber;
    public string CardNumber
    {
        get => _cardNumber;
        set => _cardNumber = value.Length > 4 ? value[^4..] : value;
    }
    public string LocationCode { get; set; }
    public string ProductName { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionTime { get; set; }
    public bool Finalized { get; set; }
    public bool Revoked { get; set; }

    //Constructor
    public Transaction(int transactionId, DateTime transactionTime, string cardNumber = "", string locationCode = "", string productName = "", decimal amount = 0, bool finalized = false, bool revoked = false)
    {
        TransactionId = transactionId;
        CardNumber = cardNumber;
        LocationCode = locationCode;
        ProductName = productName;
        Amount = amount;
        TransactionTime = transactionTime;
        Finalized = finalized;
        Revoked = revoked;
    }
}

public class TransactionUpdate
{
    public int TransactionUpdateId { get; set; }
    public int TransactionId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; }

    public TransactionUpdate(int transactionUpdateId, int transactionId, DateTime timestamp, string message)
    {
        TransactionUpdateId = transactionUpdateId;
        TransactionId = transactionId;
        Timestamp = timestamp;
        Message = message;
    }
}