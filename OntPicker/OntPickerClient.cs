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
        private int _smushLevel;
        private string _rootPlusOne;
        List<string> thisOnt = new List<string>();

        public OntPickerClient(string ontology, string apiKey, string root, int hLevel, int smushLevel)
        {
            this._ontology = ontology;
            this._apikey = apiKey;
            this._root = root;
            this._hLevel = hLevel;
            this._smushLevel = smushLevel;
            this._purlUri = string.Concat(this._purlUri, this._ontology, "/");
        }

        public async Task<List<string>> GetRootsAsync()
        {
            await this.GetRootsAsJsonAsync();

            return thisOnt;
        }

        public async Task<string> GetRawOntologyAsync()
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
                        return content.ToString();
                    }
                    else
                        return string.Empty;
                }
                else
                    return string.Empty;
            }
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
                            this._rootPlusOne = string.Concat(this._root, "\\", result[i].prefLabel);
                            await ProcessOntNode(result[i], this._hLevel, string.Empty);
                            int x = 1;
                        }
                    }
                }
            }
        }

        private async Task ProcessOntNode(Ontology node, int hLevel, string previousLevel)
        {
            // testing
            //if (thisOnt.Count > 50)
            //    return;

            int c_hlevel = hLevel;
            string id = MaskId(node.id);
            string name = node.prefLabel;
            string fullname;
            string toolTip;
            if (string.IsNullOrEmpty(previousLevel))
                fullname = string.Concat(this._root, "\\", id, "\\");
            else
                fullname = string.Concat(this._root, previousLevel, "\\", id, "\\");

            string visualAttribute = string.Empty;

            // Special root case
            if (hLevel == 3)
                toolTip = string.Concat(this._root, "\\", name);
            else
                toolTip = string.Concat(this._rootPlusOne, "\\", name, "(", id, ")");

            toolTip = toolTip.Replace("\\", " \\ "); // just adding spaces!
            toolTip.Trim(' '); // trim leading/trailing spaces
            toolTip = toolTip.Substring((toolTip.IndexOf(@"\", 3) + 2), (toolTip.Length - (toolTip.IndexOf(@"\", 3) + 2)));

            Ontpage children = await RetrieveChildren(node.links.children, true);

            if (children.pageCount != "0")
            {
                int pageCount;
                int.TryParse(children.pageCount, out pageCount);

                for (int i = 1; i <= pageCount; i++)
                {
                    visualAttribute = "FA";
                    thisOnt.Add(string.Concat(c_hlevel, "|", fullname, "|", name, "||", id, "|", visualAttribute, "|", toolTip, "|N"));

                    if (node.synonym.Count() > 0)
                    {
                        string altName;
                        foreach (var synonym in node.synonym)
                        {
                            altName = synonym.ToString();
                            thisOnt.Add(string.Concat(c_hlevel, "|", fullname, "|", name, "|", altName, "|", id, "|", visualAttribute, "|", toolTip, "|Y"));
                        }
                    }

                    for (int x = 0; x < children.collection.Count(); x++)
                    {
                        await ProcessOntNode(children.collection[x], hLevel + 1, string.Concat(previousLevel, "\\", id));
                    }

                    if (children.links.nextPage != null)
                        children = await RetrieveChildren(children.links.nextPage, false);
                }
            }
            else
            {
                visualAttribute = "LA";
                if (node.synonym.Count() > 0)
                {
                    thisOnt.Add(string.Concat(c_hlevel, "|", fullname, "|", name, "||", id, "|", visualAttribute, "|", toolTip, "|N"));
                    string altName;
                    foreach (var synonym in node.synonym)
                    {
                        altName = synonym.ToString();
                        thisOnt.Add(string.Concat(c_hlevel, "|", fullname, "|", name, "|", altName, "|", id, "|", visualAttribute, "|", toolTip, "|Y"));
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
                if (includeApiKey)
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