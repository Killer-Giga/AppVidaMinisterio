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

        Debe contar con un metodo para borrar la semanas anteriores a la actual. Y un metodo para actualizar las semanas siguentes que no esten 
        ya guardadas en el archivo. Por ejemplo si tengo los 2 meses proximos ya guardados no debe actualizarlos de nuevo porque podria borrar las semanas ya editadas.
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
