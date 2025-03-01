using AppVidaMinisterio.Models;
using System.Globalization;

namespace AppVidaMinisterio.Services
{
    internal class GetWeeksService
    {
        // Esta clase se encarga de controlar la logica de las semanas a descargar y hacerlo en paralelo

        private string _url = "https://wol.jw.org/es/wol/meetings/r4/lp-s";
        private string Url { get; set; }
     
        public GetWeeksService()
        {
            Url = GetUrl();
        }

        public async Task<SortedDictionary<int, Semana>> GetWeeks()
        {
            // Saca las semanas apartir de la semana actual del dispostivo
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
                //  Cambia de año si llega a la ultima semana del año
                if (currentWeekNumber > 52)
                {
                    ChangeUrl(url);
                    currentWeekNumber = 1;
                }
            }
            await Task.WhenAll(tasks);
            
            return ConvertedArrayInDictionary(weekArray);
        }

        public async Task GetNewWeeks(SortedDictionary<int, Semana> weeks)
        {
            // Metodo para obtener nuevas semanas apartir de la ultima anteriormente descargada
            int currentDate = weeks.Keys.Last() + 1; 
            if (currentDate % 100 == 53)
                currentDate = ChangeDate(currentDate);
            string currentDateString = currentDate.ToString().Remove(0, 4);
            int currentWeekNumber = int.Parse(currentDateString);

            int validWeeks = await ValidWeeks(currentWeekNumber);
            Semana[] weekArray = new Semana[validWeeks];
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < validWeeks; i++)
            {
                weekArray[i] = new Semana();
                
                string url = $"{Url.Substring(0, Url.LastIndexOf('/') + 1)}{currentWeekNumber:D2}";
                tasks.Add(new WebScrapingService(weekArray[i], url).WebScraping());
                currentWeekNumber++;
                // Cambia de año si llega a la ultima semana del año
                if (currentWeekNumber > 52)
                {
                    url = ChangeUrl(url);
                    currentWeekNumber = 1;
                }
            }
            await Task.WhenAll(tasks);

            foreach (Semana week in weekArray)
            {
                if (currentDate % 100 == 53)
                {
                    currentDate = ChangeDate(currentDate);
                    weeks.Add(currentDate++, week);
                }
                else
                    weeks.Add(currentDate++, week);
            }
        }

        public async Task<int> ValidWeeks(int currentWeekNumber)
        {
            // Valida las semanas que si tienen contenido apartir de la semana recibida para determinar cuantas semanas se van a descargar
            try
            {
                int maxWeeks = 20;
                List<Task<bool>> tasks = new List<Task<bool>>();
                Semana week = new Semana();
                for (int i = 0; i < maxWeeks; i++)
                {
                    string url = $"{Url.Substring(0, Url.LastIndexOf('/') + 1)}{currentWeekNumber:D2}";
                    var service = new WebScrapingService(week, url);
                    tasks.Add(service.HasValidContentAsync());

                    currentWeekNumber++;
                    if (currentWeekNumber > 52)
                    {
                        url = ChangeUrl(url);
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
            catch (IndexOutOfRangeException ex)
            {
                throw new IndexOutOfRangeException("Error: ", ex);
            }
        }

        public SortedDictionary<int, Semana> ConvertedArrayInDictionary(Semana[] weekArray)
        {
            // Convierte el array de semanas en un diccionario para poder acceder a las semanas por su numero
            string currentDateString = $"{GetCurrentYear().ToString()}{GetCurrentWeek().ToString("D2")}";
            int currentWeekNumber = int.Parse(currentDateString);
            SortedDictionary<int, Semana> weeks = new SortedDictionary<int, Semana>();
            foreach (var week in weekArray)
            {
                if (currentWeekNumber % 100 == 53)
                {
                    currentWeekNumber = ChangeDate(currentWeekNumber);
                    weeks.Add(currentWeekNumber++, week);
                }
                else
                    weeks.Add(currentWeekNumber++, week);
            }
            return weeks;
        }

        public string GetUrl()
        {
            string currentYear = GetCurrentYear().ToString();
            string currentWeek = GetCurrentWeek().ToString("D2");
            return Url = $"{_url}/{currentYear}/{currentWeek}";
        }

        public int GetCurrentDate()
        {
            string currentDateString = $"{GetCurrentYear().ToString()}{GetCurrentWeek().ToString("D2")}";
            int currentDate = int.Parse(currentDateString);
            return currentDate;
        }

        public int GetCurrentWeek()
        {
            DateTime currenDate = DateTime.Now;
            var calendar = new GregorianCalendar();
            int currentWeek = calendar.GetWeekOfYear(currenDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return currentWeek;
        }

        public int GetCurrentYear()
        {
            int year = DateTime.Now.Year;
            return year;
        }

        public string ChangeUrl(string url)
        {
            string[] urlParts = url.Split('/');
            int actualYear = int.Parse(urlParts[urlParts.Length - 2]);
            int newYear = actualYear + 1;
            return $"{string.Join("/", urlParts[..^2])}/{newYear.ToString()}/01";
        }

        public int ChangeDate(int date) => date + 48;
    }
}
