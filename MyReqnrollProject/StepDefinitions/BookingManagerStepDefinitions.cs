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
    private Exception caughtException;

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
        try
        {
            result = await manager.CreateBooking(booking);
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }
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

    [Then("the booking should throw an exception")]
    public void ThenTheBookingShouldThrowAnException()
    {
        Assert.That(caughtException, Is.Not.Null);
    }

    //New functions to handle ECT/BVT

    [Given("a room has an active booking from day {int} to day {int}")]
    public void GivenARoomHasAnActiveBookingFromDayToDay(int existingStartDayOffset, int existingEndDayOffset)
    {
        bookingRepo = new Mock<IRepository<Booking>>();
        roomRepo = new Mock<IRepository<Room>>();

        roomRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Room>
                {
                    new Room { Id = 1, Description = "Room 1" }
                });

        bookingRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Booking>
                {
                    new Booking
                    {
                        Id = 1,
                        RoomId = 1,
                        IsActive = true,
                        StartDate = DateTime.Today.AddDays(existingStartDayOffset),
                        EndDate = DateTime.Today.AddDays(existingEndDayOffset)
                    }
                });

        manager = new BookingManager(bookingRepo.Object, roomRepo.Object);
    }

    [Given("there are no existing bookings")]
    public void GivenThereAreNoExistingBookings()
    {
        bookingRepo = new Mock<IRepository<Booking>>();
        roomRepo = new Mock<IRepository<Room>>();

        roomRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Room>
                {
                    new Room { Id = 1, Description = "Room 1" }
                });

        bookingRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Booking>());

        manager = new BookingManager(bookingRepo.Object, roomRepo.Object);
    }

    [Given("i want to create a booking from day {int} to day {int}")]
    public void GivenIWantToCreateABookingFromDayToDay(int newStartDayOffset, int newEndDayOffset)
    {
        booking = new Booking
        {
            Id = 2,
            CustomerId = 1,
            RoomId = 1,
            StartDate = DateTime.Today.AddDays(newStartDayOffset),
            EndDate = DateTime.Today.AddDays(newEndDayOffset)
        };
    }

    [When("i create a booking with these dates")]
    public async Task WhenICreateABookingWithTheseDates()
    {
        try
        {
            result = await manager.CreateBooking(booking);
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }
    }

    [Given("i want to book a room where startdate is day {int} and enddate is day {int}")]
    public void AndEndDateIsSpecificDay(int startDate, int endDate)
    {
        booking = new Booking
        {
            Id = 2,
            CustomerId = 1,
            StartDate = DateTime.Today.AddDays(startDate),
            EndDate = DateTime.Today.AddDays(endDate)
        };
    }

}
