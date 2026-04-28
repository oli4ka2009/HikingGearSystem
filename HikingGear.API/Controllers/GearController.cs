using HikingGear.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HikingGear.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GearController : ControllerBase
    {
        private readonly IGearGenerationService _gearGenerationService;

        public GearController(IGearGenerationService gearGenerationService)
        {
            _gearGenerationService = gearGenerationService;
        }

        [HttpPost("generate/{tripId}")]
        public async Task<IActionResult> GenerateGearList(int tripId)
        {
            try
            {
                var gearList = await _gearGenerationService.GenerateGearListAsync(tripId);

                return Ok(gearList);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Помилка під час генерації: {ex.Message}" });
            }
        }
    }
}
