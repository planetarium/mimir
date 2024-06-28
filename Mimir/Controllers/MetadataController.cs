using Microsoft.AspNetCore.Mvc;
using Mimir.Models.Metadata;
using Mimir.Repositories;

namespace Mimir.Controllers;

[ApiController]
[Route("{network}")]
public class MetadataController : ControllerBase
{
    [HttpGet("metadata")]
    public MetadataResponse GetMetadata(
        string network,
        string pollerType,
        MetadataRepository metadataRepository
    )
    {
        return new MetadataResponse(
            pollerType,
            metadataRepository.GetLatestBlockIndex(network, pollerType)
        );
    }
}
