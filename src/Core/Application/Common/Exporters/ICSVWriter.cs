
namespace Demo.WebApi.Application.Common.Export;

public interface ICSVWriter : ITransientService
{
    byte[] WriteCSV<T>(List<T> data);
}