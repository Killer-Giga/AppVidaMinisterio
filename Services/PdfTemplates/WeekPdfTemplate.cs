using AppVidaMinisterio.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using IContainer = QuestPDF.Infrastructure.IContainer;
using Colors = QuestPDF.Infrastructure.Color;
using System.Reflection;

namespace AppVidaMinisterio.Services.PdfTemplates
{
    internal class WeekPdfTemplate : IDocument
    {
        // Plantilla del pdf para una semana.
        public Semana Week { get; set; }

        public WeekPdfTemplate(Semana week)
        {
            Week = week;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Padding(10).BorderBottom(1).Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
           {
               row.RelativeItem().Column(column =>
               {
                   column.Item().Padding(5).Text(Week.SemanaFecha).FontSize(20).Bold().AlignCenter().FontColor(Colors.FromHex("#000000"));
                   column.Item().Padding(5).Text(Week.TextoBiblico).FontSize(15).Bold().AlignCenter();
                   column.Item().Text("").FontSize(10);
               });

           });
        }

        void ComposeContent(IContainer container)
        {
            container.Table(table =>
             {
                 table.ColumnsDefinition(columns =>
                 {
                     columns.RelativeColumn(2);
                     columns.RelativeColumn(1);
                 });

                 table.Cell().Element(CellStyle).Text(Week.Cancion.Inicio);
                 table.Cell().Element(CellStyle).Text($"{Week.Presidente}\n{Week.Orador.Inicial}");

                 table.Cell().ColumnSpan(2).Background(Colors.FromHex("#FF3C7F8B")).Padding(4).Text(text =>
                 {
                     text.Element().Padding(-4).PaddingBottom(-2).PaddingRight(15).Height(20).Image(GetImageBytes("tesoros_embedded_resource.png"));
                     text.Span("TESOROS DE LA BIBLIA").FontColor("ffffff");
                 });
                 table.Cell().ColumnSpan(2).Padding(-4).Text("");


                 table.Cell().Element(CellStyle).Text($"{Week.Tesoros.TituloTesoros}\n(10 mins.)");
                 table.Cell().Element(CellStyle).Text($"{Week.Tesoros.OradorTesoros}");
                 table.Cell().Element(CellStyle).Text("2. Busquemos perlas escondidas\n(10 mins.)");
                 table.Cell().Element(CellStyle).Text($"{Week.Tesoros.ConductorPerlas}");
                 table.Cell().Element(CellStyle).Text($"3. Lectura de la Biblia\n(10 mins.) {Week.Tesoros.DetallesLecturaBiblica}");
                 table.Cell().Element(CellStyle).Text($"{Week.Tesoros.LectorBiblia}");

                 table.Cell().ColumnSpan(2).Padding(-4).Text("");
                 table.Cell().ColumnSpan(2).Background(Colors.FromHex("#FFD68F00")).Padding(4).Text(text =>
                 {
                     text.Element().Padding(-4).PaddingBottom(-1).PaddingRight(15).Height(20).Image(GetImageBytes("smm_embedded_resource.png"));
                     text.Span("SEAMOS MEJORES MAESTROS").FontColor("ffffff");
                 });
                 table.Cell().ColumnSpan(2).Padding(-4).Text("");

                 table.Cell().Element(CellStyle).Text($"{Week.MejoresMaestros.Asignacion1}\n{Week.MejoresMaestros.Detalles1}");
                 table.Cell().Element(CellStyle).Text(ChangeString(Week.MejoresMaestros.Asignado1));
                 table.Cell().Element(CellStyle).Text($"{Week.MejoresMaestros.Asignacion2}\n{Week.MejoresMaestros.Detalles2}");
                 table.Cell().Element(CellStyle).Text(ChangeString(Week.MejoresMaestros.Asignado2));
                 if (!string.IsNullOrEmpty(Week.MejoresMaestros.Asignacion3))
                 {
                     table.Cell().Element(CellStyle).Text($"{Week.MejoresMaestros.Asignacion3}\n{Week.MejoresMaestros.Detalles3}");
                     table.Cell().Element(CellStyle).Text(ChangeString(Week.MejoresMaestros.Asignado3));
                 }
                 if (!string.IsNullOrEmpty(Week.MejoresMaestros.Asignacion4))
                 {
                     table.Cell().Element(CellStyle).Text($"{Week.MejoresMaestros.Asignacion4}\n{Week.MejoresMaestros.Detalles4}");
                     table.Cell().Element(CellStyle).Text(ChangeString(Week.MejoresMaestros.Asignado4));
                 }

                 table.Cell().ColumnSpan(2).Padding(-4).Text("");
                 table.Cell().ColumnSpan(2).Background(Colors.FromHex("#FFBF2F13")).Padding(4).Text(text =>
                 {
                     text.Element().PaddingLeft(2).Padding(-4).PaddingBottom(1).PaddingRight(15).Height(20).Image(GetImageBytes("nvc_embedded_resource.png"));
                     text.Span("NUESTRA VIDA CRISTIANA").FontColor("ffffff");
                 });
                 table.Cell().ColumnSpan(2).Element(CellStyle).Text(Week.Cancion.Intermedio);
                 table.Cell().ColumnSpan(2).Padding(-4).Text("");

                 table.Cell().Element(CellStyle).Text($"{Week.VidaCristiana.Asignacion1}\n{Week.VidaCristiana.Detalles1}");
                 table.Cell().Element(CellStyle).Text($"{Week.VidaCristiana.Asignado1}");
                 if (!string.IsNullOrEmpty(Week.VidaCristiana.Asignacion2))
                 {
                     table.Cell().Element(CellStyle).Text($"{Week.VidaCristiana.Asignacion2}\n{Week.VidaCristiana.Detalles2}");
                     table.Cell().Element(CellStyle).Text($"{Week.VidaCristiana.Asignado2}");
                 }
                 table.Cell().Element(CellStyle).Text($"{Week.VidaCristiana.EstudioBiblico}\n{Week.VidaCristiana.DetallesEstudioBiblico}");
                 table.Cell().Element(CellStyle).Text(ChangeString(Week.VidaCristiana.ConductorYLectorEstudioBiblico));

                 static IContainer CellStyle(IContainer container)
                     => container.Padding(5);
                 
                 string ChangeString(string? str)
                 {
                     string newString = "";
                     if (str != null)
                     {
                         if (str.Contains("\r"))
                             newString = str.Replace("\r", "\n");
                         else
                             return str;
                     }
                         return newString;
                 }
             });
        }

        void ComposeFooter(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem(2).Column(column =>
                {
                    column.Item().Text(Week.Cancion.Final);
                });
                row.RelativeItem(1).Column(column =>
                {
                    column.Item().Text(Week.Orador.Final);
                });
            });
        }

        public byte[] GetImageBytes(string imageName)
        {
            // Se utiliza este metodo para que se puedan hacer cambios faciles con QuestPdfCompanion porque si no se convierten a bytes manda error al acutalizar
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"AppVidaMinisterio.Resources.Images.{imageName}";

            // Obtener el recurso incrustado como Stream
            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException($"Recurso no encontrado: {resourceName}");
                }

                // Leer los bytes del Stream
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }
    }
}

