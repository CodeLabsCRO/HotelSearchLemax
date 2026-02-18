using HotelSearchLemax.Core.DTOs;
using HotelSearchLemax.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSearchLemax.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme},{CookieAuthenticationDefaults.AuthenticationScheme}")]
public class HotelsController : ControllerBase
{
    private readonly IHotelService _hotelService;

    public HotelsController(IHotelService hotelService)
    {
        _hotelService = hotelService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<HotelDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<HotelDto>>> GetAll()
    {
        var hotels = await _hotelService.GetAllHotelsAsync();
        return Ok(hotels);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(HotelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HotelDto>> GetById(int id)
    {
        var hotel = await _hotelService.GetHotelByIdAsync(id);
        if (hotel == null)
            return NotFound();

        return Ok(hotel);
    }

    [HttpPost]
    [ProducesResponseType(typeof(HotelDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HotelDto>> Create([FromBody] CreateHotelDto createDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var hotel = await _hotelService.CreateHotelAsync(createDto);
        return CreatedAtAction(nameof(GetById), new { id = hotel.Id }, hotel);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(HotelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HotelDto>> Update(int id, [FromBody] UpdateHotelDto updateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var hotel = await _hotelService.UpdateHotelAsync(id, updateDto);
        if (hotel == null)
            return NotFound();

        return Ok(hotel);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _hotelService.DeleteHotelAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(PaginatedResult<HotelSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResult<HotelSearchResultDto>>> Search([FromQuery] HotelSearchRequestDto searchRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var results = await _hotelService.SearchHotelsAsync(searchRequest);
        return Ok(results);
    }
}
