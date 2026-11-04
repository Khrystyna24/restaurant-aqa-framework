@api
Feature: Locations

  Each restaurant location has a description, a seating capacity and an average
  occupancy. A manager can update a location; a guest can look it up. Occupancy is
  shown as a percentage, so an occupancy of 70 reads back as "70%".

  Background:
    Given the restaurant has a waiter account
    And a restaurant location with:
      | detail            | value             |
      | description       | Riverside terrace |
      | capacity          | 80                |
      | average occupancy | 40                |

  Rule: A guest can look up a location

    @smoke
    Scenario: A guest sees the details the manager entered
      When a guest looks up the location
      Then the location shows:
        | detail            | value             |
        | description       | Riverside terrace |
        | average occupancy | 40%               |

    # Known defect (expected to fail until fixed): the capacity a manager enters is not kept — a
    # look-up reports 0, and once tables are added it reports their number instead.
    @known-defect
    Scenario: A guest sees the capacity the manager entered
      When a guest looks up the location
      Then the location shows:
        | detail   | value |
        | capacity | 80    |

  Rule: A manager can update a location

    Scenario Outline: A manager updates the average occupancy
      When the manager updates the location's average occupancy to <occupancy>%
      And a guest looks up the location
      Then the location shows an average occupancy of <occupancy>%

      Examples: Busy and quiet
        | occupancy |
        | 70        |
        | 5         |

    Scenario: A manager describes a location over several lines
      When the manager describes the location as:
        """
        Open daily from 9 to 23.
        Terrace seating available in summer.
        """
      And a guest looks up the location
      Then the location is described as:
        """
        Open daily from 9 to 23.
        Terrace seating available in summer.
        """

  Rule: A location's menu lists the dishes added to it

    Scenario: A dish the kitchen adds shows up on the location's menu
      When the manager adds a main course dish to the location
      And a guest opens the location's menu
      Then that dish appears in the results
