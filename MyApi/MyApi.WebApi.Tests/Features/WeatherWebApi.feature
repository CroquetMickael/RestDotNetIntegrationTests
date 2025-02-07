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

Scenario: Save weather forecast
	Given the weather forecast
	| Date       | TemperatureC | Summary |
	| 2023-01-05 | 5            | Bracing |
	When I save it
	Then the response status code is '200'

Scenario: Get weather forecast for one date with existing forecast
	When I make a GET request to 'weatherforecast/2023-01-02'
	Then the response status code is '200'
	And the response is
	| Date       | TemperatureC | Summary |
	| 2023-01-02 | 2            | Bracing |
