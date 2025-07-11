using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391.Api.Models;
using SWP391.Api.Services.Interfaces;
using SWP391.Application.Services.Interfaces;
using SWP391.Application.DTOs;
namespace SWP391.Api.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class MenstrualCycleController : ControllerBase
    {
        private readonly IMenstrualCycleService _menstrualCycleService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserService _userService;
        public MenstrualCycleController(
        IMenstrualCycleService menstrualCycleService,
        ICurrentUserService currentUserService,
        IUserService userService)
        {
            _menstrualCycleService = menstrualCycleService;
            _currentUserService = currentUserService;
            _userService = userService;
        }

        [HttpPost("toggle-menstrual-date")]
        //[Authorize]
        public async Task<IActionResult> ToggleMenstrualDate([FromBody] ToggleMenstrualDateDto dto)
        {
            try
            {
                var userId = await _currentUserService.GetCurrentUserIdAsync();

                await _menstrualCycleService.ToggleMenstrualDate(userId, dto.Day, dto.Month, dto.Year);

                return Ok(ApiResponse<object>.Success(null, "Toggle menstrual date successful."));
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

        [HttpPost("load-menstrual-calendar")]
        //[Authorize]
        public async Task<IActionResult> LoadMenstrualCalendar([FromBody] MonthAndYear dto)
        {
            try
            {
                //var userId = await _currentUserService.GetCurrentUserIdAsync();

                var calendar = await _menstrualCycleService.LoadMenstrualCalendar(2, dto.Month, dto.Year);

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

        [HttpPost("get-next-period-date")]
        //[Authorize]
        public async Task<IActionResult> GetNextPeriodDate()
        {
            try
            {
                //var userId = await _currentUserService.GetCurrentUserIdAsync();

                var date = await _menstrualCycleService.GetNextMenstrualDate(2);

                return Ok(ApiResponse<string>.Success(date, "Next period date generated successfully"));
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

        [HttpPost("update-user-menstrual-length-and-cycle-length")]
        public async Task<IActionResult> UpdateUserMenstrualLengthAndCycleLength(MenstrualInfo dto)
        {
            try
            {
                var userId = await _currentUserService.GetCurrentUserIdAsync();
                await _menstrualCycleService.UpdateMenstrualLengthAndCycleLength(userId, dto.MenstrualLength,dto.CycleLength);
                

                return Ok(ApiResponse<object>.Success(null, "Update successfully"));
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
    }
}
