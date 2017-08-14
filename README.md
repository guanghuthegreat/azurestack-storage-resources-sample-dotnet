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
    - [create a resource group](#create-rg)
    - [create a storage account](#create-sa)
    - [get the access keys of storage account](#get-sa-keys)
    - [get a list of storage accounts within a resource group](#list-sa-rg)
    - [get all the storage accounts for a given subscription](#list-sa-sub)
    - [create a new blob container](#create-blob)
    - [upload a blob](#upload)
    - [download a blob](#download)
    - [delete a storage account](#delete-sa)
    - [delete a resource group](#delete-rg)

<a id="run"></a>
## Run this sample
### Prerequisits
1. This sample requires to be run either from the [Azure Stack Development Kit(ASDK)](https://docs.microsoft.com/en-us/azure/azure-stack/azure-stack-connect-azure-stack#connect-with-remote-desktop) or from an external client if you are [connected through VPN](https://docs.microsoft.com/en-us/azure/azure-stack/azure-stack-connect-azure-stack#connect-with-vpn).
1. If you don't have it, install the [.NET Core SDK](https://www.microsoft.com/net/core).
1. Recommand to [Install and configure CLI for use with Azure Stack](https://docs.microsoft.com/en-us/azure/azure-stack/azure-stack-connect-cli)
### Steps
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
   An quick way is to run the following Azure CLI cmdlet on Azure Stack and record the **appId** and **password** as client id and client secret key. 
   ```
   az ad sp create-for-rbac -n {choose a name for sp}
   ```
1. Obtain the value for your account subscription ID and tenant ID. You can find these info by running CLI cmdlet `az account show` on the target Azure Stack and find out these value in the fields **id** and **tenantId** from the cmdlet outputs 
1. Obtain the URI for AAD login, AAD resource ID and endpoints for management, storage. You can easily find out these info by running CLI cmdlet `az cloud show` on the target Azure Stack and find out these value in the fields of **activeDirectory**, **activeDirectoryResourceId**, **management** and **storageEndpoint** from this cmdlet outputs. 
1. Export these environment variables and fill in the values you created in previous steps.  
    ```
    Set AZS_ACTTIVEDIRECTORY={the AAD login URI}
    Set AZS_ACTTIVEDIRECTORYRESOURCEID={the value of audience URI retrieved in previous step}
    Set AZS_MANAGEMENTENDPOINT={the management endpoint URI}
    Set AZS_STORAGENDPOINT={the storage endpoint URI}
    Set AZS_SUBID={your subscription id}
    Set AZS_TENANTID={your tenant id}
    Set AZS_CLIENTID={your client id}
    Set AZS_SECRETKEY={your client secret key}
    Set AZS_LOCATION={the location (region) of your Azure Stack deployment, like 'local' in a ASDK deployments}
    ```
1. Run the sample.
    ```
    dotnet run
    ```
<a id="example"></a>
## What is program.cs doing? 
The sample walks you through several resrouce group and storage services operations. It starts by setting up a ResourceManagementClient and StorageManagementClient objects using your subscription and credentials. 
#### get the authentication token after login:
```
ActiveDirectoryServiceSettings s = new ActiveDirectoryServiceSettings();
s.AuthenticationEndpoint = new Uri(AzS_ActiveDirectory);
s.TokenAudience = new Uri(AzS_ActiveDirectoryResourceID);
s.ValidateAuthority = true;
var serviceCreds = await ApplicationTokenProvider.LoginSilentAsync(AzS_TenantID, AzS_ClientID, AzS_SecretKey, s);
```
#### set up the ResourceManagementClient object
```
var resourceClient = new ResourceManagementClient(creds)
{
    BaseUri = new Uri(AzS_ManagementEndPoint),
    SubscriptionId = AzS_SubscriptionID
};
```
#### set up the StorageManagementClient object 
```            
var storageClient = new StorageManagementClient(creds)
{
    BaseUri = new Uri(AzS_ManagementEndPoint),
    SubscriptionId = AzS_SubscriptionID
};
```
<a id="create-rg"></a>
### create a resource group 
```
var resourceGroup = resourcesClient.ResourceGroups.CreateOrUpdate(
    rgname,
    new ResourceGroup
    {
        Location = AzS_Location
    });
```
<a id="create-sa"></a>
### create a storage account 
```
// set default parameters for storage account in Azure Stack 
public static Microsoft.Azure.Management.Storage.Models.Sku DefaultSku = new Microsoft.Azure.Management.Storage.Models.Sku(SkuName.StandardLRS);
public static Kind DefaultStorageKind = Kind.Storage;
public static Dictionary<string, string> DefaultTags = new Dictionary<string, string>
{
    {"key1","value1"},
    {"key2","value2"}
};
```
```
// create storage accounts 
StorageAccountCreateParameters parameters = new StorageAccountCreateParameters
{
    Location = AzS_Location, 
    Kind = DefaultStorageKind, 
    Tags = DefaultTags, 
    Sku = DefaultSku 
};
var storageAccount = storageMgmtClient.StorageAccounts.Create(rgname, acctName, parameters);
```
<a id="get-sa-keys"></a>
### get the access keys for a storage account 
```
//Get the storage account keys for a given account and resource group
IList<StorageAccountKey> acctKeys = storage.StorageAccounts.ListKeys(rgName, storageAccountName1).Keys;
```
<a id="list-sa-rg"></a>
### get a list of storage accounts within a resource group 
```
//Get a list of storage accounts within a specific resource group
IEnumerable<StorageAccount> storAccts = storage.StorageAccounts.ListByResourceGroup(rgName);
```
<a id="list-sa-sub"></a>
### get all the storage accounts for a given subscription 
```
//Get all the storage accounts for a given subscription
IEnumerable<StorageAccount> storAcctsSub = storage.StorageAccounts.List();
```
<a id="create-blob"></a>
### create a new blob container
```
// set up the cloudblobclient object
StorageCredentials cre = new StorageCredentials(accountName, key);
CloudStorageAccount storageAccount = new CloudStorageAccount(cre, AzS_StorageEndPoint, true);
CloudBlobClient blob = storageAccount.CreateCloudBlobClient();
// create a blob container fro a given container name
CloudBlobContainer blobContainer = blob.GetContainerReference(blobcontainerName);
blobContainer.CreateIfNotExists();
```
<a id="upload"></a>
### upload a blob 
```
CloudBlockBlob bb = blobContainer.GetBlockBlobReference(blobname);
/* steps of setting up the bytes array buffer */ 
using (MemoryStream stream = new MemoryStream(buffer))
{
    bb.UploadFromStream(stream,size);
}
```
<a id="download"></a>
### download a blob 
```
using (MemoryStream outputStream = new MemoryStream())
{
    bb.DownloadToStream(outputStream, null, null); 
}
```
<a id="delete-sa"></a>
### delete a storage account 
```
storageMgmtClient.StorageAccounts.Delete(rgname, acctName);

```
<a id="delete-rg"></a>
### delete a resource group 
```
resourceClient.ResourceGroups.Delete(rgName);

```
