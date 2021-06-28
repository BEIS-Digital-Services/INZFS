Feature: UserRegistration
	As a user
	I want to register
	so that I can login and apply for funds

@UserCleanUp
Scenario: Able to register with valid details
	Given I am on the Registration page
	When I provide valid registration details
	And I submit the registration
	#Then I am able to see the confirmation message
	Then I should be directed to the home page
	Then the user is created


@Smoke
Scenario: Validation messages when fields are left blank
	Given I am on the Registration page
	When I click on the submit button
	Then I should see validation messages accordingly

@UserCleanUp
Scenario: Exisiting user invalid password and email validations
	Given I am on the Registration page
	When I submit with the below Fields and its InvalidValue then I should see the FieldValidation and ValidationSummary accordingly
	| Field    | InvalidValue                 | FieldValidation                                                 | ValidationSummary                                                                                                                                                      |
	| Password | abc                          | Passwords must be at least 7 characters                         | Passwords must be at least 7 characters.Passwords must have at least one uppercase character ('A'-'Z').                                                                |
	| Password | 123                          | Passwords must be at least 7 characters                         | Passwords must be at least 7 characters.Passwords must have at least one lowercase character ('a'-'z').Passwords must have at least one uppercase character ('A'-'Z'). |
	| Password | 12345678                     | Passwords must have at least one lowercase character ('a'-'z'). | Passwords must have at least one lowercase character ('a'-'z').Passwords must have at least one uppercase character ('A'-'Z').                                         |
	| Password | ABCDEFGH                     | Passwords must have at least one lowercase character ('a'-'z'). | Passwords must have at least one lowercase character ('a'-'z').                                                                                                        |
	| Password | abcdefgh                     | Passwords must have at least one uppercase character ('A'-'Z'). | Passwords must have at least one uppercase character ('A'-'Z').                                                                                                        |
	| Email    | test@                        | Invalid Email.                                                  | Invalid Email.                                                                                                                                                         |
	| UserName | TestUserDoNotDelete          | User name 'TestUserDoNotDelete' is already used.                | User name 'TestUserDoNotDelete' is already used.                                                                                                                       |
	| Email    | TestUserDoNotDelete@Test.com | A user with the same email already exists.                      | A user with the same email already exists.                                                                                                                             |
	When I provide valid registration details
	And I submit with valid but different passwords
	Then I should see password mismatch validation

