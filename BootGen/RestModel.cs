using System;
using System.Collections.Generic;

namespace BootGen
{
    public class RestModel
    {
        public string Title { get; set; }
        public string Version { get; set; }
        public string Licence { get; set; }
        public string Url { get; set; }
        public List<OASSchema> Schemas { get; set; }
        public List<Route> Routes { get; set; }
        public List<RestResource> Resources { get; set; }
    }
}