using AppVidaMinisterio.Models;
using System.Globalization;

namespace AppVidaMinisterio.Services
{
    internal class GetWeeksService
    {
        // Esta clase se encarga de controlar la logica de las semanas a descargar y hacerlo en paralelo
        private string Url { get; set; }
        public GetWeeksService(string url)
        {
            Url = url;
        }

        public async Task<SortedDictionary<int, Semana>> GetWeeks()
        {
            // Define las semanas a descargar apartir de la semana actual del dispositivo.
            // seria bueno que tuviera 2 sobrecargas
            // para que el metodo pueda empezar desde que se inicia la app pero tambien apartir de la ultima semana descargada del dispositivo
            int currentWeekNumber = GetCurrentWeek();
            int validWeeks = await ValidWeeks(currentWeekNumber);
            Semana[] weekArray = new Semana[validWeeks];
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < validWeeks; i++)
            {
                weekArray[i] = new Semana();
                string url = $"{Url.Substring(0, Url.LastIndexOf('/') + 1)}{currentWeekNumber:D2}";
                tasks.Add(new WebScrapingService(weekArray[i], url).WebScraping());
                currentWeekNumber++;
                // Probar con 2024
                if (currentWeekNumber > 52)
                {
                    ChangeUrl();
                    currentWeekNumber = 1;
                }
            }
            await Task.WhenAll(tasks);
            
            return ConvertedArrayInDictionary(weekArray);
        }    

        public async Task<int> ValidWeeks(int currentWeekNumber)
        {
            // Valida las semanas que si tienen contenido apartir de la semana del dispositivo para determinar cuantas semanas se van a descargar
            int maxWeeks = 25;
            List<Task<bool>> tasks = new List<Task<bool>>();
            List<int> weeksNumbers = new List<int>();
            Semana week = new Semana();

            for (int i = 0; i < maxWeeks; i++)
            {
                string url = $"{Url.Substring(0, Url.LastIndexOf('/') + 1)}{currentWeekNumber:D2}";
                var service = new WebScrapingService(week, url);
                tasks.Add(service.HasValidContentAsync());
                
                currentWeekNumber++;
                if (currentWeekNumber > 52)
                {
                    ChangeUrl();
                    currentWeekNumber = 1;
                }
            }

            bool[] results = await Task.WhenAll(tasks);
            int validWeeks = 0;
            foreach (bool isValid in results)
            {
                if (isValid)
                    validWeeks++;
                else
                    break;
            } 
            return validWeeks;
        }

        public SortedDictionary<int, Semana> ConvertedArrayInDictionary(Semana[] weekArray)
        {
            // Convierte el array de semanas en un diccionario para poder acceder a las semanas por su numero
            int currentWeekNumber = GetCurrentWeek();
            SortedDictionary<int, Semana> weeks = new SortedDictionary<int, Semana>();
            foreach (var week in weekArray)
            {
                weeks.Add(currentWeekNumber++, week);
            }
            return weeks;
        }

        public int GetCurrentWeek()
        {
            DateTime currenDate = DateTime.Now;
            var calendar = new GregorianCalendar();
            int currentWeek = calendar.GetWeekOfYear(currenDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return currentWeek;
        }

        public string ChangeUrl()
        {
            return Url = $"{Url.Substring(0, Url.LastIndexOf("22") + 2)}00/01";
        }
    }
}
