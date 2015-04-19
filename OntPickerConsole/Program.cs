using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OntPickerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // Testing ----
            string ontology = "CPT";
            string apiKey = "f2e61f58-a80e-4b80-941a-6a19c0fcf8cc"; // My APIKEY
            // ---- Testing

            OntologyManager manager = new OntologyManager();

            manager.ProcessOntology(ontology, apiKey).ContinueWith(t =>
                {
                    if (t.Exception != null)
                        Console.WriteLine(t.Exception.Message);
                    else
                        Console.WriteLine("Done!");
                });
        }
    }
}