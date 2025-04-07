using AppVidaMinisterio.Models;
using AppVidaMinisterio.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using AppVidaMinisterio.Messages;

namespace AppVidaMinisterio.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        // Variables y propiedades para las semanas
        DataStorageService dataStorageService = new DataStorageService();
        SortedDictionary<int, Semana> weeks = new SortedDictionary<int, Semana>();

        // Semana del año que se muestra en la pantalla de inicio. Esta conformado por año y numero de semana
        private int _weekNumber = 0;
        public int WeekNumber
        {
            get => _weekNumber;
            set => SetProperty(ref _weekNumber, value);
        }

        private Semana? _semanaActual;
        public Semana? SemanaActual
        {
            get => _semanaActual;
            set
            {
                if (SetProperty(ref _semanaActual, value))
                {
                    // Notificamos que las propiedades dependientes también cambiaron
                    OnPropertyChanged(nameof(IsVisibleAsignacionMejoresMaestros3));
                    OnPropertyChanged(nameof(IsVisibleAsignacionMejoresMaestros4));
                    OnPropertyChanged(nameof(IsVisibleAsignacionVidaCristiana2));
                    OnPropertyChanged(nameof(IsVisibleIsVisit));
                }
            }
        }

        public bool IsVisibleAsignacionMejoresMaestros3 =>
            !string.IsNullOrEmpty(SemanaActual?.MejoresMaestros.Asignacion3);
        public bool IsVisibleAsignacionMejoresMaestros4 =>
            !string.IsNullOrEmpty(SemanaActual?.MejoresMaestros.Asignacion4);
        public bool IsVisibleAsignacionVidaCristiana2 =>
            !string.IsNullOrEmpty(SemanaActual?.VidaCristiana.Asignacion2);
        public bool IsVisibleIsVisit => SemanaActual?.IsVisit ?? false;
        public bool IsVisibleIsUpdate => SemanaActual?.IsUpdate ?? false;

        // Propiedades para animaciones
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(IsNotLoading));
                };
            }
        }

        public bool IsNotLoading => !IsLoading;
        
        // Constructores 
        public MainViewModel()
        {
        }

        public async Task InitializeAsync()
        {
            // Al inciar la app se verifica si ya se han descargado las semanas. Si es asi se leen del archivo json.
            try
            {
                IsLoading = true;
                var getWeeksService = new GetWeeksService();
                WeekNumber = getWeeksService.GetCurrentDate();
                if (!File.Exists(dataStorageService.PathStorage))
                {
                    weeks = await getWeeksService.GetWeeks();
                    SaveJson();
                    SemanaActual = weeks[WeekNumber];
                }
                else
                {
                    weeks = await dataStorageService.ReadJsonAsync();
                    SemanaActual = weeks[WeekNumber];
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Métodos para las semanas
        [RelayCommand]
        private void NextWeek()
        {
            WeekNumber++;
            SaveJson();
            if (WeekNumber % 100 == 53)
            {
                WeekNumber += 48;
                if (!weeks.ContainsKey(WeekNumber))
                    WeekNumber -= 49;
                else
                    WeakReferenceMessenger.Default.Send(new AnimationMessage("ButtonPrevious"));
            }
            else if (!weeks.ContainsKey(WeekNumber))
                WeekNumber--;
            else
                WeakReferenceMessenger.Default.Send(new AnimationMessage("ButtonNext"));
        }

        [RelayCommand]
        private void PreviousWeek()
        {
            WeekNumber--;
            SaveJson();
            if (WeekNumber % 100 == 00)
            {
                WeekNumber -= 48;
                if (!weeks.ContainsKey(WeekNumber))
                    WeekNumber += 49;
                else
                    WeakReferenceMessenger.Default.Send(new AnimationMessage("ButtonPrevious"));
            }
            else if (!weeks.ContainsKey(WeekNumber))
                WeekNumber++;
            else
                WeakReferenceMessenger.Default.Send(new AnimationMessage("ButtonPrevious"));
        }

        public void UpdateWeekAfterAnimation()
        {
            SemanaActual = weeks[WeekNumber];
        }

        [RelayCommand]
        private void SaveJson()
        {
            dataStorageService.SaveJson(weeks);
        }

        [RelayCommand]
        private void PdfGenerator()
        {
            if (SemanaActual != null)
            {
                PdfGeneratorService pdfGeneratorService = new PdfGeneratorService(SemanaActual);
            }
        }

        [RelayCommand]
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

        [RelayCommand]
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

        [RelayCommand]
        private void ChangeByVisit() 
        { 
            // Quita el estudio Biblico por el discurso de la visita
            if (SemanaActual != null)
            {
                SemanaActual.IsVisit = !SemanaActual.IsVisit;
                OnPropertyChanged(nameof(IsVisibleIsVisit));
            }
        }

        [RelayCommand]
        private void ChangeByUpdate()
        {
            // Cambia las asignaciones de vida crisitana por el informe del cuerpo gobernante
            if (SemanaActual != null)
            {
                SemanaActual.IsUpdate = !SemanaActual.IsUpdate;
                OnPropertyChanged(nameof(IsVisibleIsUpdate));
            }
        }
    }
}