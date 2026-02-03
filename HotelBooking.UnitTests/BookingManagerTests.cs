using System;
using HotelBooking.Core;
using HotelBooking.UnitTests.Fakes;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private IBookingManager bookingManager;
        IRepository<Booking> bookingRepository;

        public BookingManagerTests(){
            DateTime start = DateTime.Today.AddDays(10);
            DateTime end = DateTime.Today.AddDays(20);
            bookingRepository = new FakeBookingRepository(start, end);
            IRepository<Room> roomRepository = new FakeRoomRepository();
            bookingManager = new BookingManager(bookingRepository, roomRepository);
        }

        [Fact]
        public async Task FindAvailableRoom_StartDateNotInTheFuture_ThrowsArgumentException()
        {
            // Arrange
            DateTime date = DateTime.Today;

            // Act
            Task result() => bookingManager.FindAvailableRoom(date, date);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(result);
        }

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_RoomIdNotMinusOne()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
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
            DateTime date = DateTime.Today.AddDays(1);
            
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);

            var bookingForReturnedRoomId = (await bookingRepository.GetAllAsync()).
                Where(b => b.RoomId == roomId
                           && b.StartDate <= date
                           && b.EndDate >= date
                           && b.IsActive);
            
            // Assert
            Assert.Empty(bookingForReturnedRoomId);
        }

        [Fact]
        public async Task FindAvailableRoom_CreateBooking_BookingSucess()
        {
            // Arrange
            DateTime dateStart = DateTime.Today.AddDays(5);
            DateTime dateEnd = DateTime.Today.AddDays(9);

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
            DateTime dateStart = DateTime.Today.AddDays(-2);
            DateTime dateEnd = DateTime.Today.AddDays(9);

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
            DateTime dateStart = DateTime.Today.AddDays(1);
            DateTime dateEnd = DateTime.Today.AddDays(1);

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
            DateTime dateStart = DateTime.Today.AddDays(22);
            DateTime dateEnd = DateTime.Today.AddDays(25);

            Booking newBooking = new()
            {
                Id = 3,
                CustomerId = 1,
                IsActive = true,
                StartDate = dateStart,
                EndDate = dateEnd,
                RoomId = 1,
            };

            Booking duplicateBooking = new()
            {
                Id = 4,
                CustomerId = 1,
                IsActive = true,
                StartDate = dateStart.AddDays(1),
                EndDate = dateEnd.AddDays(-1),
                RoomId = 1,
            };

            // Act
            await bookingManager.CreateBooking(newBooking);
            bool result = await bookingManager.CreateBooking(duplicateBooking);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task FindAvailableRoom_CreateBooking_BookingOverlapButFreeOnStartAndEndDate()
        {
            // Arrange
            DateTime dateStart = DateTime.Today.AddDays(25);
            DateTime dateEnd = DateTime.Today.AddDays(30);

            Booking newBooking = new()
            {
                Id = 3,
                CustomerId = 1,
                IsActive = true,
                StartDate = dateStart,
                EndDate = dateEnd,
                RoomId = 1,
            };

            Booking overlappingBooking = new()
            {
                Id = 4,
                CustomerId = 1,
                IsActive = true,
                StartDate = dateStart.AddDays(-1),
                EndDate = dateEnd.AddDays(1),
                RoomId = 1,
            };

            // Act
            await bookingManager.CreateBooking(newBooking);
            bool result = await bookingManager.CreateBooking(overlappingBooking);

            // Assert
            Assert.False(result);

        }

        [Fact]
        public async Task FindAvailableRoom_CreateBooking_OnePersonBooksMoreThan1RoomAtTheSameDate()
        {
            // Arrange
            DateTime dateStart = DateTime.Today.AddDays(3);
            DateTime dateEnd = DateTime.Today.AddDays(4);

            Booking newBooking = new()
            {
                Id = 3,
                CustomerId = 1,
                IsActive = true,
                StartDate = dateStart,
                EndDate = dateEnd,
                RoomId = 2,
            };

            Booking newBooking2 = new()
            {
                Id = 3,
                CustomerId = 1,
                IsActive = true,
                StartDate = dateStart,
                EndDate = dateEnd,
                RoomId = 3,
            };

            // Act
            await bookingManager.CreateBooking(newBooking);
            bool result = await bookingManager.CreateBooking(newBooking2);

            // Assert
            Assert.True(result);

        }

        [Fact]
        public async Task FindAvailableRoom_CreateBooking_BookingNotOverlapingAtStartButAtEnd()
        {
            // Arrange
            DateTime dateStart = DateTime.Today.AddDays(21);
            DateTime dateEnd = DateTime.Today.AddDays(29);

            Booking newBooking = new()
            {
                Id = 3,
                CustomerId = 1,
                IsActive = true,
                StartDate = dateStart,
                EndDate = dateEnd,
                RoomId = 2,
            };

            Booking newBooking2 = new()
            {
                Id = 3,
                CustomerId = 1,
                IsActive = true,
                StartDate = dateStart.AddDays(-1),
                EndDate = dateEnd.AddDays(-1),
                RoomId = 3,
            };

            // Act
            await bookingManager.CreateBooking(newBooking);
            bool result = await bookingManager.CreateBooking(newBooking2);

            // Assert
            Assert.False(result);

        }

        [Fact]
        public async Task FindAvailableRoom_CreateBooking_BookingOverlapingAtStartButNotAtEnd()
        {
            // Arrange
            DateTime dateStart = DateTime.Today.AddDays(21);
            DateTime dateEnd = DateTime.Today.AddDays(29);

            Booking newBooking = new()
            {
                Id = 3,
                CustomerId = 1,
                IsActive = true,
                StartDate = dateStart,
                EndDate = dateEnd,
                RoomId = 2,
            };

            Booking newBooking2 = new()
            {
                Id = 3,
                CustomerId = 1,
                IsActive = true,
                StartDate = dateStart.AddDays(1),
                EndDate = dateEnd.AddDays(1),
                RoomId = 3,
            };

            // Act
            await bookingManager.CreateBooking(newBooking);
            bool result = await bookingManager.CreateBooking(newBooking2);

            // Assert
            Assert.False(result);

        }

        [Fact]
        public async Task FindAvailableRoom_CreateBooking_CanBookRoomAfterEmpty()
        {
            // Arrange
            DateTime dateStart = DateTime.Today.AddDays(21);
            DateTime dateEnd = DateTime.Today.AddDays(29);

            Booking newBooking = new()
            {
                Id = 3,
                CustomerId = 1,
                IsActive = true,
                StartDate = dateStart,
                EndDate = dateEnd,
                RoomId = 2,
            };

            Booking newBooking2 = new()
            {
                Id = 3,
                CustomerId = 1,
                IsActive = true,
                StartDate = DateTime.Today.AddDays(30),
                EndDate = DateTime.Today.AddDays(35),
                RoomId = 3,
            };

            // Act
            await bookingManager.CreateBooking(newBooking);
            bool result = await bookingManager.CreateBooking(newBooking2);

            // Assert
            Assert.True(result);

        }

        [Fact]
        public async Task FindAvailableRoom_StartDateInPastEndDateInFuture_ThrowsArgumentException() {

            // Arrange
            DateTime date = DateTime.Today;

            // Act
            Task result() => bookingManager.FindAvailableRoom(date, date.AddDays(1));

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(result);

        }

        [Fact]
        public async Task FindAvailableRoom_StartDateLaterThanEndDate_ThrowsArgumentException() {

            // Arrange
            DateTime date = DateTime.Today;

            // Act
            Task result() => bookingManager.FindAvailableRoom(date, date.AddDays(-1));

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(result);

        }

        [Fact]
        public async Task GetFullyOccupiedDates_Start_ThrowsArgumentException() {

            // Arrange
            DateTime date = DateTime.Today;

            // Act
            List<DateTime> result = await bookingManager.GetFullyOccupiedDates(date, date);

            // Assert
            Assert.False(result.Any());

        }

        [Fact]
        public async Task GetFullyOccupiedDates_Bookings_NonEmptyList() {

            // Arrange
            DateTime startDate = DateTime.Today.AddDays(1);
            DateTime endDate = DateTime.Today.AddDays(3);
            
            // Fill up all rooms for the given dates
            Booking newBooking = new()
            {
                Id = 1,
                CustomerId = 1,
                IsActive = true,
                StartDate = startDate,
                EndDate = endDate,
                RoomId = 1,
            };
            await bookingManager.CreateBooking(newBooking);

            Booking newBooking2 = new()
            {
                Id = 2,
                CustomerId = 1,
                IsActive = true,
                StartDate = startDate,
                EndDate = endDate,
                RoomId = 2,
            };
            await bookingManager.CreateBooking(newBooking2);

            // Act
            List<DateTime> result = await bookingManager.GetFullyOccupiedDates(startDate, endDate);

            // Assert
            Assert.True(result.Any());

        }

        [Fact]
        public async Task GetFullyOccupiedDates_NoBookings_EmptyList() {

            // Arrange
            DateTime date = DateTime.Today;

            // Act
            List<DateTime> result = await bookingManager.GetFullyOccupiedDates(date, date.AddDays(3));

            // Assert
            Assert.False(result.Any());

        }

        
    }
}
