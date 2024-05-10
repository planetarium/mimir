using Libplanet.Crypto;
using Microsoft.AspNetCore.Mvc;
using Mimir.Models.Agent;
using Mimir.Repositories;
using Mimir.Services;
using Mimir.Util;

namespace Mimir.Controllers;

[ApiController]
[Route("{network}/sheets")]
public class TableSheetsController(TableSheetsRepository tableSheetsRepository) : ControllerBase
{
    [HttpGet("names")]
    public async Task<string[]> GetSheetNames(
        string network
    )
    {
        var sheetNames = tableSheetsRepository.GetSheetNames(network);

        return sheetNames;
    }

    [HttpGet("{sheetName}")]
    public async Task<ContentResult> GetSheet(
        string network,
        string sheetName
    )
    {
        var sheet = tableSheetsRepository.GetSheet(network, sheetName);

        return Content(sheet, "application/json");;
    }
}
