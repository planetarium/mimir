using Microsoft.AspNetCore.Mvc;
using Mimir.Models.Metadata;
using Mimir.Repositories;

namespace Mimir.Controllers;

[ApiController]
[Route("{network}")]
public class MetadataController : ControllerBase
{
    [HttpGet("metadata")]
    public async Task<MetadataResponse> GetMetadata(
        string network,
        string pollerType,
        MetadataRepository metadataRepository
    )
    {
        return new MetadataResponse(
            pollerType,
            await metadataRepository.GetLatestBlockIndex(network, pollerType)
        );
    }
}
