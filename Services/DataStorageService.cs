using AppVidaMinisterio.Models;
using System.Text.Json;

namespace AppVidaMinisterio.Services
{
    internal class DataStorageService
    {
        // Clase para guardar las semanas en json

        /*
         Recibe la Dictionary<int, Semana> y la agrega a un json.
         Estas semanas se acumulan en el mismo archivo y se van agregando mas al final de este.
        */

        public string PathStorage { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "semanas.json");

        public async Task<SortedDictionary<int, Semana>> ReadJsonAsync()
        {
            if (File.Exists(PathStorage))
            {
                using FileStream fs = File.OpenRead(PathStorage);
                return await JsonSerializer.DeserializeAsync<SortedDictionary<int, Semana>>(fs) ?? new SortedDictionary<int, Semana>();
            }           
            return new SortedDictionary<int, Semana>();
        }

        public void SaveJson(SortedDictionary<int, Semana> semanas)
        {
            string json = JsonSerializer.Serialize(semanas);
            File.WriteAllText(PathStorage, json);
        }
    }
}
