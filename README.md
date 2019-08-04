# Sample Url Shortener

This is a sample Url Shortener service.

It contains:
- a REST API allowing clients to create new shortened urls, as well as retrieve the details of existing shortened urls.
- a Swagger UI endpoint documenting the API and allowing users to try it out.
- a (very basic) React UI allowing users to create new shortened urls.
- a url redirection service to redirect shortened urls to their original address.
- a persistent data store, backed by Azure Cosmos DB.
- a build and deployment pipeline in Azure DevOps, including tests, code coverage and automatic deployment to Azure App Services.

Deployed instance:
https://smallerurl.azurewebsites.net/

Source Code:
https://github.com/perrymana/url-shortener

Azure Pipeline:
https://dev.azure.com/ashleyperryman/url-shortener/_build