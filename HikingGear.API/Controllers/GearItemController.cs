using HikingGear.BLL.DTOs;
using HikingGear.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HikingGear.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GearItemController : ControllerBase
    {
        private readonly IGearItemService _gearItemService;

        public GearItemController(IGearItemService gearItemService)
        {
            _gearItemService = gearItemService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException("Недійсний токен.");
            return userId;
        }

        [HttpGet("trip/{tripId}")]
        public async Task<IActionResult> GetGearForTrip(int tripId)
        {
            try
            {
                var result = await _gearItemService.GetGearForTripAsync(tripId, GetUserId());
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomGearItem([FromBody] GearItemCreateDto dto)
        {
            try
            {
                var result = await _gearItemService.AddCustomGearItemAsync(GetUserId(), dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{itemId}")]
        public async Task<IActionResult> UpdateGearItem(int itemId, [FromBody] GearItemUpdateDto dto)
        {
            try
            {
                await _gearItemService.UpdateGearItemAsync(GetUserId(), itemId, dto);
                return Ok(new { message = "Річ успішно оновлено." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{itemId}")]
        public async Task<IActionResult> DeleteGearItem(int itemId)
        {
            try
            {
                await _gearItemService.DeleteGearItemAsync(GetUserId(), itemId);
                return Ok(new { message = "Річ успішно видалено." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{itemId}/pack")]
        public async Task<IActionResult> UpdatePackedStatus(int itemId, [FromBody] GearItemPackStatusDto dto)
        {
            try
            {
                await _gearItemService.UpdatePackedStatusAsync(GetUserId(), itemId, dto.IsPacked);
                return Ok(new { message = "Статус пакування оновлено." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("trip/{tripId}/progress")]
        public async Task<IActionResult> GetPackingProgress(int tripId)
        {
            try
            {
                var progress = await _gearItemService.GetPackingProgressAsync(GetUserId(), tripId);
                return Ok(progress);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
