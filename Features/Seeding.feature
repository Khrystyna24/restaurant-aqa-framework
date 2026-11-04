@api
Feature: Restaurant setup

  A manager sets up the restaurant: a location, the dishes on its menu and the tables
  guests can book. Everything a scenario creates is removed again afterwards, and is
  recognisable as test data by its "AQA-Sample-" prefix in case a run is interrupted.

  Background:
    Given the restaurant has a waiter account

  Rule: A manager can set up everything a restaurant needs

    @smoke
    Scenario: A manager sets up a location with a dish and a table
      When the manager sets up a new location
      Then the location is created
      When the manager adds a dish to the location
      Then the dish is created
      When the manager adds a table to the location
      Then the table is created

  Rule: A manager cannot set up a location with invalid details

    # Known defects (expected to fail until fixed): the backend accepts both of these.
    @negative @validation @known-defect
    Scenario Outline: An invalid location is not created
      When the manager tries to set up a location with <problem>
      Then the location is not created

      Examples: Details a location must have
        | problem           |
        | an empty address  |
        | a negative capacity |

  Rule: Dishes and tables keep the details they were created with

    Background:
      Given the manager has set up a new location

    Scenario Outline: A dish is listed in the category it was added to
      When the manager adds a <category> dish to the location
      Then the dish is created
      And the dish is listed in the <category> category

      Examples: Every category
        | category    |
        | appetizer   |
        | main course |
        | dessert     |

    Scenario Outline: A table seats the number of guests it was created for
      When the manager adds a table for <guests> guests to the location
      Then the table is created
      And the table seats <guests> guests

      @small
      Examples: Small tables
        | guests |
        | 2      |
        | 4      |

      @large
      Examples: Large tables
        | guests |
        | 8      |
