@GroupPreOrder @Level3
Feature: Removing a participant from a group pre-order
  As a booking Owner
  I want to remove a participant from my group
  So that the list and the total are recalculated and only the remaining dishes are submitted

  Background:
    Given the Owner has a group reservation
    And the guest "Anna" joined and submitted a cart with 2 x dish 1
    And the guest "Bohdan" joined with an unsubmitted cart with 1 x dish 2

  @UI
  Scenario: Owner removes one participant out of several
    When the Owner opens the group order page
    And the Owner removes "Bohdan" from the group
    Then "Bohdan" no longer appears in the participants list
    And "Anna" still appears in the participants list
    And the grand total equals the sum of the remaining carts

  @UI
  Scenario: Owner removes a participant who already submitted the cart
    When the Owner opens the group order page
    And the Owner removes "Anna" from the group
    Then "Anna" no longer appears in the participants list
    And "Bohdan" still appears in the participants list
    And the grand total equals the sum of the remaining carts

  @API
  Scenario: Owner removes every guest
    When the Owner removes "Anna" via API
    And the Owner removes "Bohdan" via API
    Then no guests remain in the group and the total is 0

  @API @Negative
  Scenario: A participant of another group cannot be removed
    Given a second independent group with the guest "Dmytro"
    When the Owner removes "Dmytro" through the first group via API
    Then the API returns 400
    And "Dmytro" is still in the second group

  @API @Negative
  Scenario: A participant that does not exist
    When the Owner removes a non-existent participant via API
    Then the API returns 404

  @API @Negative
  Scenario Outline: Only the Owner can remove a participant
    When <actor> tries to remove "Anna" via API
    Then the API returns <status>
    And "Anna" is still in the group

    Examples:
      | actor                   | status |
      | a guest of this group   | 401    |
      | another customer        | 403    |
      | an unauthenticated user | 401    |

  @UI @E2E
  Scenario: Remove a guest, submit, and the waiter sees the order without the removed guest
    Given the guest "Bohdan" submitted the cart
    When the Owner opens the group order page
    And the Owner removes "Bohdan" from the group
    And the Owner submits the consolidated pre-order
    Then the page shows "Sent to Restaurant"
    And the group status is "Submitted"
    And the waiter sees the order with dish 1 but without dish 2
