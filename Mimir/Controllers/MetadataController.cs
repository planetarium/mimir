using Microsoft.AspNetCore.Mvc;
using Mimir.Models.Metadata;
using Mimir.Repositories;

namespace Mimir.Controllers;

[ApiController]
[Route("metadata")]
public class MetadataController : ControllerBase
{
    [HttpGet]
    public async Task<MetadataResponse> GetMetadata(
        string pollerType,
        string collectionName,
        MetadataRepository metadataRepository
    )
    {
        var document = await metadataRepository.GetByCollectionAndTypeAsync(
            pollerType,
            collectionName
        );

        return new MetadataResponse(pollerType, document.LatestBlockIndex);
    }
}
