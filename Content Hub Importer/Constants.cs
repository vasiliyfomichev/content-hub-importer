using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ContentHub.Importer
{
    public static class Constants
    {
        public static readonly CultureInfo DefaultCulture = CultureInfo.GetCultureInfo("en-US");

        public static class Job
        {
            public const string DefinitionName = "M.Job";

            public static class Properties
            {
                public const string Condition = "Job.Condition";
                public const string State = "Job.State";
                public const string Type = "Job.Type";
            }

            public static class Conditions
            {
                public const string Failed = "Failed";
                public const string Pending = "Pending";
                public const string Success = "Success";
            }

            public static class States
            {
                public const string Failed = "Failed";
                public const string Pending = "Pending";
                public const string Completed = "Completed";
            }

            public static class Types
            {
                public const string Processing = "Processing";
            }
        }
        public static class AssetType
        {
            public const string DefinitionName = "M.AssetType";

            public static class Properties
            {
                public const string Label = "Label";
            }

            public static class Relations
            {
                public const string AssetTypeToAsset = "AssetTypeToAsset";
            }
        }
        public static class Asset
        {
            public const string DefinitionName = "M.Asset";
            public const string AsssetTypeIdPrefix = "M.AssetType.";
            public const string LifeCyclePrefix = "M.Final.LifeCycle.Status.";
            public const string ContentSecurityPrefix = "ContentSecurity.";
            public const string SocialMediaChannelPrefix = "SocialMedia.";
            public static class MemberGroups
            {
                public const string Content = "Content";
            }

            public static class Properties
            {
                public const string ApprovalDate = "ApprovalDate";
                public const string Title = "Title";
                public const string Description = "Description";
                public const string MarketingDescription = "MarketingDescription";
                public const string FileName = "FileName";
                public const string AssetSource = "AssetSource";
            }

            public static class Relations
            {
                public const string AssetTypeToAsset = "AssetTypeToAsset";
                public const string AssetMediaToAsset = "AssetMediaToAsset";
                public const string ContentRepositoryToAsset = "ContentRepositoryToAsset";
                public const string FinalLifeCycleStatusToAsset = "FinalLifeCycleStatusToAsset";
                public const string ContentSecurityToAsset = "ContentSecurity";
                public const string AssetSourceToAsset = "AssetSource"; 
                public const string SocialMediaChannelToAsset = "SocialMediaChannel";
            }
        }

        public static class ContentRepositories
        {
            public const string Standard = "M.Content.Repository.Standard";
        }

        public static class LifeCycleStatus
        {
            public const string Created = "M.Final.LifeCycle.Status.Created";
            public const string Approved = "M.Final.LifeCycle.Status.Approved";
            public const string Rejected = "M.Final.LifeCycle.Status.Rejected";
            public const string Archived = "M.Final.LifeCycle.Status.Archived";
            public const string RequiresApproval = "M.Final.LifeCycle.Status.RequiresApproval";
        }
    }
}
