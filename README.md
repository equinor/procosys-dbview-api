# pcs-dbview-api
REST API for the Project Completion System (ProCoSys (PCS)) using ASP.NET 5

## Notes
The API has endpoints for extracting big data from PCS in a M2M dataflow.

### Secrets
Before running the application, some settings need to be set. These are defined in appsettings.json. 
To avoid the possibility to commit secrets to code repo, move parts of the configuration to your UserSecret file (secrets.json) file on your computer and/or to Azure App Configuration and Azure Keyvaults.
Typical settings that should not be commited are:
* AD IDs
* Keys
* URLs
* Other secrets
To use local settings on developers sandbox: Set UseAzureAppConfiguration to false, and fill UserSecret file with necessary empty values in appsettings.json
To use settings in an Azure App Configuration (and Azure Keyvault): Set UseAzureAppConfiguration to true, and set ConnectionStrings:AppConfig in UserSecret file to a valid Azure App Configuration connectionstring.

### Environment
Set the runtime environment by setting the environment variable ASPNETCORE_ENVIRONMENT on your machine to one of the following:
* Development
* Test
* Production

If this variable is not set, it will default to Production.
