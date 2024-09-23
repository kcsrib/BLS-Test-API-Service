// Tests/CpiServiceTests.cs
using CpiWebService.Services;
using CpiWebService.Models;
using Moq;
using Xunit;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CpiServiceTests
{
    private readonly CachedCpiService _cachedCpiService;
    private readonly Mock<BlsApiService> _mockBlsApiService;
    private readonly IMemoryCache _memoryCache;
    private readonly string _seriesId = "LAUCN040010000000005";

    public CpiServiceTests()
    {
        _mockBlsApiService = new Mock<BlsApiService>(null);
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _cachedCpiService = new CachedCpiService(_mockBlsApiService.Object, _memoryCache);
    }

    [Fact]
    public async Task Should_ReturnFilteredData_FromCachedData()
    {
        // Arrange
        var mockCpiDataList = new List<CpiData>
        {
            new CpiData { Year = 2020, Month = (int)Month.May, CPI = 250, Notes = "Test Note May" },
            new CpiData { Year = 2020, Month = (int)Month.June, CPI = 260, Notes = "Test Note June" }
        };

        _mockBlsApiService.Setup(service => service.GetAllCpiData(_seriesId))
                          .ReturnsAsync(mockCpiDataList);

        // First call to cache the data
        var result = await _cachedCpiService.GetCpiData(_seriesId ,2020, Month.May);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(250, result.CPI);
        Assert.Equal("Test Note May", result.Notes);

        // Verify that the BLS API service was called once
        _mockBlsApiService.Verify(service => service.GetAllCpiData(_seriesId), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnNull_ForNonExistingYearAndMonth()
    {
        // Arrange
        var mockCpiDataList = new List<CpiData>
        {
            new CpiData { Year = 2020, Month = (int)Month.May, CPI = 250, Notes = "Test Note May" },
            new CpiData { Year = 2020, Month = (int)Month.June, CPI = 260, Notes = "Test Note June" }
        };

        _mockBlsApiService.Setup(service => service.GetAllCpiData(_seriesId))
                          .ReturnsAsync(mockCpiDataList);

        // Try to get data for a year/month combination that doesn't exist in the mock data
        var result = await _cachedCpiService.GetCpiData(_seriesId, 2019, Month.January);

        // Assert
        Assert.Null(result);

        // Verify that the BLS API service was called once
        _mockBlsApiService.Verify(service => service.GetAllCpiData(_seriesId), Times.Once);
    }

    [Fact]
    public async Task Should_CacheData_AfterFirstCall()
    {
        // Arrange
        var mockCpiDataList = new List<CpiData>
        {
            new CpiData { Year = 2020, Month = (int)Month.May, CPI = 250, Notes = "Test Note May" }
        };

        _mockBlsApiService.Setup(service => service.GetAllCpiData(_seriesId))
                          .ReturnsAsync(mockCpiDataList);

        // First call to cache the data
        var firstCall = await _cachedCpiService.GetCpiData(_seriesId, 2020, Month.May);

        // Second call should fetch data from the cache
        var secondCall = await _cachedCpiService.GetCpiData(_seriesId, 2020, Month.May);

        // Assert
        Assert.Equal(firstCall, secondCall);

        // Verify that the BLS API service was only called once due to caching
        _mockBlsApiService.Verify(service => service.GetAllCpiData(_seriesId), Times.Once);
    }
}
