using System.Collections.Generic;

namespace OntPicker
{
    public class Ontpage
    {
        public string page { get; set; }
        public string pageCount { get; set; }
        public string prevPage { get; set; }
        public string nextPage { get; set; }
        public List<Ontology> collection { get; set; }

        public class Links
        {
            public string nextPage { get; set; }
            public string prevPage { get; set; }
        }
    }
}