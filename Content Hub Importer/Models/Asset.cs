using ContentHub.Importer.Utils;
using Stylelabs.M.Base.Querying;
using Stylelabs.M.Base.Querying.Filters;
using Stylelabs.M.Base.Querying.Linq;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Stylelabs.M.Framework.Essentials.LoadOptions;
using Stylelabs.M.Sdk;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Sdk.Contracts.Querying;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ContentHub.Importer
{
    public class Asset
    {
        public string OriginUrl { get; set; }
        public string Description { get; set; }
        public string MarketingDescription { get; set; }
        public string AssetType { get; set; }
        public string SocialMediaChannel { get; set; }
        public string ContentSecurity { get; set; }
        public string AssetSource { get; set; }
        public string LifecycleStatus { get; set; }

        private static string username = "ApiAdmin";

        public string Title
        {
            get
            {
                var lastPart = Path.GetFileNameWithoutExtension(OriginUrl);
                return lastPart ?? "Fallback Title - CHANGE ME";
            }
        }


        public static async Task<long> CreateAssetType(string name)
        {
            // Check if the asset type already exists
            var query = Query.CreateIdsQuery(entities =>
                    from e in entities
                    where e.Property(Constants.AssetType.Properties.Label, Constants.DefaultCulture) == name
                    select e);

            var result = await MConnector.Client.Querying.QueryIdsAsync(query).ConfigureAwait(false);
            if (result.Items.Count > 0) return result.Items.First();

            // Create a new asset type entity
            var assetType = await MConnector.Client.EntityFactory.CreateAsync(Constants.AssetType.DefinitionName, CultureLoadOption.Default).ConfigureAwait(false);

            // Set a human readable identifier
            assetType.Identifier = $"{Constants.AssetType.DefinitionName}.{name}";

            // Set the classification name
            assetType.SetPropertyValue(Constants.AssetType.Properties.Label, Constants.DefaultCulture, name);

            // Mark the asset type as a root taxonomy item
            assetType.IsRootTaxonomyItem = true;

            // Save the asset type
            var assetTypeId = await MConnector.Client.Entities.SaveAsync(assetType).ConfigureAwait(false);

            return assetTypeId;
        }

        public static async Task SetupAsset(Asset importedAsset)
        {
            // Optional: Impersonate the user to setup the asset
            var impersonatedClient = await MConnector.Client.ImpersonateAsync(username).ConfigureAwait(false);

            // Get or create the asset type
            var assetTypeId = await CreateAssetType(importedAsset.AssetType).ConfigureAwait(false);

            // Create an asset
            var assetId = await CreateAsset(impersonatedClient, importedAsset, assetTypeId).ConfigureAwait(false);

            // Create a fetchjob to attach a resource/file to the asset
            var fetchJobId = await Jobs.CreateFetchJob(assetId, new Uri(importedAsset.OriginUrl)).ConfigureAwait(false);

            // Add a delay of 30 seconds to let the job complete
            //Task.Delay(30000).Wait();
            Console.WriteLine($"Added asset {assetId}. Job ID {fetchJobId}");
            //// Check if the fetchjob has completed
            //if (await Jobs.IsJobCompleted(fetchJobId))
            //{
            //    // Create a publiclink for the preview rendition
            //    var publicLinkId = await PublicLink.Create(assetId, Constants.Renditions.Preview).ConfigureAwait(false);
            //}
        }
        public static async Task<long> CreateAsset(IMClient client, Asset importedAsset, long? assetTypeId = null)
        {
            // Create the entity resource
            var asset = await client.EntityFactory.CreateAsync(Constants.Asset.DefinitionName, CultureLoadOption.Default).ConfigureAwait(false);

            // Set the mandatory title property
            asset.SetPropertyValue(Constants.Asset.Properties.Title, importedAsset.Title);
            asset.SetPropertyValue(Constants.Asset.Properties.Description, Constants.DefaultCulture ,importedAsset.Description);
            asset.SetPropertyValue(Constants.Asset.Properties.AssetSource, importedAsset.AssetSource);
            asset.SetPropertyValue(Constants.Asset.Properties.MarketingDescription, importedAsset.MarketingDescription);

            // Link the asset to content repository: standard
            var standardContentRepository = await client.Entities.GetAsync(Constants.ContentRepositories.Standard).ConfigureAwait(false);
            var contentRepositoryRelation = asset.GetRelation<IChildToManyParentsRelation>(Constants.Asset.Relations.ContentRepositoryToAsset);
            contentRepositoryRelation.Parents.Add(standardContentRepository.Id.Value);

            // Link the asset to lifecycle
            var finalLifeCycleCreated = await client.Entities.GetAsync($"{Constants.Asset.LifeCyclePrefix}{importedAsset.LifecycleStatus}").ConfigureAwait(false);
            var finalLifeCycleRelation = asset.GetRelation<IChildToOneParentRelation>(Constants.Asset.Relations.FinalLifeCycleStatusToAsset);
            finalLifeCycleRelation.Parent = finalLifeCycleCreated.Id.Value;

            // Link the asset to content security
            if (!string.IsNullOrWhiteSpace(importedAsset.ContentSecurity))
            {
                var contentSecurityCreated = await client.Entities.GetAsync($"{Constants.Asset.ContentSecurityPrefix}{UppercaseFirst(importedAsset.ContentSecurity).Replace(" ", string.Empty)}").ConfigureAwait(false);
                var contentSecurityRelation = asset.GetRelation<IChildToOneParentRelation>(Constants.Asset.Relations.ContentSecurityToAsset);
                contentSecurityRelation.Parent = contentSecurityCreated.Id.Value;
            }

            // Link the asset to social media
            if (!string.IsNullOrWhiteSpace(importedAsset.SocialMediaChannel))
            {
                var socialMediaChannelCreated = await client.Entities.GetAsync($"{Constants.Asset.SocialMediaChannelPrefix}{importedAsset.SocialMediaChannel}").ConfigureAwait(false);
                var socialMediaChannelRelation = asset.GetRelation<IChildToOneParentRelation>(Constants.Asset.Relations.SocialMediaChannelToAsset);
                socialMediaChannelRelation.Parent = socialMediaChannelCreated.Id.Value;
            }

            // Link the asset to asset source
            //var assetSourceCreated = await client.Entities.GetAsync($"AssetSource.Legacy").ConfigureAwait(false);
            //var assetSourceRelation = asset.GetRelation<IChildToOneParentRelation>("AssetSource");
            //assetSourceRelation.Parent = assetSourceCreated.Id.Value;




            // Link the asset to the asset type when specified
            if (assetTypeId.HasValue)
            {
                var assetTypeRelation = asset.GetRelation<IChildToOneParentRelation>(Constants.Asset.Relations.AssetTypeToAsset);
                assetTypeRelation.Parent = assetTypeId.Value;
            }

            // Create the asset
            var assetId = await client.Entities.SaveAsync(asset).ConfigureAwait(false);

            // Return a reference to the newly created asset
            return assetId;
        }

        static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}
