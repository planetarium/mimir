using Microsoft.AspNetCore.Mvc;
using NineChroniclesUtilBackend.Models.Avatar;
using NineChroniclesUtilBackend.Repositories;

namespace NineChroniclesUtilBackend.Controllers;

[ApiController]
[Route("avatars")]
public class AvatarController(AvatarRepository avatarRepository) : ControllerBase
{
    [HttpGet("{avatarAddress}/inventory")]
    public Inventory GetInventory(string avatarAddress) =>
        avatarRepository.GetInventory(avatarAddress);
}
