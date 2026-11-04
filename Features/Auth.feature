Feature: Signing in

  Staff sign in with the credentials they were given. A waiter account cannot be
  created by signing up — everyone who registers becomes a customer — so the
  restaurant's waiter account is provided to the test environment up front.

  Background:
    Given the restaurant has a waiter account

  Rule: A waiter can sign in with the correct password

    @api @smoke
    Scenario: A waiter signs in
      When the waiter signs in with the correct password
      Then the waiter is signed in

    @ui @smoke
    Scenario: A waiter signs in on the storefront
      When the waiter signs in with the correct password
      Then the waiter is signed in

  Rule: Wrong credentials are refused

    @api @negative
    Scenario: A waiter who gets the password wrong is refused
      When the waiter signs in with a wrong password
      Then sign-in is refused
      And the visitor is told "Invalid email or password"

    # Known defect (expected to fail until fixed): someone who never registered gets a server
    # error instead of the same refusal a wrong password gets.
    @api @negative @known-defect
    Scenario: Someone who never registered is refused the same way
      When someone who never registered tries to sign in
      Then sign-in is refused

  Rule: Invalid sign-in details are rejected with an explanation

    @api @negative @validation
    Scenario Outline: Someone cannot sign in with an invalid detail
      When someone tries to sign in with an invalid <field> (<problem>)
      Then the visitor is told "<message>"

      @email
      Examples: Email address problems
        | field         | problem           | message                                                                         |
        | email address | without an @ sign | Invalid email address. Please ensure it follows the format: username@domain.com |
        | email address | without a domain  | Invalid email address. Please ensure it follows the format: username@domain.com |
        | email address | empty             | Email must be a valid email address and cannot exceed 254 characters.           |
        | email address | too long          | Email must be a valid email address and cannot exceed 254 characters.           |

      @password
      Examples: Password problems
        | field    | problem | message                                                       |
        | password | empty   | Password is required. Please enter your password to continue. |
