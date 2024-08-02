using Microsoft.AspNetCore.Mvc;
using Mimir.Models.Metadata;
using Mimir.Repositories;

namespace Mimir.Controllers;

[ApiController]
[Route("metadata")]
public class MetadataController : ControllerBase
{
    [HttpGet]
    public MetadataResponse GetMetadata(
        string pollerType,
        string collectionName,
        MetadataRepository metadataRepository
    )
    {
        return new MetadataResponse(
            pollerType,
            metadataRepository.GetLatestBlockIndex(pollerType, collectionName)
        );
    }
}
