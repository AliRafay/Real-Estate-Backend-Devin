using Microsoft.AspNetCore.Http;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.WebApi.Application.Public.Document;
public class MultipleDocumentRequest
{
    public List<IFormFile> Documents { get; set; }
    public string Path { get; set; }
}
