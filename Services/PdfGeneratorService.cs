using AppVidaMinisterio.Models;
using AppVidaMinisterio.Services.PdfTemplates;
using QuestPDF.Companion;

namespace AppVidaMinisterio.Services
{
    internal class PdfGeneratorService
    {
        // Esta clase manejara la logica en la creacion de un archivo PDF
        // Donde guardar el pdf
        // Enviar el pdf
        // Decidir cuantas semanas se mostrarán en el pdf
        WeekPdfTemplate document;
        public PdfGeneratorService(Semana week)
        {
            document = new WeekPdfTemplate(week);

#if DEBUG
            ShowInCompanionA();
#endif
            //document.GeneratePdfAndShow();
        }

        public async Task ShowInCompanionA()
        {
            // Tengo que usar Async pues ShowInCompanion no funciona
            await document.ShowInCompanionAsync();
        }
    }
}
