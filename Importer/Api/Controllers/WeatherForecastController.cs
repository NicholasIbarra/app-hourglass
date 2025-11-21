using Application;
using Application.Contracts.Imports;
using Application.Imports;
using Domain.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Emit;
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

            var json = JsonSerializer.Serialize(response);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            return response;
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import(IFormFile importDto)
        {
            if (importDto == null || importDto.Length == 0)
                return BadRequest("File is required.");


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
            var csv = type switch
            {
                ImportType.EpicCostCenter => CsvTemplateGenerator.GenerateTemplateCsv<ImportOfficeMappingDto>(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), "Unsupported import type")
            };

            // to json
            return File(new UTF8Encoding(true).GetBytes(csv),
                "text/csv",
                "office-mapping-template.csv");
        }
    }

    public static class CsvTemplateGenerator
    {
        public static string GenerateTemplateCsv<T>()
        {
            var props = typeof(T)
                .GetProperties()
                .Select(p => p.Name);

            return string.Join(",", props);
        }
    }

}
