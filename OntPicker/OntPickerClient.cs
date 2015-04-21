using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
        List<string> thisOnt = new List<string>();

        public OntPickerClient(string ontology, string apiKey, string root, int hLevel)
        {
            this._ontology = ontology;
            this._apikey = apiKey;
            this._root = root;
            this._hLevel = hLevel;
            this._purlUri = string.Concat(this._purlUri, this._ontology, "/");
        }

        public async Task<List<string>> GetRootsAsync()
        {
            await this.GetRootsAsJsonAsync();

            return thisOnt;
        }

        public async Task GetRootsAsJsonAsync()
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
                            await ProcessOntNode(result[i], this._hLevel, string.Empty);
                            int x = 10;
                        }
                    }
                }
            }
        }

        private async Task ProcessOntNode(Ontology node, int hLevel, string previousLevel)
        {
            int c_hlevel = hLevel;
            var name = node.prefLabel;
            string fullname;
            if (string.IsNullOrEmpty(previousLevel))
                fullname = string.Concat(this._root, node.prefLabel);
            else
                fullname = string.Concat(this._root, previousLevel, "\\", node.prefLabel);
            var id = MaskId(node.id);
            var visualAttribute = string.Empty;
            var toolTip = fullname.Replace("\\", " \\ ");

            // add final \ to fullname after creating toolTip
            fullname = fullname + "\\";

            Ontpage children = await RetrieveChildren(node.links.children, true);

            if (children.pageCount != "0")
            {
                int pageCount;
                int.TryParse(children.pageCount, out pageCount);

                for (int i = 1; i <= pageCount; i++)
                {
                    visualAttribute = "FA";

                    if (node.synonym.Count() > 0)
                    {
                        thisOnt.Add(string.Concat(c_hlevel, "|", fullname, "|", name, "|", id, "|", visualAttribute, "|", toolTip, "|N"));
                        string altName;
                        foreach (var synonym in node.synonym)
                        {
                            altName = synonym.ToString();
                            thisOnt.Add(string.Concat(c_hlevel, "|", fullname, "|", altName, "|", id, "|", visualAttribute, "|", toolTip, "|Y"));
                        }
                    }

                    for (int x = 0; x < children.collection.Count(); x++)
                    {
                        await ProcessOntNode(children.collection[x], hLevel + 1, name);
                    }

                    if(children.links.nextPage != null)
                        children = await RetrieveChildren(children.links.nextPage, false);
                }
            }
            else
            {
                visualAttribute = "LA";
                if (node.synonym.Count() > 0)
                {
                    thisOnt.Add(string.Concat(c_hlevel, "|", fullname, "|", name, "|", id, "|", visualAttribute, "|", toolTip, "|N"));
                    string altName;
                    foreach (var synonym in node.synonym)
                    {
                        altName = synonym.ToString();
                        thisOnt.Add(string.Concat(c_hlevel, "|", fullname, "|", altName, "|", id, "|", visualAttribute, "|", toolTip, "|Y"));
                    }
                }
            }
        }

        private async Task<Ontpage> RetrieveChildren(string url, bool includeApiKey)
        {
            Ontpage result = new Ontpage();

            using (var client = new HttpClient())
            {
                client.BaseAddress = this._baseUri;
                if(includeApiKey)
                    url = string.Concat(url, "?apikey=", this._apikey);
                var response = await client.GetAsync(url);

                if (response.StatusCode != null)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var jsonResponse = JsonConvert.DeserializeObject<Ontpage>(content);

                        result = jsonResponse;
                    }
                }
            }

            return result;
        }

        private string MaskId(string id)
        {
            return id.Remove(0, this._purlUri.Length);
        }
    }
}