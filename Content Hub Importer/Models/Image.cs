#region

using System.Runtime.Serialization;

#endregion

namespace ContentHub.Importer.Models
{
    [DataContract]
    public class Image
    {
        [DataMember]
        public string Src { get; set; }

        [DataMember]
        public string Alt { get; set; }
    }
}