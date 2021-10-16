# pcs-dbview-api
REST API for the Project Completion System (ProCoSys (PCS)) using ASP.NET 5

## Notes

### Secrets
Before running the application, some settings need to be set. These are defined in appsettings.json. To avoid the possibility to commit secrets, move parts of the configuration to the secrets.json file on your computer.
Typical settings that should be moved to secrets.json are:
* AD IDs
* Keys
* Local URLs
* Other secrets

### Environment
Set the runtime environment by setting the environment variable ASPNETCORE_ENVIRONMENT on your machine to one of the following:
* Development
* Test
* Production

If this variable is not set, it will default to Production.
