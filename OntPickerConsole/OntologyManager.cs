using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OntPicker;

namespace OntPickerConsole
{
    public class OntologyManager
    {
        private OntPickerClient _ontPicker;
        FileStream ostream;
        FileStream cstream;
        FileStream bulkStream;
        StreamWriter owriter;
        StreamWriter cwriter;
        StreamWriter bulkWriter;
        //TextWriter oldOut = Console.Out;
        int smushLevel = 0;
        StringBuilder ontologyInsertString = new StringBuilder();
        StringBuilder conceptInsertString = new StringBuilder();
        
        string _ontologyFileName;
        string _conceptFileName;
        string _bulkFileName;

        public async Task GatherRawOntology(string ontology, string apiKey, string root)
        {
            string result = string.Empty;
            result = await this._ontPicker.GetRawOntologyAsync();

            if (File.Exists(this._bulkFileName))
                File.Delete(this._bulkFileName);
            bulkStream = new FileStream(this._bulkFileName, FileMode.CreateNew, FileAccess.Write);
            bulkWriter = new StreamWriter(this._bulkFileName);
            bulkWriter.Write(result);
        }

        public async Task GatherOntology(string ontology, string apiKey, string root, int hLevel, int smushLevel)
        {            
            this.smushLevel = smushLevel;
            this._ontPicker = new OntPicker.OntPickerClient(ontology, apiKey, root, hLevel, smushLevel);

            Console.WriteLine("Begining ontology pick...");

            List<string> result = new List<string>();
            result = await this._ontPicker.GetRootsAsync();

            Console.WriteLine("Picking complete, beginning processing...");

            this._ontologyFileName = string.Concat("./", ontology, "_Ontology.txt");
            this._conceptFileName = string.Concat("./", ontology, "_Concept.txt");
            this._bulkFileName = string.Concat("./", ontology, "_Bulk.txt");
            ProcessOntology(result, ontology);

            Console.WriteLine("Processing complete.");
            Console.WriteLine("Ontology file: " + this._ontologyFileName);
            Console.WriteLine("Concept file: " + this._conceptFileName);
            Console.ReadLine();
        }

        private void ProcessOntology(List<string> ontology, string ontologyName)
        {
            try
            {
                if (File.Exists(this._ontologyFileName))
                    File.Delete(this._ontologyFileName);

                if (File.Exists(this._conceptFileName))
                    File.Delete(this._conceptFileName);

                ostream = new FileStream(this._ontologyFileName, FileMode.CreateNew, FileAccess.Write);
                cstream = new FileStream(this._conceptFileName, FileMode.CreateNew, FileAccess.Write);
                owriter = new StreamWriter(ostream);
                cwriter = new StreamWriter(cstream);

                foreach (string item in ontology)
                {
                    string[] pieces = item.Split('|');
                    string fullName = string.Empty;
                    string toolTip = string.Empty;
                    string name = string.Empty;
                    string synonymName = string.Empty;

                    // insert
                    ontologyInsertString.Append("INSERT INTO dbo.Base_Ontology (C_HLEVEL, C_FULLNAME, C_NAME, C_SYNONYM_CD, C_VISUALATTRIBUTES, C_TOTALNUM, C_BASECODE, C_METADATAXML, C_FACTTABLECOLUMN, C_TABLENAME, C_COLUMNNAME, C_COLUMNDATATYPE, C_OPERATOR, C_DIMCODE, C_COMMENT, C_TOOLTIP, M_APPLIED_PATH, UPDATE_DATE, DOWNLOAD_DATE, IMPORT_DATE, SOURCESYSTEM_CD, VALUETYPE_CD, M_EXCLUSION_CD, C_PATH, C_SYMBOL) VALUES (");
                    if (pieces[7] == "N")
                        conceptInsertString.Append("INSERT INTO dbo.Base_Concepts (CONCEPT_PATH, CONCEPT_CD, NAME_CHAR, CONCEPT_BLOB, UPDATE_DATE, DOWNLOAD_DATE, IMPORT_DATE, SOURCESYSTEM_CD, UPLOAD_ID) VALUES (");

                    fullName = pieces[1];
                    toolTip = pieces[6];
                    name = pieces[2];
                    synonymName = pieces[3];

                    // for the ontology table
                    ProcessOntology(pieces, ontologyName, fullName, toolTip, name, synonymName);
                    Console.WriteLine("Processing Ontology ID: " + string.Concat(ontologyName, ":", pieces[4]));

                    // for the concepts table
                    if (pieces[7] == "N")
                    {
                        ProcessConcept(pieces[4], ontologyName, fullName, name);
                        Console.WriteLine("Processing Concept ID: " + string.Concat(ontologyName, ":", pieces[4]));
                    }
                }

                // write the ontology file
                owriter.WriteLine(ontologyInsertString);

                // write the concepts file
                cwriter.WriteLine(conceptInsertString);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot open text file for writing.");
                Console.WriteLine(ex.Message);
            }
            finally
            {
                owriter.Close();
                cwriter.Close();
                ostream.Close();
                cstream.Close();
                Console.WriteLine("Done");
            }

            return;
        }

        // for the concepts table
        private void ProcessConcept(string ontId, string ontologyName, string fullName, string name)
        {
            conceptInsertString.Append("'" + fullName + "'"); // CONCEPT_PATH
            conceptInsertString.Append(", '" + ontologyName + ":" + ontId + "'"); // CONCEPT_CD
            conceptInsertString.Append(", '" + name + "'"); // NAME_CHAR
            conceptInsertString.Append(",'', '" + DateTime.Today.ToShortDateString() + "', NULL, NULL, 'bioontology.org', 0"); // more stuff

            // close out
            conceptInsertString.Append(")\n");
        }

        // for the ontology table
        private void ProcessOntology(string[] pieces, string ontologyName, string fullName, string toolTip, string name, string synonymName)
        {
            ontologyInsertString.Append(pieces[0]); //C_HLEVEL
            ontologyInsertString.Append(", '" + fullName + "'"); //C_FULLNAME
            if (pieces[7] == "Y")
                ontologyInsertString.Append(", '" + synonymName + "'"); //C_NAME for synonym
            else
                ontologyInsertString.Append(", '" + name + "'"); // C_NAME for non-synonym
            ontologyInsertString.Append(", '" + pieces[7] + "'"); // C_SYNONYM_CD
            ontologyInsertString.Append(", '" + pieces[5] + "'"); // C_VISUALATTRIBUTES
            ontologyInsertString.Append(", 0"); // C_TOTALNUM
            ontologyInsertString.Append(", '" + ontologyName + ":" + pieces[4] + "'"); // C_BASECODE
            ontologyInsertString.Append(", '', 'CONCEPT_CD', 'CONCEPT_DIMENSION', 'CONCEPT_PATH', 'T', 'LIKE'"); // stuff
            ontologyInsertString.Append(", '" + fullName + "'"); //C_DIMCODE
            ontologyInsertString.Append(", ''"); // C_COMMENT
            ontologyInsertString.Append(", '" + toolTip + "'"); // C_TOOLTIP
            ontologyInsertString.Append(",'@', '" + DateTime.Today.ToShortDateString() + "', NULL, NULL, 'bioontology.org', NULL, NULL, NULL, '" + ontologyName + ":" + pieces[4] + "'"); // more stuff

            // close out
            ontologyInsertString.Append(")\n");
        }
    }
}