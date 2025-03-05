using AppVidaMinisterio.Models;
using AppVidaMinisterio.Services;
using System.ComponentModel;
using System.Windows.Input;

namespace AppVidaMinisterio.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // Propiedades para el Data Binding
        public event PropertyChangedEventHandler? PropertyChanged;

        // Comandos
        public ICommand SiguienteComand { get; }
        public ICommand AnteriorComand { get; }
        public ICommand GuardarComand { get; }
        public ICommand GeneratePdfComand { get; }
        public ICommand NewWeeksComand { get; }
        public ICommand DeleteWeeksComand { get; }
        public ICommand LoadDataCommand { get; }

        // Variables y propiedades para las semanas
        DataStorageService dataStorageService = new DataStorageService();
        SortedDictionary<int, Semana> weeks = new SortedDictionary<int, Semana>();
        // Semana del año que se muestra en la pantalla de inicio. Esta conformado por año y numero de semana
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

        // Propiedades para animaciones
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
        public bool IsNotLoading => !IsLoading;

        // Constructores 
        public MainViewModel()
        {
            SiguienteComand = new Command(NextWeek);
            AnteriorComand = new Command(PreviousWeek);
            GuardarComand = new Command(SaveJson);
            GeneratePdfComand = new Command(PdfGenerator);
            NewWeeksComand = new Command(async () => await GetNewWeeksAsync());
            DeleteWeeksComand = new Command(DeleteOldWeeks);
            // Es necesario? Tal vez solo para pruebas
            LoadDataCommand = new Command(async () => await LoadDataAsync());
        }

        public async Task InitializeAsync()
        {
            // Al inciar la app se verifica si ya se han descargado las semanas. Si es asi se leen del archivo json.
            try
            {
                IsLoading = true;
                var getWeeksService = new GetWeeksService();
                _weekNumber = getWeeksService.GetCurrentDate();
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
            finally
            {
                IsLoading = false;
            }
        }

        // Métodos para las semanas
        private void NextWeek()
        {
            _weekNumber++;
            SaveJson();
            if (_weekNumber % 100 == 53)
            {
                _weekNumber += 48;
                if (!weeks.ContainsKey(_weekNumber))
                    _weekNumber -= 49;
                else
                    SemanaActual = weeks[_weekNumber];
            }
            if (!weeks.ContainsKey(_weekNumber))
                _weekNumber--;
            else
                SemanaActual = weeks[_weekNumber];
        }

        private void PreviousWeek()
        {
            _weekNumber--;
            SaveJson();
            if (_weekNumber % 100 == 00)
            {
                _weekNumber -= 48;
                if (!weeks.ContainsKey(_weekNumber))
                    _weekNumber += 49;
                else
                    SemanaActual = weeks[_weekNumber];
            }
            if (!weeks.ContainsKey(_weekNumber))
                _weekNumber++;
            else
                SemanaActual = weeks[_weekNumber];
        }

        private void SaveJson()
        {
            dataStorageService.SaveJson(weeks);
        }

        private void PdfGenerator()
        {
            if (SemanaActual != null)
            {
                PdfGeneratorService pdfGeneratorService = new PdfGeneratorService(SemanaActual);
            }
        }

        private async Task GetNewWeeksAsync()
        {
            try
            {
                IsLoading = true;
                var getWeekService = new GetWeeksService();
                await getWeekService.GetNewWeeks(weeks);
                SaveJson();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void DeleteOldWeeks()
        {
            // Metodo para borrar las semanas anteriores a la actual
            var getWeekService = new GetWeeksService();
            int currentWeek = getWeekService.GetCurrentDate();
            List<int> keys = new List<int>();

            foreach (var key in weeks)
            {
                if (key.Key < currentWeek)
                    keys.Add(key.Key);
            }

            foreach (int key in keys)
            {
                weeks.Remove(key);
            }
        }

        private async Task LoadDataAsync()
        {
            // Es necesario? NO ES NECESARIO ES SOLO PARA PRUEBAS
            IsLoading = true;
            await Task.Delay(3000);
            IsLoading = false;
        }

        // Metodos para el Data Binding
        protected virtual void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}