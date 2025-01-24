Feature: WeatherWebApi

Web API for weather forecasts

Background:
    Given the existing forecast are
        | Date       | Summary  | TemperatureC |
        | 2023-01-01 | Freezing | -7           |
        | 2023-01-02 | Bracing  | 2            |
        | 2023-05-03 | Chilly   | 17           |

Scenario: Get weather forecasts
    When I make a GET request to 'weatherforecast'
    Then the response status code is '200'


Scenario: Get weather forecast for one date with no forecast
    When I make a GET request to 'weatherforecast/2020-01-01'
    Then the response status code is '204'

Scenario: Get weather forecast for one date with existing forecast
    When I make a GET request to 'weatherforecast/2023-01-02'
    Then the response status code is '200'
    And the response is
        | Date       | TemperatureC | Summary |
        | 2023-01-02 | 2            | Bracing |

Scenario: Get weather forecast for 7 days
    #Given The days are:
    #    | Time       |
    #    | 2025-01-06 |
    #    | 2025-01-07 |
    #    | 2025-01-08 |
    #    | 2025-01-09 |
    #    | 2025-01-10 |
    #    | 2025-01-11 |
    #    | 2025-01-12 |
    #Given The minimal temperatures are:
    #    | Min  |
    #    | 23.0 |
    #    | 24.3 |
    #    | 24.1 |
    #    | 23.9 |
    #    | 23.5 |
    #    | 23.5 |
    #    | 23.3 |
    #Given The maximum temperatures are:
    #    | Max  |
    #    | 25.0 |
    #    | 25.2 |
    #    | 25.0 |
    #    | 24.5 |
    #    | 24.6 |
    #    | 24.5 |
    #    | 24.8 |
    Given The external service forecast respond
    When I make a GET request to 'weatherforecast/sevendayminmax' for the adress: PL Charles Valentin 59140 Dunkerque, 59140 Dunkerque
    Then the response status code is '200'
    And the response with longitude '52.2' and latitude '14.2'
        | date       | min  | max  |
        | 2025-01-06 | 23   | 25   |
        | 2025-01-07 | 24,3 | 25,2 |
        | 2025-01-08 | 24,1 | 25   |
        | 2025-01-09 | 23,9 | 24,5 |
        | 2025-01-10 | 23,5 | 24,6 |
        | 2025-01-11 | 23,5 | 24,5 |
        | 2025-01-12 | 23,3 | 24,8 |
