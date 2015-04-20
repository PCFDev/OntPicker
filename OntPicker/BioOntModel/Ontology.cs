using System.Collections.Generic;
using Newtonsoft.Json;

namespace OntPicker
{
    public class Ontology
    {
        public string prefLabel { get; set; }
        public List<object> synonym { get; set; }
        public List<object> definition { get; set; }
        public List<string> cui { get; set; }
        public List<string> semanticType { get; set; }
        public bool obsolete { get; set; }
        
        [JsonProperty("@id")]
        public string id { get; set; }
        
        [JsonProperty("@type")]
        public string type { get; set; }

        public Links links { get; set; }

        [JsonProperty("@context")]
        public Context context { get; set; }
    }

    public class Context
    {
        [JsonProperty("@vocab")]
        public string vocab { get; set; }
        public string prefLabel { get; set; }
        public string synonym { get; set; }
        public string definition { get; set; }
        public string obsolete { get; set; }
        public string semanticType { get; set; }
        public string cui { get; set; }

    }

    public class Links
    {
        public string self { get; set; }
        public string ontology { get; set; }
        public string children { get; set; }
        public string parents { get; set; }
        public string descendants { get; set; }
        public string ancestors { get; set; }
        public string tree { get; set; }
        public string notes { get; set; }
        public string mappings { get; set; }
        public string ui { get; set; }

        [JsonProperty("@context")]
        public Context context { get; set; }

        public class Context
        {
            public string self { get; set; }
            public string ontology { get; set; }
            public string children { get; set; }
            public string parents { get; set; }
            public string descendants { get; set; }
            public string ancestors { get; set; }
            public string tree { get; set; }
            public string notes { get; set; }
            public string mappings { get; set; }
            public string ui { get; set; }
        }
    }

}