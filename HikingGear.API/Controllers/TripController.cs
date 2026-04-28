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
    public class TripController : ControllerBase
    {
        private readonly ITripService _tripService;

        public TripController(ITripService tripService)
        {
            _tripService = tripService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTrip([FromBody] TripCreateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Недійсний токен" });
            }

            try
            {
                var result = await _tripService.CreateTripAsync(userId, dto);
                return CreatedAtAction(nameof(GetTrip), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Помилка створення походу: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrip(int id)
        {
            var trip = await _tripService.GetTripByIdAsync(id);
            if (trip == null) return NotFound();
            return Ok(trip);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyTrips()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Недійсний токен" });
            }

            var trips = await _tripService.GetUserTripsAsync(userId);
            return Ok(trips);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrip(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Недійсний токен" });
            }

            try
            {
                await _tripService.DeleteTripAsync(userId, id);
                return Ok(new { message = "Похід успішно видалено." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Помилка видалення походу: {ex.Message}" });
            }
        }
    }
}
