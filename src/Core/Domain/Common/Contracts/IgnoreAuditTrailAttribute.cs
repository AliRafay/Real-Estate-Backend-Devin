namespace Demo.WebApi.Domain.Common.Contracts;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class IgnoreAuditTrailAttribute : Attribute
{
}
