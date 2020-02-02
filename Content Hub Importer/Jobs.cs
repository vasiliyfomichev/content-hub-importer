using ContentHub.Importer.Utils;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Stylelabs.M.Framework.Utilities;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Sdk.Models.Jobs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Importer
{
    public static class Jobs
    {
        public static async Task<long> CreateFetchJob(long entityId, Uri resourceUrl)
        {
            // Validation
            Guard.GreaterThan("entityId", entityId, 0);
            Guard.NotNull("resourceUrl", resourceUrl);

            // Create the fetch job request
            var fjr = new WebFetchJobRequest("File", entityId);
            fjr.Urls.Add(resourceUrl);

            // Create the fetch job
            long fetchJobId = await MConnector.Client.Jobs.CreateFetchJobAsync(fjr).ConfigureAwait(false);
            return fetchJobId;
        }

        public static async Task<bool> IsJobCompleted(long id)
        {
            // Query for the job by id
            var job = await MConnector.Client.Entities.GetAsync(id, EntityLoadConfiguration.Full);

            // Get the state of the job
            var stateProperty = job.GetProperty<ICultureInsensitiveProperty>(Constants.Job.Properties.State);

            // Check if the job was completed
            return (stateProperty.GetValue<string>() == Constants.Job.States.Completed);
        }
    }
}
