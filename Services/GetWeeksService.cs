using AppVidaMinisterio.Models;
using System;
using System.Globalization;

namespace AppVidaMinisterio.Services
{
    internal class GetWeeksService
    {
        // Esta clase se encarga de controlar la logica de las semanas a descargar, y hacerlo en paralelo


        /* "https://wol.jw.org/es/wol/d/r4/lp-s/202024169"; asi es la nueva url.
         /202024169 20 es por defecto, 2024 es el año y 169 es la semana por decirlo asi
         aumenta 80 en 80 cada vez que se acaba una guia, es decir
         /202025001 es la primera semana del 2025 y la primera guia del 2025
         /202025008 es la octava semana del 2025 y la ultima leccion de la primera guia del 2025
         /202025080 es la segunda guia del 2025 pero la portada
         /202025081 es la primera leccion de la segunda guia del 2025
         /202025161 es la primera leccion de la tercera guia del 2025
         /202025241 es la primera leccion de la cuarta guia del 2025
        asi sucesivamente hasta llegar a la ultima guia del 2025 que es la /202025401
        */

        private string _url = "https://wol.jw.org/es/wol/d/r4/lp-s/20";
        // private bool _has53WeekYear; // hay años con 53 semanas, lo normal son 52
        private string Url { get; set; }


        public GetWeeksService()
        {
            Url = GetUrl();
            // _has53WeekYear = YearWhit53Weeks(GetCurrentYear());
        }

        public async Task<SortedDictionary<int, Semana>> GetWeeks()
        {
            int currentMonthNumber = GetCurrentMonth();
            int currentYearNumber = GetCurrentYear();
            int validMonths = await ValidMonth(currentMonthNumber, currentYearNumber);

            var result = new SortedDictionary<int, Semana>();

            for (int i = 0; i < validMonths; i++)
            {
                int validWeeks = await ValidWeek(currentMonthNumber, currentYearNumber);
                Semana[] weekArray = new Semana[validWeeks];
                List<Task> tasks = new List<Task>();

                for (int j = 0; j < validWeeks; j++)
                {
                    weekArray[j] = new Semana();

                    string baseUrl = Url[..^7]; // siempre recortar de Url original
                    string url = $"{baseUrl}{currentYearNumber}{currentMonthNumber:D2}{j + 1}";

                    tasks.Add(new WebScrapingService(weekArray[j], url).WebScraping());
                }

                await Task.WhenAll(tasks);

                // Ahora convertimos el array en diccionario parcial
                var partialWeeks = ConvertedArrayInDictionary(weekArray, currentYearNumber, currentMonthNumber);

                // Y lo agregamos al diccionario final sin borrar lo anterior
                foreach (var item in partialWeeks)
                    if (!result.ContainsKey(item.Key))
                        result.Add(item.Key, item.Value);

                // Avanzar mes
                if (currentMonthNumber >= 40)
                {
                    currentMonthNumber = 0;
                    currentYearNumber++;
                }
                else
                    currentMonthNumber += 8;
            }

            return result;
        }


        public async Task GetNewWeeks(SortedDictionary<int, Semana> weeks) // Falta arreglar para que reinicie si ya se llego a la ultima semana del año||||||||||||||||||||||
        {
            // Metodo para obtener nuevas semanas apartir de la ultima anteriormente descargada
            int currentDate = weeks.Keys.Last();
            string currentMonthString = currentDate.ToString().Substring(4, 2);
            string currentYearString = currentDate.ToString().Substring(0, 4);

            int currentWeekNumber = int.Parse(currentMonthString);
            int currentYearNumber = int.Parse(currentYearString);
            int validWeeks = await ValidMonth(currentWeekNumber, currentYearNumber);

            Semana[] weekArray = new Semana[validWeeks];
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < validWeeks; i++) // es necesario que se sigan verificando las semanas? no podre simplemente descargar hasta que no haya mas semanas?
            {
                weekArray[i] = new Semana();

                string url = $"{Url.Substring(0, Url.LastIndexOf('/') + 1)}{currentWeekNumber:D2}";
                tasks.Add(new WebScrapingService(weekArray[i], url).WebScraping());
                currentWeekNumber++;
                // Cambia de año si llega a la ultima semana del año
                if (currentWeekNumber > 52) // OJO, hay años con 53 semanas asi que se necesita un bool para verificar esto.
                {
                    ChangeUrl(url);
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

        public async Task<int> ValidMonth(int currentMonth, int currentYear)
        {
            // Valida los meses que si tienen contenido apartir del mes recibido para determinar cuantas semanas se van a descargar
            try
            {
                int maxMonth = 4;
                string baseUrl = Url[..^7]; // siempre recortar de Url original
                string url = $"{baseUrl}{currentYear}{currentMonth:D2}1";
                int[] monthCode = { 00, 08, 16, 24, 32, 40 };
                List<Task<bool>> tasks = new List<Task<bool>>();
                var checkUrl = new CheckContentService();
                for (int i = 0; i < maxMonth; i++)
                {
                    switch (currentMonth)
                    {
                        case 0:
                            url = $"{baseUrl}{currentYear}{currentMonth:D2}1";
                            currentMonth = monthCode[1];
                            break;
                        case 08:
                            url = $"{baseUrl}{currentYear}{currentMonth:D2}1";
                            currentMonth = monthCode[2];
                            break;
                        case 16:
                            url = $"{baseUrl}{currentYear}{currentMonth:D2}1";
                            currentMonth = monthCode[3];
                            break;
                        case 24:
                            url = $"{baseUrl}{currentYear}{currentMonth:D2}1";
                            currentMonth = monthCode[4];
                            break;
                        case 32:
                            url = $"{baseUrl}{currentYear}{currentMonth:D2}1";
                            currentMonth = monthCode[5];
                            break;
                        case 40:
                            url = $"{baseUrl}{currentYear}{currentMonth:D2}1";
                            currentMonth = monthCode[0];
                            currentYear++;
                            break;
                        default:
                            url = $"{baseUrl}{currentYear}{currentMonth:D2}1";
                            break;
                    }
                    tasks.Add(checkUrl.HasValidContentAsync(url));
                }

                bool[] results = await Task.WhenAll(tasks);
                int validMonths = results.TakeWhile(isValid => isValid).Count();
                return validMonths;
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new IndexOutOfRangeException("Error: ", ex);
            }
        }

        public async Task<int> ValidWeek(int currentMonth, int currentYear)
        {
            // Valida las semanas que si tienen contenido apartir del mes recibido para determinar cuantas semanas se van a descargar
            try
            {
                int maxWeeks = 13;
                string baseUrl = Url[..^7]; // siempre recortar de Url original
                string url = $"{baseUrl}{currentYear}{currentMonth:D2}1";
                List<Task<bool>> tasks = new List<Task<bool>>();
                var checkUrl = new CheckContentService();

                for (int i = 1; i < maxWeeks; i++)
                {
                    if (i < 10)
                        url = $"{url.Remove(Url.Length - 1)}{i}";
                    else
                        url = $"{url.Remove(Url.Length - 2)}{i}";

                    tasks.Add(checkUrl.HasValidContentAsync(url));
                }

                bool[] results = await Task.WhenAll(tasks);
                int validWeeks = results.TakeWhile(isValid => isValid).Count();
                return validWeeks;
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new IndexOutOfRangeException("Error: ", ex);
            }
        }

        public SortedDictionary<int, Semana> ConvertedArrayInDictionary(Semana[] weekArray, int year, int month)
        {
            // Convierte el array de semanas en un diccionario para poder acceder a las semanas por su numero
            string currentDateString = $"{year}{month:D2}1";
            int currentWeekNumber = int.Parse(currentDateString);
            var weeks = new SortedDictionary<int, Semana>();

            foreach (var week in weekArray)
            {
                weeks.Add(currentWeekNumber++, week);
            }
            return weeks;
        }

        public string GetUrl()
        {
            string currentYear = GetCurrentYear().ToString();
            string currentMonth = GetCurrentMonth().ToString();
            return Url = $"{_url}{currentYear}{currentMonth}1"; // El 1 al final es porque siempre empieza con 1 la guia del mes
        }

        public int GetCurrentDate()
        {
            // Este metodo da el inicio de la guia actual, pero lo ideal es que devuelva la semana actual  ||| PENDIENTE |||

            /*
            string currentDateString = $"{GetCurrentYear().ToString()}{GetCurrentWeek().ToString("D2")}";
            int currentDate = int.Parse(currentDateString);
            return currentDate;
            */

            string currentYear = GetCurrentYear().ToString();
            string currentMonth = GetCurrentMonth().ToString();
            string currentDate = $"{currentYear}{currentMonth}1"; // El 1 al final es porque siempre empieza con 1 la guia del mes
            return int.Parse(currentDate);
        }

        /* Metodo usado en la logica anterior de 52 semanas por año
        public int GetCurrentWeek()
        {
            DateTime currenDate = DateTime.Now;
            var calendar = new GregorianCalendar();
            int currentWeek = calendar.GetWeekOfYear(currenDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return currentWeek;
        }
        */

        public int GetCurrentMonth(int currentMonth = 0)
        {
            if (currentMonth == 0)
                currentMonth = DateTime.Now.Month;

            int[] monthCode = { 00, 08, 16, 24, 32, 40 }; // Codigo para cambiar de mes en la url
            int index = (int)Math.Ceiling((double)currentMonth / 2) - 1;

            return monthCode[index];
        }

        public int GetCurrentYear()
        {
            int year = DateTime.Now.Year;
            return year;
        }

        void ChangeUrl(string url)
        {
            // Cuando se cambia de año, se actualiza la url con el nuevo año.
            string lastYear = url.Substring(url.Length - 7, 4);
            int newYear = 0;
            if (int.TryParse(lastYear, out int year))
            {
                newYear = year + 1;
                Url = url.Replace(lastYear, newYear.ToString());
            }
            else
            {
                throw new FormatException("El formato del año en la URL es incorrecto.");
            }
        }

        public int ChangeDate(int currentKey, bool isPreview = false)
        {
            string year = currentKey.ToString().Substring(0, 4);
            int currentYearNumber = int.Parse(year);
            string month = currentKey.ToString().Substring(4, 2);
            int currentMonthNumber = int.Parse(month);

            if (currentMonthNumber >= 40)
            {
                currentMonthNumber = 0;
                currentYearNumber = isPreview == false ? currentYearNumber + 1 : currentYearNumber - 1;

                return int.Parse($"{currentYearNumber}{currentMonthNumber:D2}1");
            }
            else
            {
                if (currentMonthNumber % 4 == 0)
                    currentMonthNumber = isPreview == false ? currentMonthNumber += 8 : currentMonthNumber -= 8;
                else
                    currentMonthNumber = isPreview == false ? currentMonthNumber -= 7 : currentMonthNumber += 7;

                return int.Parse($"{currentYearNumber}{currentMonthNumber:D2}1");
            }
        }

        /* Metodo usado en la logica anterior de 52 semanas por año
        bool YearWhit53Weeks(int year)
        {
            // Obtener el primer día del año
            DateTime firstDay = new DateTime(year, 1, 1);

            // Verificar si el año es bisiesto
            bool isLeapYear = DateTime.IsLeapYear(year);

            // Regla ISO: 53 semanas si empieza en jueves, o si es bisiesto y empieza en miércoles
            return firstDay.DayOfWeek == DayOfWeek.Thursday ||
                   (isLeapYear && firstDay.DayOfWeek == DayOfWeek.Wednesday);
        }
        */
    }
}
