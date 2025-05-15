using System.Linq;
using Pulumi;
using Pulumi.AzureNative.Authorization;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using ManagedServiceIdentityType = Pulumi.AzureNative.Web.ManagedServiceIdentityType;

namespace Ba7besh.Deployment;

public class MainStack : Stack
{
    public MainStack()
    {
        var config = new Config();
        var appServicePlanSku = config.Get("appServicePlanSku") ?? "F1"; // Default to Free Tier
        var azureLocation = config.Get("azure-native:location") ?? "WestEurope"; // Changed to a more common location
        var servicePrincipalId = config.Require("servicePrincipalId");

        var resourceGroup = new ResourceGroup("ba7besh-rg", new ResourceGroupArgs
        {
            ResourceGroupName = "ba7besh-resource-group", // Explicitly set the resource group name
            Location = azureLocation
        });
        var spContributorRoleAssignment = new RoleAssignment("spContributorRole", new RoleAssignmentArgs
        {
            PrincipalId = servicePrincipalId,
            RoleDefinitionId =
                "/providers/Microsoft.Authorization/roleDefinitions/b24988ac-6180-42a0-ab88-20f7382dd24c", //Contributor
            Scope = resourceGroup.Id,
            PrincipalType = PrincipalType.ServicePrincipal
        });

        // Create a storage account
        var storageAccount = new StorageAccount("ba7beshsa", new StorageAccountArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountName = "ba7beshsa",
            Location = azureLocation,
            Kind = Kind.StorageV2,
            Sku = new SkuArgs
            {
                Name = SkuName.Standard_LRS
            },
            AllowBlobPublicAccess = true,
        }, new CustomResourceOptions { DependsOn = spContributorRoleAssignment });
        
        var photoStorage = new PhotoStorage(resourceGroup, storageAccount);
        CdnEndpoint = photoStorage.CdnEndpoint;
        
        var logsContainer = new BlobContainer("logs", new BlobContainerArgs
        {
            AccountName = storageAccount.Name,
            ResourceGroupName = resourceGroup.Name,
            PublicAccess = PublicAccess.None,
            ContainerName = "logs"
        });
        
        var spStorageContributorRoleAssignment = new RoleAssignment("spStorageContributorRole", new RoleAssignmentArgs
        {
            PrincipalId = servicePrincipalId,
            RoleDefinitionId =
                "/providers/Microsoft.Authorization/roleDefinitions/ba92f5b4-2d11-453d-a403-e96b0029c9fe", //Storage Blob Data Contributor
            Scope = storageAccount.Id,
            PrincipalType = PrincipalType.ServicePrincipal
        });

        // Create a storage container
        var container = new BlobContainer("zips", new BlobContainerArgs
        {
            AccountName = storageAccount.Name,
            ResourceGroupName = resourceGroup.Name,
            PublicAccess = PublicAccess.None
        }, new CustomResourceOptions { DependsOn = spStorageContributorRoleAssignment });

        // Upload the API `.zip` file to the blob container
        var zipPath = config.Require("ba7beshZipPath");
        var zipFileName = zipPath.Split("/").Last();
        var blob = new Blob(zipFileName, new BlobArgs
        {
            AccountName = storageAccount.Name,
            ResourceGroupName = resourceGroup.Name,
            ContainerName = container.Name,
            Source = new FileAsset(zipPath), // Path to the zip file in your GitHub Actions output
            ContentType = "application/zip"
        }, new CustomResourceOptions { DependsOn = spStorageContributorRoleAssignment });
        


        var blobUrl = GetBlobReadSasUrl(blob, storageAccount, container, resourceGroup.Name);

        var appServicePlan = new AppServicePlan("ba7besh-plan", new AppServicePlanArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Name = "ba7besh-plan",
            Kind = "App",
            Reserved = true,
            Sku = new SkuDescriptionArgs
            {
                Name = appServicePlanSku,
                Tier = appServicePlanSku == "F1" ? "Free" : "Standard"
            },
            Location = azureLocation // Explicitly set the location
        });
        
        var storageAccountKeys = Output.All(resourceGroup.Name, storageAccount.Name).Apply(names =>
        {
            var rgName = names.First();
            var accountName = names.Last();
            return ListStorageAccountKeys.InvokeAsync(new ListStorageAccountKeysArgs
            {
                ResourceGroupName = rgName,
                AccountName = accountName
            });
        });
        var connectionString = storageAccountKeys.Apply(keys =>
        {
            var key = keys.Keys[0].Value;
            return Output.Format($"DefaultEndpointsProtocol=https;AccountName={storageAccount.Name};AccountKey={key};EndpointSuffix=core.windows.net");
        });
        
        var firebaseCredentialsPath = config.Require("firebaseCredentialsPath");
        var dbConnectionString = config.Require("dbConnectionString");
        var appService = new WebApp("ba7besh-app", new WebAppArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Name = "ba7besh-app",
            Identity = new ManagedServiceIdentityArgs
            {
                Type = ManagedServiceIdentityType.SystemAssigned
            },
            ServerFarmId = appServicePlan.Id,
            Location = azureLocation, // Explicitly set the location
            SiteConfig = new SiteConfigArgs
            {
                Http20Enabled = true,
                LinuxFxVersion = "DOTNETCORE|8.0",
                AppSettings = new[]
                {
                    new NameValuePairArgs
                    {
                        Name = "WEBSITE_RUN_FROM_PACKAGE",
                        Value = blobUrl
                    },
                    new NameValuePairArgs
                    {
                        Name ="Firebase__CredentialsPath",
                        Value = firebaseCredentialsPath
                    },
                    new NameValuePairArgs
                    {
                        Name = "DbConnectionString",
                        Value = dbConnectionString
                    },
                    new NameValuePairArgs
                    {
                        Name = "PhotoStorage__ConnectionString", 
                        Value = connectionString
                    },
                    new NameValuePairArgs
                    {
                        Name = "PhotoStorage__ContainerName",
                        Value = photoStorage.PhotosContainer.Name,
                    },
                    new NameValuePairArgs
                    {
                        Name = "PhotoStorage__CdnEndpoint", 
                        Value = photoStorage.CdnEndpoint
                    },
                    new NameValuePairArgs
                    {
                        Name = "Storage__ConnectionString",
                        Value = connectionString
                    },
                    new NameValuePairArgs
                    {
                        Name = "Storage__ContainerName",
                        Value = logsContainer.Name
                    },
                    new NameValuePairArgs
                    {
                        Name = "BotApi__AuthToken", 
                        Value = config.Require("botApiToken")
                    }
                }
            }
        });
        
        // Upload the API `.zip` file to the blob container
        var botZipPath = config.Require("ba7beshBotZipPath");
        var botZipFileName = botZipPath.Split("/").Last();
        var botBlob = new Blob(botZipFileName, new BlobArgs
        {
            AccountName = storageAccount.Name,
            ResourceGroupName = resourceGroup.Name,
            ContainerName = container.Name,
            Source = new FileAsset(botZipPath), // Path to the zip file in your GitHub Actions output
            ContentType = "application/zip"
        }, new CustomResourceOptions { DependsOn = spStorageContributorRoleAssignment });
        var botBlobUrl = GetBlobReadSasUrl(botBlob, storageAccount, container, resourceGroup.Name);

        var botService = new WebApp("ba7besh-bot", new WebAppArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Name = "ba7besh-bot",
            Identity = new ManagedServiceIdentityArgs
            {
                Type = ManagedServiceIdentityType.SystemAssigned
            },
            ServerFarmId = appServicePlan.Id,
            Location = azureLocation,
            SiteConfig = new SiteConfigArgs
            {
                Http20Enabled = true,
                LinuxFxVersion = "DOTNETCORE|8.0",
                AppSettings = new[]
                {
                    new NameValuePairArgs
                    {
                        Name = "WEBSITE_RUN_FROM_PACKAGE",
                        Value = botBlobUrl
                    },
                    new NameValuePairArgs
                    {
                        Name = "BotConfiguration__BotToken",
                        Value = config.Require("botToken") 
                    },
                    new NameValuePairArgs
                    {
                        Name = "Api__BaseUrl",
                        Value = appService.DefaultHostName.Apply(hostname => $"https://{hostname}/api/v1")
                    },
                    new NameValuePairArgs
                    {
                        Name = "Api__AuthToken", 
                        Value = config.Require("botApiToken") 
                    },
                    new NameValuePairArgs
                    {
                        Name = "ASPNETCORE_ENVIRONMENT",
                        Value = "Production"
                    }
                }
            }
        });

        // Assign Storage Blob Data Reader Role to Managed Identity
        var appServiceBlobRoleAssignment = new RoleAssignment("appServiceBlobRole", new RoleAssignmentArgs
        {
            PrincipalId = appService.Identity.Apply(identity => identity.PrincipalId), // App Service Managed Identity
            RoleDefinitionId =
                "/providers/Microsoft.Authorization/roleDefinitions/2a2b9908-6ea1-4ae2-8e65-a410df84e7d1", //Storage Blob Data Reader,
            Scope = storageAccount.Id,
            PrincipalType = PrincipalType.ServicePrincipal
        });
        var botServiceBlobRoleAssignment = new RoleAssignment("botServiceBlobRole", new RoleAssignmentArgs
        {
            PrincipalId = botService.Identity.Apply(identity => identity.PrincipalId), // App Service Managed Identity
            RoleDefinitionId =
                "/providers/Microsoft.Authorization/roleDefinitions/2a2b9908-6ea1-4ae2-8e65-a410df84e7d1", //Storage Blob Data Reader,
            Scope = storageAccount.Id,
            PrincipalType = PrincipalType.ServicePrincipal
        });
        Endpoint = appService.DefaultHostName.Apply(hostname => $"https://{hostname}");
        BotEndpoint = botService.DefaultHostName.Apply(hostname => $"https://{hostname}");
    }

    [Output] public Output<string> Endpoint { get; set; }
    [Output] public Output<string> BotEndpoint { get; set; }
    [Output] public Output<string> CdnEndpoint { get; set; }
    private static Output<string> GetBlobReadSasUrl(Blob blob, StorageAccount account, BlobContainer container,
        Output<string> resourceGroupName)
    {
        // First, ensure that both `account.Name` and `container.Name` are available before invoking the SAS.
        return Output.Tuple(account.Name, container.Name).Apply(names =>
        {
            var (accountName, containerName) = names;
            // Combine the URL with the generated SAS token
            return Output.Format($"https://{accountName}.blob.core.windows.net/{containerName}/{blob.Name}");
        });
    }
}