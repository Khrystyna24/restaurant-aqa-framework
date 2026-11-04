@api
Feature: Location menus

  Every location has its own menu: a dish the kitchen adds belongs to the location it was added to and to no other.

  Background:
    Given the restaurant has a waiter account

  Rule: A dish belongs only to the location it was added to

    @smoke
    Scenario: A dish shows up on its own location's menu and not on another's
      Given a restaurant location called "Riverside"
      And a restaurant location called "Hilltop"
      When the manager adds a main course dish to the "Riverside" location
      And a guest opens the menu of the "Riverside" location
      Then that dish appears in the results
      When a guest opens the menu of the "Hilltop" location
      Then that dish does not appear in the results
