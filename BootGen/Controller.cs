using System.Collections.Generic;

namespace BootGen
{
    /// <summary>Represents an ASP.Net Core Controller</summary>
    public class Controller
    {

        /// <summary>Name of the controller</summary>
        public string Name { get; set; }

        /// <summary>Methods implemented by the controller</summary>
        public List<Method> Methods { get; set; }

        /// <summary>Indicates if the controller needs to authenticate or not</summary>
        public bool Authenticate { get; set; }
    }
}