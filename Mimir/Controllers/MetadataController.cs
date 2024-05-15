using Microsoft.AspNetCore.Mvc;
using Mimir.Models.Metadata;
using Mimir.Repositories;

namespace Mimir.Controllers;

[ApiController]
[Route("{network}")]
public class MetadataController : ControllerBase
{
    [HttpGet("tip")]
    public async Task<GetTipResponse> GetTip(
        string network,
        MetadataRepository metadataRepository
    )
    {
        return new GetTipResponse(await metadataRepository.GetLatestBlockIndex(network));
    }
}
