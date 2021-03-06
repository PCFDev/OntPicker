﻿using System;
using System.Collections.Generic;

namespace OntPickerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // Testing ----
            string ontology = "CPT";
            int hLevel = 3; // \i2b2(0)\Procedures(1)\!!!!(2)
            string root = "\\i2b2\\Procedures\\CPT"; // for procedures
            string apiKey = "f2e61f58-a80e-4b80-941a-6a19c0fcf8cc"; // My APIKEY
            int smushLevel = 2;
            // ---- Testing
            
            OntologyManager manager = new OntologyManager();

            // uses default temp tables for Ontology: Base_Ontology and for Concepts: Base_Concepts
            manager.GatherOntology(ontology, apiKey, root, hLevel, smushLevel).ContinueWith(t =>
                {
                    if (t.Exception != null)
                        Console.WriteLine(t.Exception.Message);
                    else
                        Console.WriteLine("Done!");
                });

            Console.ReadLine();
        }
    }
}