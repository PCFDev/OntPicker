using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OntPicker;

namespace OntPickerConsole
{
    public class OntologyManager
    {
        private OntPickerClient _ontPicker;
        FileStream ostream;
        StreamWriter writer;
        TextWriter oldOut = Console.Out;

        public async Task ProcessOntology(string ontology, string apiKey, string root, int hLevel)
        {
            this._ontPicker = new OntPicker.OntPickerClient(ontology, apiKey, root, hLevel);

            List<string> x = await this._ontPicker.GetRootsAsync();

            try
            {
                ostream = new FileStream("./CPT.csv", FileMode.OpenOrCreate, FileAccess.Write);
                writer = new StreamWriter(ostream);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot open text file for writing.");
                Console.WriteLine(ex.Message);

                return;
            }

            foreach (string item in x)
            {
                Console.WriteLine(item);
            }

            Console.SetOut(writer);

            Console.SetOut(oldOut);
            writer.Close();
            ostream.Close();
            Console.WriteLine("Done");
        }
    }
}