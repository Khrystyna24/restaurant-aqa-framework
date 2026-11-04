@smoke
Feature: Smoke

  A guest can see what the restaurant offers and reach its storefront. Run these
  first: if they fail, nothing else is worth running.

  Rule: Guests can see the restaurant's menu information

    @api
    Scenario Outline: The menu information is available
      When a guest asks for the <information>
      Then the information is available

      @catalog
      Examples: Dishes
        | information    |
        | dish catalog   |
        | popular dishes |

      @locations
      Examples: Locations
        | information       |
        | list of locations |

    @api
    Scenario: Every dish in the catalog is complete
      When a guest asks for the dish catalog
      Then every dish has a name, a price and a type

  Rule: The storefront is online

    @ui
    Scenario: A guest can open the home page
      When a guest opens the home page
      Then the home page is shown
