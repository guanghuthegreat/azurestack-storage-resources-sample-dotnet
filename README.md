---
services: azure-resource-manager, azure-storeage
platforms: dotnet
author: guanghu
---

# Manage Azure Stack resource and storage with .Net

This sample explains how to manage your resources and storage services in Azure Stack using the Azure .NET SDK. 

**On this page**
- [Run this sample](#run)
- [What is program.cs doing?](#example)

<a id="run"></a>
## Run this sample
1. If you don't have it, install the [.NET Core SDK](https://www.microsoft.com/net/core).
1. Clone the repository.
    ```
    git clone https://github.com/guanghuthegreat/azurestack-storage-resources-sample-dotnet.git
    ```
1. Install the dependencies.

    ```
    dotnet restore
    ```
1. Create an Azure service principal either through
    [Azure CLI](https://azure.microsoft.com/documentation/articles/resource-group-authenticate-service-principal-cli/),
    [PowerShell](https://azure.microsoft.com/documentation/articles/resource-group-authenticate-service-principal/)
    or [the portal](https://azure.microsoft.com/documentation/articles/resource-group-create-service-principal-portal/).
   In Azure Stack give Contributor permissions to the subscription where the resources are stored. 
1. Obtain the management endpoint URI for Azure Stack instance you are targeting from the service administrator. For example, if you are running your own ASDK deployment the URI will be in the form https://management.local.azurestack.external 

1. Find the AAD resource ID of the Azure Stack instance you are targeting. For example, if you are running your own ASDK deployment, you issue a request to https://management.local.azurestack.external/metadata/endpoints?api-version=2015-01-01. Then copy the value of the first element of the audiences property in the JSON response. 

1. Export these environment variables using your subscription id and the tenant id, client id and client secret from the service principle that you created. 

    ```
    Set AZS_ACTTIVEDIRECTORY={the AAD login URI}
    Set AZS_ACTTIVEDIRECTORYRESOURCEID={the value of audience URI retrieved in previous step}
    Set AZS_MANAGEMENTENDPOINT={the management endpoint URI}
    Set AZS_STORAGENDPOINT={the storage endpoint URI}
    Set AZS_SUBID={your subscription id}
    Set AZS_TENANTID={your tenant id}
    Set AZS_CLIENTID={your client id}
    Set AZS_SECURITYKEY={your client securit}
    Set AZS_LOCATION={the location (region) of your Azure Stack deployment}
    ```

1. Run the sample.

    ```
    dotnet run
    ```


