using Microsoft.AspNetCore.Mvc;
using Mimir.Models.Avatar;
using Mimir.Repositories;

namespace Mimir.Controllers;

[ApiController]
[Route("{network}/avatars")]
public class AvatarController(AvatarRepository avatarRepository) : ControllerBase
{
    [HttpGet("{avatarAddress}/inventory")]
    public Inventory? GetInventory(string network, string avatarAddress)
    {
        var inventory = avatarRepository.GetInventory(network, avatarAddress);
        if (inventory is null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return null;
        }

        return inventory;
    }
}
