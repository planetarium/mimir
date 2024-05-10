using Microsoft.AspNetCore.Mvc;
using Mimir.Enums;
using Mimir.Repositories;

namespace Mimir.Controllers;

[ApiController]
[Route("{network}/sheets")]
public class TableSheetsController(TableSheetsRepository tableSheetsRepository) : ControllerBase
{
    [HttpGet("names")]
    public async Task<string[]> GetSheetNames(string network)
    {
        var sheetNames = tableSheetsRepository.GetSheetNames(network);

        return sheetNames;
    }

    [HttpGet("{sheetName}")]
    public async Task<IActionResult> GetSheet(
        string network,
        string sheetName,
        string? format = "csv"
    )
    {
        if (!Enum.TryParse(format, true, out SheetFormat sheetFormat))
        {
            return BadRequest(
                new { message = "Invalid format. Supported formats are 'json' and 'csv'." }
            );
        }

        var sheet = tableSheetsRepository.GetSheet(network, sheetName, sheetFormat);
        string contentType = sheetFormat switch
        {
            SheetFormat.Csv => "text/csv",
            _ => "application/json"
        };

        return Content(sheet, contentType);
    }
}
