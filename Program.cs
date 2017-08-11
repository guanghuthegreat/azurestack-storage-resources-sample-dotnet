using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Azure.Management.Storage;
using Microsoft.Azure.Management.Storage.Models;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Models;
using Microsoft.Azure.KeyVault;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;

namespace AzureStackStorage
{
    class Program
    {
        public static string AzS_ActiveDirectory; 
        public static string AzS_ActiveDirectoryResourceID; 
        public static string AzS_ManagementEndPoint;
        public static string AzS_StorageEndPoint;
        public static string AzS_SubscriptionID;
        public static string AzS_TenantID;
        public static string AzS_ClientID;
        public static string AzS_SecurityKey;
        public static string AzS_Location;
        public static Microsoft.Azure.Management.Storage.Models.Sku DefaultSku = new Microsoft.Azure.Management.Storage.Models.Sku(SkuName.StandardLRS);
        public static Kind DefaultStorageKind = Kind.Storage;
        public static Dictionary<string, string> DefaultTags = new Dictionary<string, string>
        {
            {"key1","value1"},
            {"key2","value2"}
        };

        public static void Main(string[] args)
        {
            try
            {
                // These values are used by the sample to connect to and authenticate on Azure Stack. Please set up Environemtn values according to the help instructions. 

                AzS_ActiveDirectory = Environment.GetEnvironmentVariable("AZS_ACTTIVEDIRECTORY");
                AzS_ActiveDirectoryResourceID = Environment.GetEnvironmentVariable("AZS_ACTTIVEDIRECTORYRESOURCEID");
                AzS_ManagementEndPoint = Environment.GetEnvironmentVariable("AZS_MANAGEMENTENDPOINT");
                AzS_StorageEndPoint = Environment.GetEnvironmentVariable("AZS_STORAGENDPOINT");
                AzS_SubscriptionID = Environment.GetEnvironmentVariable("AZS_SUBID");
                AzS_TenantID = Environment.GetEnvironmentVariable("AZS_TENANTID");
                AzS_ClientID = Environment.GetEnvironmentVariable("AZS_CLIENTID");
                AzS_SecurityKey = Environment.GetEnvironmentVariable("AZS_SECURITYKEY");
                AzS_Location = Environment.GetEnvironmentVariable("AZS_LOCATION");

                var templist = new List<string>
                    {AzS_ActiveDirectory,
                    AzS_ActiveDirectoryResourceID,
                    AzS_ClientID,
                    AzS_Location,
                    AzS_ManagementEndPoint,
                    AzS_SecurityKey, 
                    AzS_StorageEndPoint, 
                    AzS_SubscriptionID, 
                    AzS_TenantID};

                if(templist.Any(i => string.IsNullOrEmpty(i)))
                {
                    Console.WriteLine("Please provide Environment Vars for:");
                    Console.WriteLine("AZS_ACTTIVEDIRECTORY");
                    Console.WriteLine("AZS_ACTTIVEDIRECTORYRESOURCEID");
                    Console.WriteLine("AZS_MANAGEMENTENDPOINT");
                    Console.WriteLine("AZS_STORAGENDPOINT");   
                    Console.WriteLine("AZS_SUBID");                    
                    Console.WriteLine("AZS_TENANTID");
                    Console.WriteLine("AZS_CLIENTID");   
                    Console.WriteLine("AZS_SECURITYKEY");                    
                    Console.WriteLine("AZS_LOCATION");
                }
                else
                {
                    SampleRunAsync().Wait();

                    Console.WriteLine("Press any key to exit.\n");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Press any key to exit.\n");
                Console.ReadLine();
            }
            
        }

        public static void PrintStorageAccountKeys(IReadOnlyList<StorageAccountKey> storageAccountKeys)
        {
            foreach (var storageAccountKey in storageAccountKeys)
            {
                Console.WriteLine($"Key {storageAccountKey.KeyName} = {storageAccountKey.Value}");
            }
        }

        public static void PrintStorageAccount(StorageAccount sa)
        {
            Console.WriteLine($"{sa.Name} created @ {sa.CreationTime}");
        }

        public async static Task<Microsoft.Rest.ServiceClientCredentials> AzureAuthenticateAsync()
        {
            try
            {
                ActiveDirectoryServiceSettings s = new ActiveDirectoryServiceSettings();
                s.AuthenticationEndpoint = new Uri(AzS_ActiveDirectory);
                s.TokenAudience = new Uri(AzS_ActiveDirectoryResourceID);
                s.ValidateAuthority = true;
                string seckey = AzS_SecurityKey;

                string tid = AzS_TenantID;
                string cid = AzS_ClientID;


                var serviceCreds = await ApplicationTokenProvider.LoginSilentAsync(tid, cid, seckey, s);

                return serviceCreds;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
                return null;
            }
        }

        private static string generageRamdonName(string pre, int length)
        {
            var _constent = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var r = new Random();
            string result = pre;
            while (length > 0)
            {
                result += _constent[r.Next(0,_constent.Length)];
                length--;
            }

            return result;
        }

        public async static Task SampleRunAsync()
        {
            string muri = AzS_ManagementEndPoint;
            string sid = AzS_SubscriptionID;

            var creds = await AzureAuthenticateAsync();

            var resourceClient = new ResourceManagementClient(creds)
            {
                BaseUri = new Uri(muri),
                SubscriptionId = sid
            };

            var storageClient = new StorageManagementClient(creds)
            {
                BaseUri = new Uri(muri),
                SubscriptionId = sid
            };

            StorageSampleE2E(resourceClient, storageClient);
        }

        private static void StorageBlobSample(string accountName, string key)
        {
            string blobcontainerName = "sample";
            string blobname = "blockblob";
            StorageCredentials cre = new StorageCredentials(accountName, key);
            CloudStorageAccount storageAccount = new CloudStorageAccount(cre, AzS_StorageEndPoint, true);

            Console.WriteLine("Creating a blob container...");
            CloudBlobClient blob = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blob.GetContainerReference(blobcontainerName);
            blobContainer.CreateIfNotExists();
            Console.WriteLine("Blob Conainer '{0}' created.",blobcontainerName);

            CloudBlockBlob bb = blobContainer.GetBlockBlobReference(blobname);

            // prepare a random content for upload/download 
            int size = 5 * 1024;
            byte[] buffer = new byte[size];
            Random rand = new Random();
            rand.NextBytes(buffer);

            Console.WriteLine("Uploading the blob.");
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                bb.UploadFromStream(stream,size);
            }
            Console.WriteLine("Upload completed.");
            Console.WriteLine("Size of blob : {0} bytes.", bb.Properties.Length);

            Console.WriteLine("Downloading a blob.");

            // Download the contents from the blob.
            using (MemoryStream outputStream = new MemoryStream())
            {
                bb.DownloadToStream(outputStream, null, null); 
                Console.WriteLine("Download completed.");
                Console.WriteLine("Downloaded stream size : {0} bytes.", outputStream.Length);
            }

            Console.WriteLine("\nPress enter key to continue ...");
            Console.ReadLine(); 
        }

        private static void StorageSampleE2E(ResourceManagementClient resourceClient, StorageManagementClient storage)
        {
            string rgName = generageRamdonName("rgAzS", 20);
            string storageAccountName1 = generageRamdonName("sa1", 20).ToLower();

            try
            {
                //Register the Storage Resource Provider with the Subscription
                RegisterStorageResourceProvider(resourceClient);

                //Create a new resource group
                CreateResourceGroup(rgName, resourceClient);

                //Create a new KeyVault
                string vaultUri = CreateKeyVault(rgName, resourceClient, "KeyVaultSample");

                //Create a new account in a specific resource group with the specified account name                     
                CreateStorageAccount(rgName, storageAccountName1, storage);
                
                //Get the storage account keys for a given account and resource group
                IList<StorageAccountKey> acctKeys = storage.StorageAccounts.ListKeys(rgName, storageAccountName1).Keys;
                Console.WriteLine("Key1 = {0}\nKey2 = {1}\n",acctKeys[0].Value,acctKeys[1].Value);

                //Get all the account properties for a given resource group and account name
                StorageAccount storAcct = storage.StorageAccounts.GetProperties(rgName, storageAccountName1);

                //Get a list of storage accounts within a specific resource group
                IEnumerable<StorageAccount> storAccts = storage.StorageAccounts.ListByResourceGroup(rgName);

                //Get all the storage accounts for a given subscription
                IEnumerable<StorageAccount> storAcctsSub = storage.StorageAccounts.List();

                //Create a new container, upload and download blobs. 
                StorageBlobSample(storageAccountName1, acctKeys[0].Value);

                //Check if the account name is available
                bool? nameAvailable = storage.StorageAccounts.CheckNameAvailability(storageAccountName1).NameAvailable;

                //Delete a storage account with the given account name and a resource group
                DeleteStorageAccount(rgName, storageAccountName1, storage);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                // clean up the resource groups created in this sample code 

                if (resourceClient.ResourceGroups.CheckExistence(rgName))
                {
                    Console.WriteLine("Deleting the ResourceGroup of " + rgName);
                    resourceClient.ResourceGroups.Delete(rgName);
                    Console.WriteLine("sample resource group is cleaned up.\n");
                }
            }
        }


        /// Registers the Storage Resource Provider in the subscription.
        public static void RegisterStorageResourceProvider(ResourceManagementClient resourcesClient)
        {
            Console.WriteLine();
            Console.WriteLine("Registering Storage Resource Provider with subscription...");
            resourcesClient.Providers.Register("Microsoft.Storage");
            Console.WriteLine("Storage Resource Provider registered.\n");
        }


        /// Creates a new resource group with the specified name
        public static void CreateResourceGroup(string rgname, ResourceManagementClient resourcesClient)
        {
            Console.WriteLine("Creating a resource group...");
            var resourceGroup = resourcesClient.ResourceGroups.CreateOrUpdate(
                    rgname,
                    new ResourceGroup
                    {
                        Location = AzS_Location
                    });
            Console.WriteLine("Resource group created with name " + resourceGroup.Name);
            Console.WriteLine();

        }

        public static string CreateKeyVault(string rgName, ResourceManagementClient rmClient, string kbName)
        {
            Console.WriteLine("Create a Key Vault resource with a generic PUT");
            var keyVaultParams = new GenericResource
            {
                Location = "local",
                Properties = new Dictionary<string, object>{
                    {"tenantId", AzS_TenantID},
                    {"sku", new Dictionary<string, object>{{"family", "A"}, {"name", "standard"}}},
                    {"accessPolicies", Array.CreateInstance(typeof(string), 0)},
                    {"enabledForDeployment", true},
                    {"enabledForTemplateDeployment", true},
                    {"enabledForDiskEncryption", true}
                }
            };

            var keyVault = rmClient.Resources.CreateOrUpdate(
                rgName,
                "Microsoft.KeyVault",
                "",
                "vaults",
                kbName,
                "2015-06-01", 
                keyVaultParams);

            Console.WriteLine("Key Vault Name: {0} ", keyVault.Name);
            Console.WriteLine("Key Vault Id: {0} ", keyVault.Id);
            JObject joProperties = JObject.Parse(keyVault.Properties.ToString());
            string vaultUri = joProperties["vaultUri"].ToString();
            Console.WriteLine("Key Vault BaseURI: {0} ", vaultUri);
            return vaultUri;
        }

        /// Create a new Storage Account. If one already exists then the request still succeeds
        private static void CreateStorageAccount(string rgname, string acctName, StorageManagementClient storageMgmtClient)
        {
            StorageAccountCreateParameters parameters = GetDefaultStorageAccountParameters();

            Console.WriteLine("\nCreating a storage account...");
            var storageAccount = storageMgmtClient.StorageAccounts.Create(rgname, acctName, parameters);
            Console.WriteLine("Storage account created with name " + storageAccount.Name);
        }

        /// Deletes a storage account for the specified account name
        private static void DeleteStorageAccount(string rgname, string acctName, StorageManagementClient storageMgmtClient)
        {
            Console.WriteLine("Deleting the storage account of "+acctName);
            storageMgmtClient.StorageAccounts.Delete(rgname, acctName);
            Console.WriteLine("Storage account " + acctName + " deleted");
            Console.WriteLine();
        }

        /// Returns default values to create a storage account
        private static StorageAccountCreateParameters GetDefaultStorageAccountParameters()
        {
            StorageAccountCreateParameters account = new StorageAccountCreateParameters
            {
                Location = AzS_Location,
                Kind = DefaultStorageKind,
                Tags = DefaultTags,
                Sku = DefaultSku
            };

            return account;
        }
    }
}
