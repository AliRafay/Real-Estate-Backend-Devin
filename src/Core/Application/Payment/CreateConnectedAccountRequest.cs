using Demo.WebApi.Domain.Common.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Demo.WebApi.Application.Payment;
public class CreateConnectedAccountRequest
{
    public string? Email { get; set; }
    public string? BankName { get; set; }
    public string? AccountHolderName { get; set; }
    public string? AccountNumber { get; set; }
    public bool IsOwnAccount { get; set; }
    [EnumDataType(typeof(Relationship))]
    public Relationship? Relationship { get; set; }
    public string? OtherRelationship { get; set; }
    public string? IdNumber { get; set; }
    public string? BeneficiaryId { get; set; }
    public string? RoutingNumber { get; set; }
    [DataType(DataType.Upload)]
    public IFormFile? DocumentFront { get; set; } = default!;
    [DataType(DataType.Upload)]
    public IFormFile? DocumentBack { get; set; } = default!;
    public DateTime? Dob { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? Currency { get; set; }

}
