using System;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Cdn;
using Pulumi.AzureNative.Cdn.Inputs;
using SkuArgs = Pulumi.AzureNative.Cdn.Inputs.SkuArgs;

namespace Ba7besh.Deployment;

public class PhotoStorage
{
    public BlobContainer PhotosContainer { get; }
    public Output<string> CdnEndpoint { get; }

    public PhotoStorage(ResourceGroup resourceGroup, StorageAccount storageAccount)
    {
        PhotosContainer = new BlobContainer("photos", new BlobContainerArgs
        {
            AccountName = storageAccount.Name,
            ResourceGroupName = resourceGroup.Name,
            PublicAccess = PublicAccess.None,
            ContainerName = "photos"
        });

        var cdnProfile = new Profile("ba7besh-cdn", new ProfileArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            Sku = new SkuArgs { Name = Pulumi.AzureNative.Cdn.SkuName.Standard_Microsoft}
        });

        var endpoint = new Endpoint("photos", new EndpointArgs
        {
            ProfileName = cdnProfile.Name,
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            IsHttpAllowed = false,
            IsHttpsAllowed = true,
            IsCompressionEnabled = true,
            ContentTypesToCompress = ["image/jpeg", "image/png"],
            OriginHostHeader = storageAccount.PrimaryEndpoints.Apply(ep => new Uri(ep.Blob).Host),
            Origins =
            {
                new DeepCreatedOriginArgs
                {
                    HostName = storageAccount.PrimaryEndpoints.Apply(ep => new Uri(ep.Blob).Host),
                    HttpsPort = 443,
                    Name = "photos-origin"
                }
            }
        });

        CdnEndpoint = endpoint.HostName;
    }
}