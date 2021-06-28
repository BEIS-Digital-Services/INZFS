Feature: Login
	As a user
	I want to login
	so that I can access the application


Scenario: Able to login with valid details
	Given I am on the login page
	When I submit valid login details
	Then I should be directed to the home page
	And I am able to access application page

Scenario: Unable to login with Invalid details
	Given I am on the login page
	When I submit invalid login details
	Then I should see relevant error message
	And I should remain on the Login page

@Smoke
Scenario: Validation Messages are shown correctly
	Given I am on the login page
	When I submit with the below combination of field values then I should see the FieldValidation and ValidationSummary accordingly
	| UsernameValue | PasswordValue | FieldValidation                                | ValidationSummary                              |
	| TestUser      |               | A Password is required.                        | A Password is required.                        |
	|               | TestPassword  | A Username is required.                        | A Username is required.                        |
	|               |               | A Username is required.A Password is required. | A Username is required.A Password is required. |