@ui
Feature: Reports

  The reporting dashboard is for admins only. This environment has no admin account
  yet, so the admin scenario is skipped (not failed) until ADMIN_USERNAME and
  ADMIN_PASSWORD are provided.

  Rule: Only admins can open the reports dashboard

    @smoke
    Scenario: A guest who is not signed in cannot open the reports dashboard
      When a guest opens the reports dashboard
      Then the reports dashboard is not shown

    @requires-admin
    Scenario: An admin can open the reports dashboard
      Given the restaurant has an admin account
      When the admin signs in
      And the admin opens the reports dashboard
      Then the reports dashboard is shown
