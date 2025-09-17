using Microsoft.AspNetCore.Identity;
using OnlineEventTicketing.Data.Entity;

namespace OnlineEventTicketing.Data
{
    public static class SeedData
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Define roles
            string[] roleNames = { "Admin", "EventOrganizer", "Customer" };

            // Create roles if they don't exist
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create default admin user
            var adminEmail = "admin@eventticketing.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Create sample event organizer
            var organizerEmail = "organizer@eventticketing.com";
            var organizerUser = await userManager.FindByEmailAsync(organizerEmail);

            if (organizerUser == null)
            {
                organizerUser = new ApplicationUser
                {
                    UserName = organizerEmail,
                    Email = organizerEmail,
                    FirstName = "Sample",
                    LastName = "Organizer",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(organizerUser, "Organizer@123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(organizerUser, "EventOrganizer");
                }
            }

            // Create sample customer
            var customerEmail = "customer@eventticketing.com";
            var customerUser = await userManager.FindByEmailAsync(customerEmail);

            if (customerUser == null)
            {
                customerUser = new ApplicationUser
                {
                    UserName = customerEmail,
                    Email = customerEmail,
                    FirstName = "Sample",
                    LastName = "Customer",
                    EmailConfirmed = true,
                    LoyaltyPoints = 100
                };

                var result = await userManager.CreateAsync(customerUser, "Customer@123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(customerUser, "Customer");
                }
            }
        }

        public static async Task SeedSampleDataAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Seed sample events if none exist
            if (!context.Events.Any())
            {
                var organizer = await userManager.FindByEmailAsync("organizer@eventticketing.com");
                if (organizer != null)
                {
                    var sampleEvents = new[]
                    {
                        new Event
                        {
                            Title = "Tech Conference 2024",
                            Description = "Join us for the biggest technology conference of the year featuring industry leaders and cutting-edge innovations.",
                            Date = DateTime.UtcNow.AddDays(30),
                            Location = "Convention Center, New York",
                            Category = "Technology",
                            Capacity = 500,
                            BasePrice = 299.99m,
                            ImageUrl = "https://images.unsplash.com/photo-1540575467063-178a50c2df87",
                            IsActive = true,
                            OrganizerId = organizer.Id,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Event
                        {
                            Title = "Summer Music Festival",
                            Description = "Experience amazing live music performances from top artists in a beautiful outdoor setting.",
                            Date = DateTime.UtcNow.AddDays(45),
                            Location = "Central Park, New York",
                            Category = "Music",
                            Capacity = 2000,
                            BasePrice = 75.00m,
                            ImageUrl = "https://images.unsplash.com/photo-1493676304819-0d7a8d026dcf",
                            IsActive = true,
                            OrganizerId = organizer.Id,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Event
                        {
                            Title = "Food & Wine Tasting",
                            Description = "Discover exquisite flavors and premium wines in an elegant setting with renowned chefs.",
                            Date = DateTime.UtcNow.AddDays(15),
                            Location = "Grand Hotel, San Francisco",
                            Category = "Food",
                            Capacity = 150,
                            BasePrice = 125.50m,
                            ImageUrl = "https://images.unsplash.com/photo-1510812431401-41d2bd2722f3",
                            IsActive = true,
                            OrganizerId = organizer.Id,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Event
                        {
                            Title = "Basketball Championship",
                            Description = "Watch the most exciting basketball game of the season with your favorite teams competing for the title.",
                            Date = DateTime.UtcNow.AddDays(60),
                            Location = "Sports Arena, Los Angeles",
                            Category = "Sports",
                            Capacity = 15000,
                            BasePrice = 89.99m,
                            ImageUrl = "https://images.unsplash.com/photo-1546519638-68e109498ffc",
                            IsActive = true,
                            OrganizerId = organizer.Id,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Event
                        {
                            Title = "Art Exhibition Opening",
                            Description = "Explore contemporary art pieces from emerging and established artists in this exclusive gallery opening.",
                            Date = DateTime.UtcNow.AddDays(20),
                            Location = "Modern Art Gallery, Chicago",
                            Category = "Arts",
                            Capacity = 200,
                            BasePrice = 45.00m,
                            ImageUrl = "https://images.unsplash.com/photo-1578662996442-48f60103fc96",
                            IsActive = true,
                            OrganizerId = organizer.Id,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Event
                        {
                            Title = "Comedy Night Live",
                            Description = "Laugh out loud with the funniest comedians in town performing their best material live on stage.",
                            Date = DateTime.UtcNow.AddDays(10),
                            Location = "Comedy Club, Miami",
                            Category = "Comedy",
                            Capacity = 300,
                            BasePrice = 35.00m,
                            ImageUrl = "https://images.unsplash.com/photo-1485846234645-a62644f84728",
                            IsActive = true,
                            OrganizerId = organizer.Id,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    foreach (var eventItem in sampleEvents)
                    {
                        eventItem.AvailableTickets = eventItem.Capacity;
                        context.Events.Add(eventItem);
                    }

                    await context.SaveChangesAsync();
                }
            }

            // Seed sample promotions
            if (!context.Promotions.Any() && context.Events.Any())
            {
                var firstEvent = context.Events.First();
                var samplePromotions = new[]
                {
                    new Promotion
                    {
                        Code = "WELCOME20",
                        Description = "20% off for new customers",
                        DiscountPercentage = 20,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddDays(30),
                        IsActive = true,
                        EventId = firstEvent.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Promotion
                    {
                        Code = "EARLYBIRD",
                        Description = "Early bird discount - 15% off",
                        DiscountPercentage = 15,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddDays(14),
                        IsActive = true,
                        EventId = firstEvent.Id,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                foreach (var promotion in samplePromotions)
                {
                    context.Promotions.Add(promotion);
                }

                await context.SaveChangesAsync();
            }
        }
    }
}