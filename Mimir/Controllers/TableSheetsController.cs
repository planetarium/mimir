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

        SheetFormat sheetFormat = acceptHeader.Contains("text/csv")
            ? SheetFormat.Csv
            : SheetFormat.Json;

        try
        {
            var sheet = await tableSheetsRepository.GetSheet(network, sheetName, sheetFormat);

            string contentType = sheetFormat switch
            {
                SheetFormat.Csv => "text/csv",
                _ => "application/json"
            };

            return Content(sheet, contentType);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
