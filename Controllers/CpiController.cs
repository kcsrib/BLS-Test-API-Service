// Controllers/CpiController.cs
using Microsoft.AspNetCore.Mvc;
using CpiWebService.Services;
using CpiWebService.Models;

namespace CpiWebService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CpiController : ControllerBase
    {
        private readonly CachedCpiService _cachedCpiService;

        public CpiController(CachedCpiService cachedCpiService)
        {
            _cachedCpiService = cachedCpiService;
        }

        [HttpGet("{seriesId}/{year}/{month}")]
        public async Task<IActionResult> GetCpiData(string seriesId, int year, Month month)
        {
            var cpiData = await _cachedCpiService.GetCpiData(seriesId, year, month);
            if (cpiData == null)
                return NotFound("No CPI data available for the given month and year.");

            return Ok(cpiData);
        }
    }
}
