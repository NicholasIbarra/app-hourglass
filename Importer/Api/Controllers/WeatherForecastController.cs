using Application;
using Application.Contracts.Imports;
using Application.Imports;
using Domain.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[Authorize]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IImportWorkflowBase<ImportOfficeMappingDto> _importWorkflow;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger, 
            IImportWorkflowBase<ImportOfficeMappingDto> importWorkflow)
        {
            _logger = logger;
            _importWorkflow = importWorkflow;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            var response = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            var json = System.Text.Json.JsonSerializer.Serialize(response);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            return response;
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import([FromBody] IFormFile importDto)
        {
            var result = await _importWorkflow.Import(importDto);
            if (result.Success)
            {
                return Ok(new { Message = "Import started successfully." });
            }
            else
            {
                return BadRequest(new { Message = "Import failed to start.", Error = result.Error });
            }
        }

        [HttpGet("template")]
        public IActionResult GetImportTemplate(ImportType type)
        {
            var templateStream = type switch
            {
                ImportType.EpicCostCenter => new ImportOfficeMappingDto(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), "Unsupported import type")
            };

            // to json
            var content = JsonSerializer.Serialize(templateStream, new JsonSerializerOptions { WriteIndented = true });
            var byteArray = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(byteArray);

            return File(stream, "text/csv", $"{type}_ImportTemplate.csv");
        }
    }
}
