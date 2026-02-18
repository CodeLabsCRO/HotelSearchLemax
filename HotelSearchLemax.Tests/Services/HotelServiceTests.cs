using FluentAssertions;
using HotelSearchLemax.Core.DTOs;
using HotelSearchLemax.Core.Entities;
using HotelSearchLemax.Core.Interfaces;
using HotelSearchLemax.DataAccess;
using HotelSearchLemax.DataAccess.Data;
using HotelSearchLemax.Services.Implementations;
using Microsoft.EntityFrameworkCore;

namespace HotelSearchLemax.Tests.Services;

/// <summary>
/// Unit tests for HotelService covering CRUD operations, search functionality, and edge cases.
/// </summary>
public class HotelServiceTests : IDisposable
{
    private readonly HotelDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly HotelService _sut; // System Under Test

    public HotelServiceTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new HotelDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
        _sut = new HotelService(_unitOfWork);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region CRUD Tests

    [Fact]
    public async Task CreateHotelAsync_ValidInput_ReturnsCreatedHotel()
    {
        // Arrange
        var createDto = new CreateHotelDto
        {
            Name = "Test Hotel",
            Price = 100.00m,
            Latitude = 45.8150,
            Longitude = 15.9819
        };

        // Act
        var result = await _sut.CreateHotelAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be(createDto.Name);
        result.Price.Should().Be(createDto.Price);
        result.Latitude.Should().Be(createDto.Latitude);
        result.Longitude.Should().Be(createDto.Longitude);
    }

    [Fact]
    public async Task GetHotelByIdAsync_ExistingHotel_ReturnsHotel()
    {
        // Arrange
        var hotel = await CreateTestHotel("Test Hotel", 100m, 45.8150, 15.9819);

        // Act
        var result = await _sut.GetHotelByIdAsync(hotel.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(hotel.Id);
        result.Name.Should().Be(hotel.Name);
    }

    [Fact]
    public async Task GetHotelByIdAsync_NonExistingHotel_ReturnsNull()
    {
        // Act
        var result = await _sut.GetHotelByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllHotelsAsync_WithHotels_ReturnsAllHotels()
    {
        // Arrange
        await CreateTestHotel("Hotel 1", 100m, 45.8150, 15.9819);
        await CreateTestHotel("Hotel 2", 200m, 45.8160, 15.9829);
        await CreateTestHotel("Hotel 3", 150m, 45.8170, 15.9839);

        // Act
        var result = await _sut.GetAllHotelsAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task UpdateHotelAsync_ExistingHotel_ReturnsUpdatedHotel()
    {
        // Arrange
        var hotel = await CreateTestHotel("Original Name", 100m, 45.8150, 15.9819);
        var updateDto = new UpdateHotelDto
        {
            Name = "Updated Name",
            Price = 200m,
            Latitude = 46.0,
            Longitude = 16.0
        };

        // Act
        var result = await _sut.UpdateHotelAsync(hotel.Id, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Price.Should().Be(200m);
    }

    [Fact]
    public async Task UpdateHotelAsync_NonExistingHotel_ReturnsNull()
    {
        // Arrange
        var updateDto = new UpdateHotelDto { Name = "Test", Price = 100m, Latitude = 45.0, Longitude = 15.0 };

        // Act
        var result = await _sut.UpdateHotelAsync(999, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteHotelAsync_ExistingHotel_ReturnsTrue()
    {
        // Arrange
        var hotel = await CreateTestHotel("Test Hotel", 100m, 45.8150, 15.9819);

        // Act
        var result = await _sut.DeleteHotelAsync(hotel.Id);

        // Assert
        result.Should().BeTrue();
        var deletedHotel = await _sut.GetHotelByIdAsync(hotel.Id);
        deletedHotel.Should().BeNull();
    }

    [Fact]
    public async Task DeleteHotelAsync_NonExistingHotel_ReturnsFalse()
    {
        // Act
        var result = await _sut.DeleteHotelAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchHotelsAsync_NoHotels_ReturnsEmptyResult()
    {
        // Arrange
        var request = new HotelSearchRequestDto
        {
            UserLatitude = 45.8150,
            UserLongitude = 15.9819
        };

        // Act
        var result = await _sut.SearchHotelsAsync(request);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchHotelsAsync_WithHotels_ReturnsSortedByScore()
    {
        // Arrange - Create hotels with different prices and distances
        // User location: Zagreb center (45.8150, 15.9819)
        await CreateTestHotel("Cheap & Close", 50m, 45.8151, 15.9820);   // Best score
        await CreateTestHotel("Expensive & Far", 300m, 45.8300, 16.0000); // Worst score
        await CreateTestHotel("Mid Range", 150m, 45.8200, 15.9900);       // Middle score

        var request = new HotelSearchRequestDto
        {
            UserLatitude = 45.8150,
            UserLongitude = 15.9819,
            SortBy = SortBy.Score,
            SortDirection = SortDirection.Asc
        };

        // Act
        var result = await _sut.SearchHotelsAsync(request);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items.First().Name.Should().Be("Cheap & Close");
        result.Items.Last().Name.Should().Be("Expensive & Far");
    }

    [Fact]
    public async Task SearchHotelsAsync_WithRadiusFilter_ReturnsOnlyNearbyHotels()
    {
        // Arrange
        await CreateTestHotel("Close Hotel", 100m, 45.8151, 15.9820);     // ~100m away
        await CreateTestHotel("Far Hotel", 100m, 45.9000, 16.1000);       // ~15km away

        var request = new HotelSearchRequestDto
        {
            UserLatitude = 45.8150,
            UserLongitude = 15.9819,
            RadiusKm = 1 // 1km radius
        };

        // Act
        var result = await _sut.SearchHotelsAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Close Hotel");
    }

    [Fact]
    public async Task SearchHotelsAsync_WithKeywordFilter_ReturnsMatchingHotels()
    {
        // Arrange
        await CreateTestHotel("Grand Hotel Zagreb", 200m, 45.8150, 15.9819);
        await CreateTestHotel("Hotel Esplanade", 250m, 45.8050, 15.9780);
        await CreateTestHotel("Apartment Center", 80m, 45.8100, 15.9800);

        var request = new HotelSearchRequestDto
        {
            UserLatitude = 45.8150,
            UserLongitude = 15.9819,
            SearchKeywords = "hotel"
        };

        // Act
        var result = await _sut.SearchHotelsAsync(request);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(h => h.Name.Contains("Hotel", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchHotelsAsync_Pagination_ReturnsCorrectPage()
    {
        // Arrange - Create 15 hotels
        for (int i = 1; i <= 15; i++)
        {
            await CreateTestHotel($"Hotel {i}", 100m + i, 45.8150 + (i * 0.001), 15.9819);
        }

        var request = new HotelSearchRequestDto
        {
            UserLatitude = 45.8150,
            UserLongitude = 15.9819,
            PageNumber = 2,
            PageSize = 5
        };

        // Act
        var result = await _sut.SearchHotelsAsync(request);

        // Assert
        result.Items.Should().HaveCount(5);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(15);
    }

    [Fact]
    public async Task SearchHotelsAsync_SortByPrice_ReturnsSortedByPrice()
    {
        // Arrange
        await CreateTestHotel("Expensive Hotel", 300m, 45.8150, 15.9819);
        await CreateTestHotel("Cheap Hotel", 50m, 45.8151, 15.9820);
        await CreateTestHotel("Mid Hotel", 150m, 45.8152, 15.9821);

        var request = new HotelSearchRequestDto
        {
            UserLatitude = 45.8150,
            UserLongitude = 15.9819,
            SortBy = SortBy.Price,
            SortDirection = SortDirection.Asc
        };

        // Act
        var result = await _sut.SearchHotelsAsync(request);

        // Assert
        result.Items.First().Name.Should().Be("Cheap Hotel");
        result.Items.Last().Name.Should().Be("Expensive Hotel");
    }

    [Fact]
    public async Task SearchHotelsAsync_SortByName_ReturnsSortedByName()
    {
        // Arrange
        await CreateTestHotel("Zebra Hotel", 100m, 45.8150, 15.9819);
        await CreateTestHotel("Alpha Hotel", 100m, 45.8151, 15.9820);
        await CreateTestHotel("Beta Hotel", 100m, 45.8152, 15.9821);

        var request = new HotelSearchRequestDto
        {
            UserLatitude = 45.8150,
            UserLongitude = 15.9819,
            SortBy = SortBy.Name,
            SortDirection = SortDirection.Asc
        };

        // Act
        var result = await _sut.SearchHotelsAsync(request);

        // Assert
        result.Items.First().Name.Should().Be("Alpha Hotel");
        result.Items.Last().Name.Should().Be("Zebra Hotel");
    }

    [Fact]
    public async Task SearchHotelsAsync_SortDescending_ReturnsReversedOrder()
    {
        // Arrange
        await CreateTestHotel("Cheap Hotel", 50m, 45.8150, 15.9819);
        await CreateTestHotel("Expensive Hotel", 300m, 45.8151, 15.9820);

        var request = new HotelSearchRequestDto
        {
            UserLatitude = 45.8150,
            UserLongitude = 15.9819,
            SortBy = SortBy.Price,
            SortDirection = SortDirection.Desc
        };

        // Act
        var result = await _sut.SearchHotelsAsync(request);

        // Assert
        result.Items.First().Name.Should().Be("Expensive Hotel");
        result.Items.Last().Name.Should().Be("Cheap Hotel");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task SearchHotelsAsync_AllHotelsAtSameLocation_ScoreBasedOnPriceOnly()
    {
        // Arrange - All hotels at exact same location
        await CreateTestHotel("Cheap", 50m, 45.8150, 15.9819);
        await CreateTestHotel("Expensive", 200m, 45.8150, 15.9819);

        var request = new HotelSearchRequestDto
        {
            UserLatitude = 45.8150,
            UserLongitude = 15.9819,
            SortBy = SortBy.Score,
            SortDirection = SortDirection.Asc
        };

        // Act
        var result = await _sut.SearchHotelsAsync(request);

        // Assert - Cheaper hotel should be first
        result.Items.First().Name.Should().Be("Cheap");
    }

    [Fact]
    public async Task SearchHotelsAsync_AllHotelsSamePrice_ScoreBasedOnDistanceOnly()
    {
        // Arrange - All hotels same price
        await CreateTestHotel("Close", 100m, 45.8151, 15.9820);
        await CreateTestHotel("Far", 100m, 45.8300, 16.0000);

        var request = new HotelSearchRequestDto
        {
            UserLatitude = 45.8150,
            UserLongitude = 15.9819,
            SortBy = SortBy.Score,
            SortDirection = SortDirection.Asc
        };

        // Act
        var result = await _sut.SearchHotelsAsync(request);

        // Assert - Closer hotel should be first
        result.Items.First().Name.Should().Be("Close");
    }

    [Fact]
    public async Task SearchHotelsAsync_SingleHotel_ReturnsWithZeroScore()
    {
        // Arrange - Only one hotel (normalization edge case)
        await CreateTestHotel("Only Hotel", 100m, 45.8150, 15.9819);

        var request = new HotelSearchRequestDto
        {
            UserLatitude = 45.8150,
            UserLongitude = 15.9819
        };

        // Act
        var result = await _sut.SearchHotelsAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Score.Should().Be(0); // No range = 0 score
    }

    [Fact]
    public async Task SearchHotelsAsync_RadiusZeroHotels_ReturnsEmpty()
    {
        // Arrange
        await CreateTestHotel("Far Hotel", 100m, 46.0, 16.0);

        var request = new HotelSearchRequestDto
        {
            UserLatitude = 45.8150,
            UserLongitude = 15.9819,
            RadiusKm = 0.1 // Very small radius
        };

        // Act
        var result = await _sut.SearchHotelsAsync(request);

        // Assert
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchHotelsAsync_LastPage_ReturnsRemainingItems()
    {
        // Arrange - Create 12 hotels
        for (int i = 1; i <= 12; i++)
        {
            await CreateTestHotel($"Hotel {i}", 100m, 45.8150, 15.9819);
        }

        var request = new HotelSearchRequestDto
        {
            UserLatitude = 45.8150,
            UserLongitude = 15.9819,
            PageNumber = 3,
            PageSize = 5
        };

        // Act
        var result = await _sut.SearchHotelsAsync(request);

        // Assert - Last page should have only 2 items (12 - 10)
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchHotelsAsync_PageBeyondResults_ReturnsEmpty()
    {
        // Arrange
        await CreateTestHotel("Hotel 1", 100m, 45.8150, 15.9819);

        var request = new HotelSearchRequestDto
        {
            UserLatitude = 45.8150,
            UserLongitude = 15.9819,
            PageNumber = 10, // Way beyond
            PageSize = 10
        };

        // Act
        var result = await _sut.SearchHotelsAsync(request);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(1);
    }

    #endregion

    #region Helper Methods

    private async Task<Hotel> CreateTestHotel(string name, decimal price, double lat, double lon)
    {
        var hotel = new Hotel
        {
            Name = name,
            Price = price,
            Latitude = lat,
            Longitude = lon
        };

        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();
        return hotel;
    }

    #endregion
}
