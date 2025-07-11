using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391.Api.Models;
using SWP391.Application.DTOs;
using SWP391.Application.Services;
using SWP391.Application.Services.Interfaces;

namespace SWP391.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PillIntakeCycleController : ControllerBase
    {
        private readonly IContraceptivePillReminder _pillReminder;
        private readonly IPillIntakeCycleService _pillIntakeCycleService;
        public PillIntakeCycleController(IContraceptivePillReminder pillReminder, IPillIntakeCycleService pillIntakeCycleService)
        {
            _pillReminder = pillReminder;
            _pillIntakeCycleService = pillIntakeCycleService;
        }

        [HttpPost("run-daily-reminder")]
        //[Authorize] // Bật nếu chỉ cho user/authenticated gọi
        public async Task<IActionResult> RunDailyPillReminder()
        {
            try
            {
                await _pillReminder.RunDailyPillReminderAsync();

                return Ok(ApiResponse<object>.Success(null, "Daily pill reminder run successful."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.Error(401, ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "Internal server error."));
            }
        }

        [HttpPost("extend-pill-pack")]
        public async Task<IActionResult> ExtendPillPack([FromQuery] int packId)
        {
            try
            {
                await _pillIntakeCycleService.ExtendPackAsync(packId);

                return Ok(ApiResponse<object>.Success(null, "Extend pack successfully."));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.Error(409, ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "Internal server error."));
            }            
        }



        [HttpPost("load-pill-pack-calendar")]
        public async Task<IActionResult> LoadPillCalendar([FromBody] MonthAndYear dto)
        {
            try
            {
                //var userId = await _currentUserService.GetCurrentUserIdAsync();

                var calendar = await _pillIntakeCycleService.LoadPillCalendar(2, dto.Month, dto.Year);

                return Ok(ApiResponse<List<DateAndType>>.Success(calendar, "Load calendar successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(400, ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.Error(401, ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "Internal server error."));
            }
        }

        [HttpPost("add-pill-pack")]
        public async Task<IActionResult> AddPillPack([FromBody] PillPackDto dto)
        {
            try
            {
                //var userId = await _currentUserService.GetCurrentUserIdAsync();
                await _pillIntakeCycleService.AddPackAsync(2, dto.StartDate, dto.PackSize);

                return Ok(ApiResponse<object>.Success(null, "Add pack successfully."));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.Error(409, ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.Error(500, "Internal server error."));
            }
        }

    }
}
