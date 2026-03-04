using HotelBooking.Core;
using Moq;
using NUnit.Framework;
using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyReqnrollProject.StepDefinitions;

[Binding]
public sealed class BookingManagerStepDefinitions
{
    private BookingManager manager;
    private Booking booking;
    private bool result;

    private Mock<IRepository<Booking>> bookingRepo;
    private Mock<IRepository<Room>> roomRepo;

    [Given("a room is available")]
    public void GivenARoomIsAvailable()
    {
        Setup(roomIsAvailable: true);
    }

    [Given("a room is not available")]
    public void GivenARoomIsNotAvailable()
    {
        Setup(roomIsAvailable: false);
    }

    private void Setup(bool roomIsAvailable)
    {
        bookingRepo = new Mock<IRepository<Booking>>();
        roomRepo = new Mock<IRepository<Room>>();

        // Hotel has exactly 1 room
        roomRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Room> { new Room { Id = 1, Description = "Room 1" } });

        var existingBookings = roomIsAvailable
            ? new List<Booking>()
            : new List<Booking> 
            {
                new Booking
                {
                    Id = 1,
                    RoomId = 1,
                    IsActive = true,
                    StartDate = DateTime.Today.AddDays(10),
                    EndDate   = DateTime.Today.AddDays(20)
                }
            };

        bookingRepo.Setup(r => r.GetAllAsync())
                   .ReturnsAsync(existingBookings);

        manager = new BookingManager(bookingRepo.Object, roomRepo.Object);

        // Desired booking overlaps the "blocked" range if the room is not available
        booking = new Booking
        {
            Id = 2,
            CustomerId = 1,
            StartDate = DateTime.Today.AddDays(12),
            EndDate = DateTime.Today.AddDays(14)
        };
    }

    [When("i create a booking")]
    public async Task WhenICreateABooking()
    {
        result = await manager.CreateBooking(booking);
    }

    [Then("the booking should be successful")]
    public void ThenTheBookingShouldBeSuccessful()
    {
        Assert.That(result, Is.True);
        bookingRepo.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Once);
    }

    [Then("the booking should be unsuccessful")]
    public void ThenTheBookingShouldBeUnsuccessful()
    {
        Assert.That(result, Is.False);
        bookingRepo.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
    }
}
