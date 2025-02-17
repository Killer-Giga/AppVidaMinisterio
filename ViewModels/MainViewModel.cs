using AppVidaMinisterio.Models;
using AppVidaMinisterio.Services;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;

namespace AppVidaMinisterio.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        // Propiedades para el Data Binding
        public event PropertyChangedEventHandler? PropertyChanged;

        // Comandos
        public ICommand SiguienteComand { get; }
        public ICommand AnteriorComand { get; }

        // Variables y propiedades para las semanas
        DataStorageService dataStorageService = new DataStorageService();
        List<Semana> semanas = new List<Semana>();
        // Nota: Que pasa si el dispositivo no esta conectado a internet. Se necesita manejar esas excepciones
        private string _url = "https://wol.jw.org/es/wol/meetings/r4/lp-s/2025/00";
        private int _numeroDeSemanaArray = 0;
        private Semana? _semanaActual;
        public Semana? SemanaActual
        {
            get => _semanaActual;
            set
            {
                if (_semanaActual != value)
                {
                    _semanaActual = value;
                    OnPropertyChanged(nameof(SemanaActual));
                    OnPropertyChanged(nameof(IsVisibleAsignacionMejoresMaestros3));
                    OnPropertyChanged(nameof(IsVisibleAsignacionMejoresMaestros4));
                    OnPropertyChanged(nameof(IsVisibleAsignacionVidaCristiana2));
                }    
            }
        }

        public bool IsVisibleAsignacionMejoresMaestros3 => !string.IsNullOrEmpty(SemanaActual?.AsignacionMejoresMaestros3);
        public bool IsVisibleAsignacionMejoresMaestros4 => !string.IsNullOrEmpty(SemanaActual?.AsignacionMejoresMaestros4);
        public bool IsVisibleAsignacionVidaCristiana2 => !string.IsNullOrEmpty(SemanaActual?.AsignacionVidaCristiana2);

        // Constructores 
        public MainViewModel()
        {
            SiguienteComand = new Command(SiguienteSemana);
            AnteriorComand = new Command(AnteriorSemana);
        }

        public async Task InitializeAsync()
        {
            // Al inciar la app se verifica si ya se han descargado las semanas. Si es asi se leen del archivo json.
            if (!File.Exists(dataStorageService.PathStorage))
            {
                await ObtenerSemanas();
                SemanaActual = semanas[0];
            }
            else
            {
                semanas = await dataStorageService.ReadJsonAsync();
                // No puede inicar con la primera semana de la list si no con la semana actual del año
                SemanaActual = semanas[0];
            }
                _numeroDeSemanaArray = ObtenerSemanaActual();
        }

        // Métodos para las semanas
        private void SiguienteSemana()
        {
            // Tienen Errores!!!!!!!!!!! Al dar siguiente avanza varias semanas a la vez
            // es porque esta detectando la semana actual del dispositvo y a esta le agrega mas 1 y por eso el salto grande
            // _numeroDeSemanaArray = ObtenerSemanaActual(); es el problema
            _numeroDeSemanaArray++;
            dataStorageService.SaveJson(semanas);
            if (_numeroDeSemanaArray >= semanas.Count)
                _numeroDeSemanaArray--;
            else
                SemanaActual = semanas[_numeroDeSemanaArray];
        }

        private void AnteriorSemana()
        {
            _numeroDeSemanaArray--;
            dataStorageService.SaveJson(semanas);
            if (_numeroDeSemanaArray < 0)
                _numeroDeSemanaArray++;
            else
                SemanaActual = semanas[_numeroDeSemanaArray];
        }

        // Este metodo es temporal es necesario otro enfoque para el proyecto final
        // porque es necesario que se descarguen las semanas la primera vez que se inicia la app
        // y despues de que el usuario presione un boton para descargar pero solo las semanas siguientes
        public async Task ObtenerSemanas()
        {
            int numeroDeSemanaActual = ObtenerSemanaActual();
            while (true)
            {   
                Semana semana = new Semana();
                string url = $"{_url.Substring(0, _url.LastIndexOf('/') + 1)}{numeroDeSemanaActual:D2}";
                await new WebScrapingService().WebScrappingService(url, semana);
                if (semana.SemanaFecha == null)
                {
                    break;
                }
                semanas.Add(semana);
                numeroDeSemanaActual++;
            }
            dataStorageService.SaveJson(semanas);
        }

        private int ObtenerSemanaActual()
        { 
            // Obtiene la semana actual del dispositivo para que al iniciar se muestre esta semana
            DateTime fechaActual = DateTime.Now;
            var calendario = new GregorianCalendar();
            int semanaActual = calendario.GetWeekOfYear(fechaActual, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return semanaActual;
        }

        // Metodos para el Data Binding
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}