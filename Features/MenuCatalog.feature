Feature: Menu

  What a guest can do with the restaurant's menu: browse by category, sort, search,
  and open a dish — plus a mixed proof that a dish the kitchen adds also shows up on
  the storefront. The restaurant's menu is shared and changes over time, so scenarios
  that depend on what happens to be on it are tagged @data-dependent and check that
  "all the dishes shown" fit, rather than counting them.

  Background:
    Given the restaurant has a location

  Rule: Guests can browse the menu by category

    @api
    Scenario Outline: Browsing a category only shows dishes of that category
      When a guest browses <category> dishes
      Then the menu is shown
      And all the dishes shown are <category> dishes

      @smoke
      Examples: A category that is always stocked
        | category    |
        | main course |

      @data-dependent
      Examples: Categories the menu may not stock at the moment
        | category  |
        | appetizer |
        | dessert   |

  Rule: Guests can sort the menu

    @api
    Scenario Outline: Sorting by price puts the dishes in price order
      When a guest sorts the menu by <sorting>
      Then the menu is shown
      And the dishes are listed <order>

      Examples: Both directions
        | sorting              | order                          |
        | price, lowest first  | from cheapest to most expensive |
        | price, highest first | from most expensive to cheapest |

    @api
    Scenario Outline: Sorting by popularity is available
      When a guest sorts the menu by <sorting>
      Then the menu is shown

      Examples: Both directions
        | sorting                         |
        | popularity, least popular first |
        | popularity, most popular first  |

    @api @data-dependent
    Scenario: A category and a sort order can be combined
      When a guest browses main course dishes sorted by price, highest first
      Then all the dishes shown are main course dishes
      And the dishes are listed from most expensive to cheapest

  Rule: Guests are told when something does not exist

    @api @negative
    Scenario Outline: Opening something that does not exist
      When a guest opens a <item> that does not exist
      Then the guest is told that the <item> could not be found

      @dishes
      Examples: A dish
        | item |
        | dish |

      @locations
      Examples: A location
        | item     |
        | location |

  Rule: A dish the kitchen adds shows up for guests

    Background:
      Given the kitchen has added a main course dish

    @api
    Scenario: A guest can find a new dish by its name
      When a guest searches the menu for that dish by name
      Then that dish appears in the results

    @api
    Scenario: A guest can open a new dish
      When a guest opens that dish
      Then the dish is shown with:
        | detail | value       |
        | type   | Main course |
        | status | Available   |

    @ui
    Scenario: A new dish is visible on the storefront menu
      When a guest views the menu
      Then that dish is visible on the menu
