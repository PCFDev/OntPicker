using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace OntPicker
{
    public class OntPickerClient
    {
        //private Uri _baseUri = new Uri("http://data.bioontology.org/ontologies/");
        private Uri _baseUri = new Uri("http://data.bioontology.org");
        private string _purlUri = @"http://purl.bioontology.ort/ontology/";
        private string _ontology;
        private string _apikey;
        private int _hLevel;
        private string _root;
        private List<string> output = new List<string>();

        public OntPickerClient(string ontology, string apiKey, string root, int hLevel)
        {
            this._ontology = ontology;
            this._apikey = apiKey;
            this._root = root;
            this._hLevel = hLevel;
            this._purlUri = string.Concat(this._purlUri, this._ontology, "/");
        }

        public async Task<Ontology[]> GetRootsAsync()
        {
            var json = await this.GetRootsAsJsonAsync();

            return json;
        }

        public async Task<Ontology[]> GetRootsAsJsonAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = this._baseUri;
                    var url = string.Concat(this._baseUri, "ontologies/", this._ontology, "/classes/roots?apikey=", this._apikey);
                    var response = await client.GetAsync(url);

                    if (response.StatusCode != null)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            var result = JsonConvert.DeserializeObject<Ontology[]>(content);

                            for (int i = 0; i < result.Count(); i++)
                            {
                                ProcessRootOntology(result[i]);
                            }

                            return result;
                        }
                    }
                }

            }
            catch (Exception)
            {
                var x = 10;
                throw;
            }


            return null;
        }

        private async Task<Ontology> ProcessRootOntology(Ontology thisOntology)
        {
            var name = thisOntology.prefLabel;
            var fullname = string.Concat(this._root, thisOntology.prefLabel);
            var id = await MaskId(thisOntology.id);
            int c_hlevel = this._hLevel;
            var toolTip = fullname.Replace("\\", " \\ ");

            // add final \ to fullname
            fullname = fullname + "\\";

            output.Add(string.Concat(c_hlevel, "|", fullname, "|", name, "|", this._ontology, ":", id, "|", toolTip));


            //DoChildren(thisOntology.links.children);

            return null;
        }

        private async void DoChildren(string childrenUrl)
        {

        }

        private async Task<Ontology> BuildOntology(Ontology thisOntology, int hLevel)
        {
            var name = thisOntology.prefLabel;
            var fullname = string.Concat(this._root, @"\", thisOntology.prefLabel);
            var id = thisOntology.id;
            int c_hlevel = hLevel;

            return null;
        }

        private async Task<string> MaskId(string id)
        {
            return id.Remove(0, this._purlUri.Length);
        }
    }
}