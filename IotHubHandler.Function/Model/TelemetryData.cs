using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotHubHandler.Function.Model
{
    internal class TelemetryData
    {
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }
        public string DeviceId { get; set; }
        public double Humidity { get; set; }
        public double TemperatureC { get; set; }
        public double TemperatureF { get; set; }
    }
}
