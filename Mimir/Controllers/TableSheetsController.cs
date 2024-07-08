using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Mimir.Enums;
using Mimir.Repositories;

namespace Mimir.Controllers;

[ApiController]
[Route("{network}/sheets")]
public class TableSheetsController(TableSheetsRepository tableSheetsRepository) : ControllerBase
{
    [HttpGet("names")]
    public string[] GetSheetNames(string network) =>
        tableSheetsRepository.GetSheetNames(network);

    [HttpGet("{sheetName}")]
    [Produces(MediaTypeNames.Application.Json, MediaTypeNames.Text.Csv)]
    public async Task<IActionResult> GetSheet(string network, string sheetName)
    {
        var acceptHeader = Request.Headers["Accept"].ToString();

        SheetFormat? sheetFormatNullable = acceptHeader switch
        {
            MediaTypeNames.Text.Csv => SheetFormat.Csv,
            MediaTypeNames.Application.Json => SheetFormat.Json,
            _ => null,
        };

        if (sheetFormatNullable is not { } sheetFormat)
        {
            return StatusCode(StatusCodes.Status406NotAcceptable);
        }

        try
        {
            var sheet = await tableSheetsRepository.GetSheetAsync(network, sheetName, sheetFormat);
            return Content(sheet, sheetFormat.ToMimeType());
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
