using GamblingApp.Models.Entities;
using GamblingApp.Models.Enums;
namespace GamblingApp.Models.DTOs;

/// <summary>
/// Read only version of a transaction.  
/// Returned anywhere transaction history is displayed (bets, payouts, deposits).
/// </summary>

public class TransactionReadDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? BetId { get; set; }
}