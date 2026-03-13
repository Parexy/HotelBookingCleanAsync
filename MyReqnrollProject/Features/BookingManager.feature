Feature: BookingManager

Scenario: Create a booking for hotel room
	Given a room is available
	When i create a booking
    Then the booking should be successful

Scenario: Create a booking for hotel room that is not available
	Given a room is not available
	When i create a booking
    Then the booking should be unsuccessful

Scenario: Create a booking when there are no existing bookings
    Given there are no existing bookings
    And i want to create a booking from day 12 to day 14
    When i create a booking with these dates
    Then the booking should be successful

Scenario: Create a booking where enddate is before startdate
    Given a room is available
    And i want to book a room where startdate is day 2 and enddate is day 1
    When i create a booking
    Then the booking should be unsuccessful

Scenario: Create a booking where enddate is the same as startdate
    Given a room is available
    And i want to book a room where startdate is day 1 and enddate is day 1
    When i create a booking
    Then the booking should be successful

Scenario: Create a booking where startdate is in the past
    Given a room is available
    And i want to book a room where startdate is day -1 and enddate is day 1
    When i create a booking
    Then the booking should throw an exception

Scenario: Create a booking where enddate is in the past
    Given a room is available
    And i want to book a room where startdate is day 1 and enddate is day -1
    When i create a booking
    Then the booking should throw an exception