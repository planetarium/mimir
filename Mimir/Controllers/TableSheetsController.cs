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
    [Produces("application/json", "text/csv")]
    public async Task<IActionResult> GetSheet(string network, string sheetName)
    {
        var acceptHeader = Request.Headers["Accept"].ToString();
        SheetFormat? sheetFormatNullable = acceptHeader switch
        {
            "text/csv" => SheetFormat.Csv,
            "application/json" => SheetFormat.Json,
            _ => null,
        };

        if (sheetFormatNullable is not { } sheetFormat)
        {
            return StatusCode(StatusCodes.Status406NotAcceptable);
        }

        try
        {
            var sheet = await tableSheetsRepository.GetSheet(network, sheetName, sheetFormat);
            return Content(sheet, sheetFormat.ToMimeType());
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
