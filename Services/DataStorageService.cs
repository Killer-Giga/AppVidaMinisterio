using AppVidaMinisterio.Models;
using System.Text.Json;

namespace AppVidaMinisterio.Services
{
    internal class DataStorageService
    {
        // Clase para guardar las semanas en json

        /*
         Recibe la semana generada en WebScrapingService y la agrega a un json.
         Estas semanas se acumulan en el mismo archivo y se van agregando mas al final de este.
         Además esta clase debe contar con la manera de editar el json según la _semanaActual en los entry y editor que hay por semana para actualizar 
        los nombres en las asignaciones. Esto se debe hacer automáticamente cuando haya algún cambio. 

        Debe contar con un metodo para borrar la semanas anteriores a la actual. Y un metodo para actualizar las semanas siguentes que no esten 
        ya guardadas en el archivo. Por ejemplo si tengo los 2 meses proximos ya guardados no debe actualizarlos de nuevo porque podria borrar las semanas ya editadas.
        */

        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "semanas.json");

        public DataStorageService() { }

        public DataStorageService(List<Semana> semanas)
        {
            if (!File.Exists(path))
            {
                string json = JsonSerializer.Serialize(semanas);
                File.WriteAllText(path, json);
            }
        }

        public async Task LeerJsonAsync(List<Semana>? semanas)
        {
            if (File.Exists(path))
            {
                using FileStream fs = File.OpenRead(path);
                semanas = await JsonSerializer.DeserializeAsync<List<Semana>>(fs);
            }
        }
    }
}
