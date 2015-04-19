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
        private Uri _baseUri = new Uri("http://data.bioontology.org/search");
        private string _ontology;
        private string _apikey;
        // private Uri _fullUri;

        public OntPickerClient(string ontology, string apiKey)
        {
            this._ontology = ontology;
            this._apikey = apiKey;
            //this._baseUri = new Uri(string.Concat(this._baseUri, this._ontology, "/classes/roots"));
            //this._fullUri = new Uri(string.Concat(this._baseUri, "?apikey=", apiKey));
        }

        public async Task<string> GetRootsAsync()
        {
            string json = this.GetRootsAsJsonAsync().Result;
            //XDocument temp = this.GetRootsAsXmlAsync().Result;

            //foreach (var item in json)
            //{

            //}

            return "abc";
        }

        public async Task<string> GetRootsAsJsonAsync()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = this._baseUri;
                var req = new StringContent(string.Concat(this._baseUri.ToString(), "?q=melanoma&apikey=", this._apikey));
                req.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";
                var response = await client.PostAsync("", req);
                
                if (response.StatusCode != null)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        try
                        {
                            var result = JsonConvert.DeserializeObject<Ontology>(response.ToString());
                            return result.ToString();
                        }
                        catch (Exception ex)
                        {
                            return ex.Message.ToString();
                        }
                        
                    }
            
                    return "abc";
                }
                else
                {
                    return "NULL response code";
                }
            }
        }
    }
}