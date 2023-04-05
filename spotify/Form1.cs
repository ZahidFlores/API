using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net.Http;
using System.Net.Http.Headers;

namespace spotify
{
    public partial class Form1 : Form
    {
        private const string BaseUri = "https://api.spotify.com/v1/";

        private readonly HttpClient _client = new HttpClient();
        private const string ClientId = "04801439039b4745befd389a50464cf9";
        private const string ClientSecret = "df5addafa59b4e0394fbae1e4e0f97fd";
        private string _accessToken;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
        private async Task<string> GetAccessToken()
        {
            var url = "https://accounts.spotify.com/api/token";
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}"));

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            var body = new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"}
            };

            var response = await _client.PostAsync(url, new FormUrlEncodedContent(body));

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JObject.Parse(json);

                return data["access_token"].ToString();
            }

            throw new Exception("No se pudo obtener el token de acceso");
        }

        private async Task RefreshAccessToken()
        {
            _accessToken = await GetAccessToken();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        private async Task<string> GetArtistId(string artistName)
        {
            var url = $"{BaseUri}search?q={artistName}&type=artist";

            var response = await _client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JObject.Parse(json);
                var artist = data["artists"]["items"].First;

                return artist["id"].ToString();
            }

            throw new Exception("No se pudo obtener el id del artista");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var artistName = txtArtista.Text;

            var artistId = await GetArtistId(artistName);

            var tracks = await GetTopTracks(artistId);

            lstCanciones.Items.Clear();

            foreach (var track in tracks)
            {
                lstCanciones.Items.Add(track);
            }
        }
        private async Task<IEnumerable<string>> GetTopTracks(string artistId)
        {
            var url = $"{BaseUri}artists/{artistId}/top-tracks?market=es";

            var response = await _client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JObject.Parse(json);
                var tracks = data["tracks"];

                var result = new List<string>();

                foreach (var track in tracks)
                {
                    result.Add(track["name"].ToString());
                }

                return result;
            }

            throw new Exception("No se pudieron obtener las canciones");
        }
    }
}