using HotelSearchLemax.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelSearchLemax.DataAccess.Data;

public static class DbSeeder
{
    public static void Seed(HotelDbContext context)
    {
        if (context.Hotels.Any())
            return;

        var hotels = new List<Hotel>
        {
            // Central Zagreb - Downtown
            new Hotel
            {
                Name = "Esplanade Zagreb Hotel",
                Price = 250.00m,
                Latitude = 45.8047,
                Longitude = 15.9785
            },
            new Hotel
            {
                Name = "Hotel Dubrovnik",
                Price = 150.00m,
                Latitude = 45.8131,
                Longitude = 15.9775
            },
            new Hotel
            {
                Name = "Sheraton Zagreb Hotel",
                Price = 180.00m,
                Latitude = 45.8058,
                Longitude = 15.9628
            },
            new Hotel
            {
                Name = "Westin Zagreb",
                Price = 200.00m,
                Latitude = 45.8069,
                Longitude = 15.9633
            },
            new Hotel
            {
                Name = "Hotel Jagerhorn",
                Price = 95.00m,
                Latitude = 45.8138,
                Longitude = 15.9764
            },
            new Hotel
            {
                Name = "Palace Hotel Zagreb",
                Price = 130.00m,
                Latitude = 45.8092,
                Longitude = 15.9697
            },
            new Hotel
            {
                Name = "DoubleTree by Hilton Zagreb",
                Price = 140.00m,
                Latitude = 45.8106,
                Longitude = 15.9697
            },
            new Hotel
            {
                Name = "Hotel Academia",
                Price = 85.00m,
                Latitude = 45.8028,
                Longitude = 15.9714
            },
            new Hotel
            {
                Name = "Hotel Aristos",
                Price = 110.00m,
                Latitude = 45.7897,
                Longitude = 15.9458
            },
            new Hotel
            {
                Name = "Best Western Premier Hotel Astoria",
                Price = 120.00m,
                Latitude = 45.8094,
                Longitude = 15.9731
            },
            // Upper Town (Gornji Grad)
            new Hotel
            {
                Name = "Hotel Capital",
                Price = 75.00m,
                Latitude = 45.8148,
                Longitude = 15.9736
            },
            new Hotel
            {
                Name = "Canopy by Hilton Zagreb",
                Price = 160.00m,
                Latitude = 45.8125,
                Longitude = 15.9768
            },
            // Near Airport / Outskirts
            new Hotel
            {
                Name = "Hotel Airportus",
                Price = 65.00m,
                Latitude = 45.7397,
                Longitude = 16.0688
            },
            new Hotel
            {
                Name = "Hotel Panorama Zagreb",
                Price = 90.00m,
                Latitude = 45.8003,
                Longitude = 15.9894
            },
            new Hotel
            {
                Name = "Hotel International",
                Price = 100.00m,
                Latitude = 45.8033,
                Longitude = 15.9619
            },
            // Novi Zagreb (New Zagreb)
            new Hotel
            {
                Name = "Hotel I",
                Price = 70.00m,
                Latitude = 45.7856,
                Longitude = 15.9819
            },
            new Hotel
            {
                Name = "Hotel Laguna",
                Price = 80.00m,
                Latitude = 45.7936,
                Longitude = 15.9728
            },
            // Near Main Square (Trg bana Jelacica)
            new Hotel
            {
                Name = "Hotel Central",
                Price = 88.00m,
                Latitude = 45.8119,
                Longitude = 15.9767
            },
            new Hotel
            {
                Name = "Hotel Jadran",
                Price = 72.00m,
                Latitude = 45.8089,
                Longitude = 15.9781
            },
            new Hotel
            {
                Name = "Art Hotel Like",
                Price = 105.00m,
                Latitude = 45.8061,
                Longitude = 15.9753
            }
        };

        context.Hotels.AddRange(hotels);
        context.SaveChanges();
    }

    public static async Task SeedAsync(HotelDbContext context)
    {
        if (await context.Hotels.AnyAsync())
            return;

        var hotels = new List<Hotel>
        {
            // Central Zagreb - Downtown
            new Hotel
            {
                Name = "Esplanade Zagreb Hotel",
                Price = 250.00m,
                Latitude = 45.8047,
                Longitude = 15.9785
            },
            new Hotel
            {
                Name = "Hotel Dubrovnik",
                Price = 150.00m,
                Latitude = 45.8131,
                Longitude = 15.9775
            },
            new Hotel
            {
                Name = "Sheraton Zagreb Hotel",
                Price = 180.00m,
                Latitude = 45.8058,
                Longitude = 15.9628
            },
            new Hotel
            {
                Name = "Westin Zagreb",
                Price = 200.00m,
                Latitude = 45.8069,
                Longitude = 15.9633
            },
            new Hotel
            {
                Name = "Hotel Jagerhorn",
                Price = 95.00m,
                Latitude = 45.8138,
                Longitude = 15.9764
            },
            new Hotel
            {
                Name = "Palace Hotel Zagreb",
                Price = 130.00m,
                Latitude = 45.8092,
                Longitude = 15.9697
            },
            new Hotel
            {
                Name = "DoubleTree by Hilton Zagreb",
                Price = 140.00m,
                Latitude = 45.8106,
                Longitude = 15.9697
            },
            new Hotel
            {
                Name = "Hotel Academia",
                Price = 85.00m,
                Latitude = 45.8028,
                Longitude = 15.9714
            },
            new Hotel
            {
                Name = "Hotel Aristos",
                Price = 110.00m,
                Latitude = 45.7897,
                Longitude = 15.9458
            },
            new Hotel
            {
                Name = "Best Western Premier Hotel Astoria",
                Price = 120.00m,
                Latitude = 45.8094,
                Longitude = 15.9731
            },
            // Upper Town (Gornji Grad)
            new Hotel
            {
                Name = "Hotel Capital",
                Price = 75.00m,
                Latitude = 45.8148,
                Longitude = 15.9736
            },
            new Hotel
            {
                Name = "Canopy by Hilton Zagreb",
                Price = 160.00m,
                Latitude = 45.8125,
                Longitude = 15.9768
            },
            // Near Airport / Outskirts
            new Hotel
            {
                Name = "Hotel Airportus",
                Price = 65.00m,
                Latitude = 45.7397,
                Longitude = 16.0688
            },
            new Hotel
            {
                Name = "Hotel Panorama Zagreb",
                Price = 90.00m,
                Latitude = 45.8003,
                Longitude = 15.9894
            },
            new Hotel
            {
                Name = "Hotel International",
                Price = 100.00m,
                Latitude = 45.8033,
                Longitude = 15.9619
            },
            // Novi Zagreb (New Zagreb)
            new Hotel
            {
                Name = "Hotel I",
                Price = 70.00m,
                Latitude = 45.7856,
                Longitude = 15.9819
            },
            new Hotel
            {
                Name = "Hotel Laguna",
                Price = 80.00m,
                Latitude = 45.7936,
                Longitude = 15.9728
            },
            // Near Main Square (Trg bana Jelacica)
            new Hotel
            {
                Name = "Hotel Central",
                Price = 88.00m,
                Latitude = 45.8119,
                Longitude = 15.9767
            },
            new Hotel
            {
                Name = "Hotel Jadran",
                Price = 72.00m,
                Latitude = 45.8089,
                Longitude = 15.9781
            },
            new Hotel
            {
                Name = "Art Hotel Like",
                Price = 105.00m,
                Latitude = 45.8061,
                Longitude = 15.9753
            }
        };

        await context.Hotels.AddRangeAsync(hotels);
        await context.SaveChangesAsync();
    }
}
