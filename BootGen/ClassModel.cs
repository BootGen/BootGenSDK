using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    /// <summary>Represents a class used in the REST API</summary>
    public class ClassModel
    {
        internal int Id { get; set; }

        /// <summary>Singular name of the class</summary>
        public string Name { get; internal set; }

        /// <summary>Plural name of the class. Use PluralNameAttribute to speciy explicitly </summary>
        public string PluralName { get; internal set; }

        /// <summary>All properties defined in class.</summary>
        public List<Property> Properties { get; internal set; }

        /// <summary>The property used as identifier</summary>
        public Property IdProperty => PropertyWithName("Id");

        /// <summary>Returns property with the given name</summary>
        /// <param>The parameter name to lool for.</param>
        /// <returns>The parameter if foun, null otherwise</returns>
        public Property PropertyWithName(string name)
        {
            return Properties.FirstOrDefault(p => p.Name == name);
        }

        /// <summary>Properties that are visible on the server side.</summary>
        public List<Property> ServerProperties => Properties.Where(p => p.Location != Location.ClientOnly).ToList();

        /// <summary>Properties that are visible on the client side.</summary>
        public List<Property> ClientProperties => Properties.Where(p => p.Location != Location.ServerOnly).ToList();

        /// <summary>Properties that are visible on both server and client side.</summary>
        public List<Property> CommonProperties => Properties.Where(p => p.Location == Location.Both).ToList();

        /// <summary>True if any property is required</summary>
        public bool HasRequiredProperties => Properties.Any(p => p.IsRequired);

        /// <summary>Determines if class is used on server, client or both</summary>
        public Location Location { get; set; }

        /// <summary>True if class is used as resorce</summary>
        public bool IsResource { get; set; }

        /// <summary>True if class is persisted to the database</summary>
        public bool Persisted { get; set; }

        /// <summary>Indicates that class usescreated nd updated timestamps</summary>
        public bool HasTimestamps { get; set; }
        internal bool RelationsAreSetUp { get; set; }
    }
}
