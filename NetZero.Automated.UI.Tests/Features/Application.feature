Feature: Application

Background: Register and Login 
    Given I have registered a new user
    And I have logged in with the new user details
	And I am on the Application Summary page


Scenario: Able to fill in Basic Information Section
    When I enter all the basic information details and save    
    And I navigate back to the application summary page
    Then I should see status next to each of the basic information section link as per below
    | Section         | Status    |
    | Company Details | COMPLETED |
    | Project Summary | COMPLETED |
    | Project Details | COMPLETED |
    | Funding         | COMPLETED |
    Then upon visting each basic information section I should be able to see that the details are populated


Scenario: Able to edit Basic Information from summary page
    Given I enter all the basic information details and save 
    And I navigate back to the application summary page
    When I update basic info details on each section
    And I navigate back to the application summary page
    Then upon visting each basic information section I should be able to see that the details are updated

Scenario: Able to see validation on the Basic Information input section
    Given I am on the first basic info section page
    When I click continue without filling any details on the Company details page
    Then I should see the validations "Please enter Comapany Name, Please enter Company Number"
    And I enter valid company details and continue
    Then I click continue without filling any details on the Project Summary page
    Then I should see the validations "Enter the project name, Enter estimated start date."
    And I enter valid project introduction input and continue
    And I click continue without filling any details on the Project Details page
    Then I should see the validations "Please Select Yes or No, Enter brief summary of the project"
    And I enter valid project details and continue
    And I click continue without slecting any of the funding options
    Then I should see the validations "Please select funding options."

Scenario: Able to upload a documents
    Given I am on upload project plan page
    When I upload a document
    Then I should see that the document is uploaded

 Scenario: Able to download an uploaded documents
    Given I am on upload project plan page
    When I upload a document
    And I click on the download document link
    Then I should see that the document is downloaded