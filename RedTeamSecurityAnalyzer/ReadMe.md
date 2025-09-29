# RedTeam Security Analyzer

A comprehensive tool for analyzing and enhancing the security of red team applications. This tool provides various functionalities to assess vulnerabilities, simulate attacks, and generate detailed reports.


## Commands

### analyze
 Perform a security analysis on a specified application.  
 **Usage:** `analyze <application_name> [options]`

 `application_name`: go, flex, lens

 `[options]`: --username <username> --password <password> -  only supply a user/password if the pages being analyzed require authentication.


 * Note - if you are running this from a local build, you can set dev secrets instead of passing credentials on the command line.  
 ``` json
 {
  "username": "user",
  "password": "password"
}
 ```
## Configuration

All configuration can be found in /Rules/RuleDefinitions.json

This files contains test cases for each of the WAF rules defined in AWS and the ability to map them to a specific RedTeam application for analysis.

## Example

``` json
{
   "Applications" : {
		"Go": {
			Tests: [
		         "Name": "Go Path Traversal - Forms",
                 "Runner": "FormData",
                 "Enabled": true,
                 "BaseUrl": "https://cfdev.go.redteam.com/uatcode/inprogress/createrfp.cfm?bidid=1356&rfptype=L",
                 "Rules": [ "FormData" ],
                 "RequiresAuthentication": true,
                 "Properties": {
                   "FormSelector": "form[id='rfpadd']",
                   "FormName": "createrfp.cfm",
                   "FormSubmitButton": "input[name='saverfp']",
                   "FormKeys": {
                     "input[name='bid_id']": "",
                     "input[name='project_id']": "",
                     "input[name='rfp_description']": ""
                   }
                }
			]
		}
   },
   Rules: [
      {
      "ID": 1,
      "RequiresAuthentication": false,
      "Name": "FormData",
      "Description": "Basic Path Traversal attempts using ../ and ..\\ sequences.",
      "TestData": [
        {
          "Pattern": "../",
          "Description": "",
          "SuccessStatusCodes": [ 403 ],
          "FailureStatusCodes": [ 200, 500 ]
        },
        {
          "Pattern": "..\\",
          "Description": "",
          "SuccessStatusCodes": [ 403 ],
          "FailureStatusCodes": [ 200, 500 ]
        }
      }
   ]
}
```

This block of JSON defines a test case for the Go application that checks for path traversal vulnerabilities in form submissions.
The test will attempt to submit form data with path traversal patterns and check the response status codes to determine if the vulnerability exists.

Test runner abstracations exists to handle different types of tests, such as FormData, URL parameters, and JSON payloads.  They are defined as keyed dependencies referenced by a string.  In this case, the "FormData" runner is used to execute this test.

Authentication is provided by an ILoginForm implementation in conjunction with the form values provided in the Properties dictionary.  
Once we migrate to unified login, this will not be needed.

ILoginForms use a keyed dependency based on the application name.