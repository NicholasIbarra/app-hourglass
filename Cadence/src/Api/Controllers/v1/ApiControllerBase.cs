using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Cadence.Api.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1")]
public abstract class ApiControllerBase : ControllerBase
{
}
