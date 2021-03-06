//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v13.15.5.0 (NJsonSchema v10.6.6.0 (Newtonsoft.Json v9.0.0.0)) (http://NSwag.org)
// </auto-generated>
//----------------------


using StreamChat.Core.DTO.Responses;
using StreamChat.Core.DTO.Requests;
using StreamChat.Core.DTO.Events;

namespace StreamChat.Core.DTO.Models
{
    using System = global::System;

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.5.0 (NJsonSchema v10.6.6.0 (Newtonsoft.Json v9.0.0.0))")]
    internal partial class ImagesDTO
    {
        [Newtonsoft.Json.JsonProperty("fixed_height", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public ImageDataDTO FixedHeight { get; set; }

        [Newtonsoft.Json.JsonProperty("fixed_height_downsampled", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public ImageDataDTO FixedHeightDownsampled { get; set; }

        [Newtonsoft.Json.JsonProperty("fixed_height_still", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public ImageDataDTO FixedHeightStill { get; set; }

        [Newtonsoft.Json.JsonProperty("fixed_width", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public ImageDataDTO FixedWidth { get; set; }

        [Newtonsoft.Json.JsonProperty("fixed_width_downsampled", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public ImageDataDTO FixedWidthDownsampled { get; set; }

        [Newtonsoft.Json.JsonProperty("fixed_width_still", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public ImageDataDTO FixedWidthStill { get; set; }

        [Newtonsoft.Json.JsonProperty("original", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public ImageDataDTO Original { get; set; }

        private System.Collections.Generic.IDictionary<string, object> _additionalProperties = new System.Collections.Generic.Dictionary<string, object>();

        [Newtonsoft.Json.JsonExtensionData]
        public System.Collections.Generic.IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }

    }

}

