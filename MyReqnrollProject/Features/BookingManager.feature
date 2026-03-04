Feature: BookingManager

@mytag
Scenario: Create a booking for hotel room
	Given a room is available
	When i create a booking
    Then the booking should be successful

@mytag
Scenario: Create a booking for hotel room that is not available
	Given a room is not available
	When i create a booking
    Then the booking should be unsuccessful