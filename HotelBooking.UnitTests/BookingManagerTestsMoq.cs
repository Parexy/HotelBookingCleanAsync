using HotelBooking.Core;
using HotelBooking.UnitTests.Fakes;
using Moq;
using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;


namespace HotelBooking.UnitTests
{
    public class BookingManagerTestsMoq
    {
        private readonly BookingManager bookingManager; 

        private readonly Mock<IRepository<Booking>> bookingRepoMock = new();
        private readonly Mock<IRepository<Room>> roomRepoMock = new();

        // In-memory “DB” for the mocks
        private readonly List<Booking> bookings = new();
        private readonly List<Room> rooms = new();

        private readonly SystemTime time = new();

        public BookingManagerTestsMoq()
        {
            time.Set(new DateTime(2026, 1, 2));
            var today = time.Today;

            rooms.AddRange(new[]
            {
                new Room { Id = 1, Description = "A" },
                new Room { Id = 2, Description = "B" },
            });

            var start = today.AddDays(10);
            var end = today.AddDays(20);

            bookings.AddRange(new[]
            {
                new Booking { Id=1, StartDate=today.AddDays(1), EndDate=today.AddDays(1), IsActive=true, CustomerId=1, RoomId=1 },
                new Booking { Id=2, StartDate=start, EndDate=end, IsActive=true, CustomerId=1, RoomId=1 },
                new Booking { Id=3, StartDate=start, EndDate=end, IsActive=true, CustomerId=2, RoomId=2 },
            });

            bookingRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(() => bookings.ToList());

            roomRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(() => rooms.ToList());

            bookingRepoMock.Setup(r => r.AddAsync(It.IsAny<Booking>()))
                .Returns((Booking b) =>
                {
                    bookings.Add(b);
                    return Task.CompletedTask;
                });

            bookingManager = new BookingManager(bookingRepoMock.Object, roomRepoMock.Object, time);
        }

        [Fact]
        public async Task FindAvailableRoom_StartDateNotInTheFuture_ThrowsArgumentException()
        {
            // Arrange
            DateTime date = time.Today;

            // Act
            Task result() => bookingManager.FindAvailableRoom(date, date);
 
            // Assert
            await Assert.ThrowsAsync<ArgumentException>(result);
        }

        [Theory]
        [InlineData("2026-01-02", "2026-01-02")]
        [InlineData("2026-01-02", "2026-01-03")]
        [InlineData("2026-01-02", "2026-01-01")]
        public async Task FindAvailableRoom_InvalidDates_ThrowsArgumentException_DataDriven(string start, string end)
        {
            var startDate = DateTime.Parse(start);
            var endDate = DateTime.Parse(end);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                bookingManager.FindAvailableRoom(startDate, endDate));
        }

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_RoomIdNotMinusOne()
        {
            // Arrange
            DateTime date = time.Today.AddDays(1);
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);
            // Assert
            Assert.NotEqual(-1, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_ReturnsAvailableRoom()
        {
            // This test was added to satisfy the following test design
            // principle: "Tests should have strong assertions".

            // Arrange
            DateTime date = time.Today.AddDays(1);
            
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);

            var bookingForReturnedRoomId = (await bookingRepoMock.Object.GetAllAsync()).
                Where(b => b.RoomId == roomId
                           && b.StartDate <= date
                           && b.EndDate >= date
                           && b.IsActive
                );
            
            // Assert
            Assert.Empty(bookingForReturnedRoomId);
        }

        [Fact]
        public async Task FindAvailableRoom_CreateBooking_BookingSucess()
        {
            // Arrange
            DateTime dateStart = time.Today.AddDays(5);
            DateTime dateEnd = time.Today.AddDays(9);

            Booking newBooking = new()
            {
                Id = 3,
                CustomerId = 1,
                IsActive = true,
                StartDate = dateStart,
                EndDate = dateEnd,
                RoomId = 1,
            };

            // Act
            bool result = await bookingManager.CreateBooking(newBooking);

            // Assert
            Assert.True(result);

        }

        [Fact]
        public async Task FindAvailableRoom_CreateBooking_BookingBackInTime()
        {
            // Arrange
            DateTime dateStart = time.Today.AddDays(-2);
            DateTime dateEnd = time.Today.AddDays(9);

            Booking newBooking = new()
            {
                Id = 3,
                CustomerId = 1,
                IsActive = true,
                StartDate = dateStart,
                EndDate = dateEnd,
                RoomId = 2,
            };

            // Act + Assert
            await Assert.ThrowsAnyAsync<ArgumentException>(() =>
                bookingManager.CreateBooking(newBooking));

        }

        [Fact]
        public async Task FindAvailableRoom_CreateBooking_BookingSameDay()
        {
            // Arrange
            DateTime dateStart = time.Today.AddDays(1);
            DateTime dateEnd = time.Today.AddDays(1);

            Booking newBooking = new()
            {
                Id = 3,
                CustomerId = 2,
                IsActive = true,
                StartDate = dateStart,
                EndDate = dateEnd,
                RoomId = 2,
            };

            // Act
            bool result = await bookingManager.CreateBooking(newBooking);

            // Assert
            Assert.True(result);

        }

        [Fact]
        public async Task FindAvailableRoom_CreateBooking_RoomAlreadyBooked()
        {
            // Arrange
            DateTime dateStart = time.Today.AddDays(10);
            DateTime dateEnd = time.Today.AddDays(20);

            Booking duplicateBooking = new()
            {
                Id = 5,
                CustomerId = 1,
                IsActive = true,
                StartDate = dateStart,
                EndDate = dateEnd,
                RoomId = 1,
            };

            // Act
            bool result = await bookingManager.CreateBooking(duplicateBooking);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task FindAvailableRoom_CreateBooking_BookingOverlapButFreeOnStartAndEndDate()
        {
            // Arrange
            DateTime dateStart = time.Today.AddDays(9);
            DateTime dateEnd = time.Today.AddDays(19);

            Booking overlappingBooking = new()
            {
                Id = 6,
                CustomerId = 1,
                IsActive = true,
                StartDate = dateStart.AddDays(-1),
                EndDate = dateEnd.AddDays(1),
                RoomId = 1,
            };

            // Act
            bool result = await bookingManager.CreateBooking(overlappingBooking);

            // Assert
            Assert.False(result);

        }

        [Fact]
        public async Task FindAvailableRoom_CreateBooking_OnePersonBooksMoreThan1RoomAtTheSameDate()
        {
            // Arrange
            DateTime dateStart = time.Today.AddDays(1);
            DateTime dateEnd = time.Today.AddDays(1);

            Booking newBooking = new()
            {
                Id = 3,
                CustomerId = 1,
                IsActive = true,
                StartDate = dateStart,
                EndDate = dateEnd,
                RoomId = 2,
            };

            // Act
            bool result = await bookingManager.CreateBooking(newBooking);

            // Assert
            Assert.True(result);

        }

        [Fact]
        public async Task FindAvailableRoom_CreateBooking_BookingNotOverlapingAtStartButAtEnd()
        {
            // Arrange
            DateTime dateStart = time.Today.AddDays(9);
            DateTime dateEnd = time.Today.AddDays(15);

            Booking newBooking = new()
            {
                Id = 3,
                CustomerId = 1,
                IsActive = true,
                StartDate = dateStart,
                EndDate = dateEnd,
                RoomId = 2,
            };

            // Act
            bool result = await bookingManager.CreateBooking(newBooking);

            // Assert
            Assert.False(result);

        }

        [Fact]
        public async Task FindAvailableRoom_CreateBooking_BookingOverlapingAtStartButNotAtEnd()
        {
            // Arrange
            DateTime dateStart = time.Today.AddDays(11);
            DateTime dateEnd = time.Today.AddDays(21);

            Booking newBooking = new()
            {
                Id = 3,
                CustomerId = 1,
                IsActive = true,
                StartDate = dateStart,
                EndDate = dateEnd,
                RoomId = 2,
            };

            // Act
            bool result = await bookingManager.CreateBooking(newBooking);

            // Assert
            Assert.False(result);

        }

        [Fact]
        public async Task FindAvailableRoom_CreateBooking_CanBookRoomAfterEmpty()
        {
            // Arrange
            DateTime dateStart = time.Today.AddDays(21);
            DateTime dateEnd = time.Today.AddDays(29);

            Booking newBooking = new()
            {
                Id = 3,
                CustomerId = 1,
                IsActive = true,
                StartDate = dateStart,
                EndDate = dateEnd,
                RoomId = 2,
            };

            // Act
            bool result = await bookingManager.CreateBooking(newBooking);

            // Assert
            Assert.True(result);

        }

        [Fact]
        public async Task FindAvailableRoom_StartDateInPastEndDateInFuture_ThrowsArgumentException() {

            // Arrange
            DateTime date = time.Today;

            // Act
            Task result() => bookingManager.FindAvailableRoom(date, date.AddDays(1));

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(result);

        }

        [Fact]
        public async Task FindAvailableRoom_StartDateLaterThanEndDate_ThrowsArgumentException() {

            // Arrange
            DateTime date = time.Today;

            // Act
            Task result() => bookingManager.FindAvailableRoom(date, date.AddDays(-1));

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(result);

        }

        [Fact]
        public async Task GetFullyOccupiedDates_Start_ThrowsArgumentException() {

            // Arrange
            DateTime date = time.Today;

            // Act
            List<DateTime> result = await bookingManager.GetFullyOccupiedDates(date, date);

            // Assert
            Assert.False(result.Any());

        }

        [Fact]
        public async Task GetFullyOccupiedDates_Bookings_NonEmptyList() {

            // Arrange
            DateTime startDate = time.Today.AddDays(10);
            DateTime endDate = time.Today.AddDays(20);

            // Act
            List<DateTime> result = await bookingManager.GetFullyOccupiedDates(startDate, endDate);

            // Assert
            Assert.True(result.Any());

        }

        [Fact]
        public async Task GetFullyOccupiedDates_NoBookings_EmptyList() {

            // Arrange
            DateTime date = time.Today;

            // Act
            List<DateTime> result = await bookingManager.GetFullyOccupiedDates(date, date.AddDays(3));

            // Assert
            Assert.False(result.Any());

        }

        
    }
}
