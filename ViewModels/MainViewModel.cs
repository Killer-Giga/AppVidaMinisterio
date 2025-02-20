using AppVidaMinisterio.Models;
using AppVidaMinisterio.Services;
using System.ComponentModel;
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
        public ICommand GuardarComand { get; }

        // Variables y propiedades para las semanas
        DataStorageService dataStorageService = new DataStorageService();
        SortedDictionary<int, Semana> weeks = new SortedDictionary<int, Semana>();

        // Nota: Que pasa si el dispositivo no esta conectado a internet. Se necesita manejar esas excepciones
        private string _url = "https://wol.jw.org/es/wol/meetings/r4/lp-s/2025/00";

        // Semana del año que se muestra en la pantalla de inicio
        private int _weekNumber = 0;

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

        public bool IsVisibleAsignacionMejoresMaestros3 => !string.IsNullOrEmpty(SemanaActual?.MejoresMaestros.Asignacion3);
        public bool IsVisibleAsignacionMejoresMaestros4 => !string.IsNullOrEmpty(SemanaActual?.MejoresMaestros.Asignacion4);
        public bool IsVisibleAsignacionVidaCristiana2 => !string.IsNullOrEmpty(SemanaActual?.VidaCristiana.Asignacion2);

        // Constructores 
        public MainViewModel()
        {
            SiguienteComand = new Command(NextWeek);
            AnteriorComand = new Command(PreviousWeek);
            GuardarComand = new Command(SaveJson);
        }

        public async Task InitializeAsync()
        {
            // Al inciar la app se verifica si ya se han descargado las semanas. Si es asi se leen del archivo json.
            var getWeeksService = new GetWeeksService(_url);
            _weekNumber = getWeeksService.GetCurrentWeek();
            if (!File.Exists(dataStorageService.PathStorage))
            {
                weeks = await getWeeksService.GetWeeks();
                SaveJson();
                SemanaActual = weeks[_weekNumber];
            }
            else
            {
                weeks = await dataStorageService.ReadJsonAsync();
                SemanaActual = weeks[_weekNumber];
            }
        }

        // Métodos para las semanas
        private void NextWeek()
        {
            _weekNumber++;
            SaveJson();
            if (!weeks.ContainsKey(_weekNumber))
                _weekNumber--;
            else
                SemanaActual = weeks[_weekNumber];
        }

        private void PreviousWeek()
        {
            _weekNumber--;
            SaveJson();
            if (!weeks.ContainsKey(_weekNumber))
                _weekNumber++;
            else
                SemanaActual = weeks[_weekNumber];
        }

        private void SaveJson()
        {
            dataStorageService.SaveJson(weeks);
        }

        // Metodos para el Data Binding
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}