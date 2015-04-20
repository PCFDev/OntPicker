using System;
using System.Linq;
using System.Threading.Tasks;
using OntPicker;

namespace OntPickerConsole
{
    public class OntologyManager
    {
        private OntPickerClient _ontPicker;

        public async Task ProcessOntology(string ontology, string apiKey, string root, int hLevel)
        {
            this._ontPicker = new OntPicker.OntPickerClient(ontology, apiKey, root, hLevel);

            var x = await this._ontPicker.GetRootsAsync();
        }
    }
}