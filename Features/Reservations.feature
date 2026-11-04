@wip
Feature: Reservations

  Booking a table. The rules below come from the product's business rules but are not
  automated yet: their steps have no implementation, so they show up as not-yet-run
  rather than as passes or failures. Each one is a ready-made specification for the
  next piece of work.

  Background:
    Given a customer who is signed in
    And a table that seats 4 guests

  Rule: A booking must make sense

    @api
    Scenario: A customer cannot book for more guests than the table seats
      When the customer books the table for 5 guests
      Then the customer is told the table is too small

    @api
    Scenario Outline: A booking must start far enough ahead
      When the customer books the table to start in <minutes> minutes
      Then the booking is <outcome>

      Examples: Around the 15 minute notice period
        | minutes | outcome  |
        | 5       | refused  |
        | 14      | refused  |
        | 16      | accepted |

    @api
    Scenario: Two bookings cannot overlap on the same table
      Given the customer has booked the table for tomorrow evening
      When another customer books the same table for the same time
      Then the second booking is refused
