using AppVidaMinisterio.Models;
using HtmlAgilityPack;

namespace AppVidaMinisterio.Services
{
    internal class WebScrapingService
    {
        // Esta clase llena al objeto Semana con la información obtenida de la url
        private enum Seccion
        {
            Ninguna,
            Tesoros,
            MejoresMaestros,
            VidaCristiana
        }
        private Seccion _seccionActual = Seccion.Ninguna;

        private Semana Semana { get;}
        private string Url { get; }
        private static readonly HttpClient httpClient = new HttpClient();
        private bool _h3Valido = false;
        private bool _firstPNode = false;

        public WebScrapingService(Semana semana, string url)
        {
            Semana = semana;
            Url = url;
        }

        public async Task<bool> HasValidContentAsync()
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(Url);
                response.EnsureSuccessStatusCode();
                string htmlContent = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);
                var h1Node = doc.DocumentNode.SelectSingleNode("//h1");
                if (h1Node !=  null)
                {
                    return true;
                }
                return false;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task WebScraping()
        {
            try
            {
                //Se inicializa el cliente http y se obtiene el contenido de la url
                HttpResponseMessage response = await httpClient.GetAsync(Url);
                response.EnsureSuccessStatusCode();
                string htmlContent = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);

                var h1Node = doc.DocumentNode.SelectSingleNode("//h1");
                var h2h3pNodes = doc.DocumentNode.SelectNodes("//h2 | //h3 | //p");

                if (h1Node == null)
                    return;
                else
                    AsignarFecha(h1Node.InnerText);

                if (h2h3pNodes == null)
                    return;
                else
                    SelectorDeNodo(h2h3pNodes);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Error: Web Scraping", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException("Error: Web Scraping", ex);
            }
        }

        private void SelectorDeNodo(HtmlNodeCollection h2h3pNodes)
        {
            // Itera en el nodo h2 y busca los nodos strong para buscar la sección y asignar las partes correspondientes
            // después itera en los nodos h3 que se encuentren entre los h2 para asignar los puntos de la reunión
            // y selecciona el primer nodo p después del h3 para asignar los detalles como el tiempo e información adicional de cada parte
            int numberH2 = 0;
            foreach (var node in h2h3pNodes)
            {
                if (node.Name == "h2")
                {
                    // busca nodos strong dentro de h2 y define el estado de _seccionActual
                    numberH2++;
                    var strongNodes = node.SelectNodes(".//strong");
                    if (strongNodes != null)
                    {
                        foreach (var strongNode in strongNodes)
                        {
                            var text = strongNode.InnerText;
                            switch (text)
                            {
                                case "TESOROS DE LA BIBLIA":
                                    _seccionActual = Seccion.Tesoros;
                                    break;
                                case "SEAMOS MEJORES MAESTROS":
                                    _seccionActual = Seccion.MejoresMaestros;
                                    break;
                                case "NUESTRA VIDA CRISTIANA":
                                    _seccionActual = Seccion.VidaCristiana;
                                    break;
                                default:
                                    if (numberH2 == 1)
                                    {
                                        AsignarTextoBiblico(text);
                                    }
                                    AsignarParte(node);
                                    break;
                            }
                        }
                    }
                }
                else if (node.Name == "h3")
                {
                    AsignarParte(node);
                    _firstPNode = true;
                }
                else if (node.Name == "p" && _firstPNode && _h3Valido)
                {
                    // Toma el nodo siguiente al h3 y asigna los detalles
                        AsignarDetallesNodoP(node.InnerText.Trim());
                }
            }
        }

        private void AsignarParte(HtmlNode node)
        {
            // itera en el nodo h3 y busca los nodos strong para buscar la parte a asignar en la semana y posteriormente llamar al metodo SeleccionarSeccion
            var strongNodes = node.SelectNodes(".//strong");
            if (_seccionActual == Seccion.VidaCristiana)
            {
                // En ocaciones el titulo se divide en varios strong por eso se tiene que recopilar el texto si es en vida cristiana
                if (strongNodes != null)
                {
                    string textAcumulado = string.Empty;
                    foreach (var strongNode in strongNodes)
                    {
                        var text = strongNode.InnerText.Trim();
                        textAcumulado += text;
                    }
                    SeleccionarSeccion(textAcumulado);
                }
            }
            else
            {
                if (strongNodes != null)
                {
                    foreach (var strongNode in strongNodes)
                    {
                        var text = strongNode.InnerText.Trim();
                        SeleccionarSeccion(text);
                    }
                }
            }
        }

        private void SeleccionarSeccion(string text)
        {
            // Selecciona la sección correspondiente segun el texto del <strong> y asigna el texto a la propiedad correspondiente
            if (text.Contains("Canción"))
                AsignarCancion(text);
            else if (_seccionActual != Seccion.Ninguna)
            {
                switch (_seccionActual)
                {
                    case Seccion.Tesoros:
                        AsignarTesorosDeLaBiblia(text);
                            break;
                    case Seccion.MejoresMaestros:
                        AsignarMejoresMaestros(text);
                        break;
                    case Seccion.VidaCristiana:
                        AsignarVidaCristiana(text);
                        break;
                    default:
                        break;
                }
            }
        }

        private void AsignarTesorosDeLaBiblia(string text)
        {
            if (Semana.Tesoros.TituloTesoros == null)
            {
                Semana.Tesoros.TituloTesoros = text;
            }
            else if (text.StartsWith("3"))
            {
                // Se examina el 3 porque es el numero de la lectura biblica, donde si hay detalles guardados en el nodo p
                _h3Valido = true;
            }
        }

        private void AsignarMejoresMaestros(string text)
        {
            if (Semana.MejoresMaestros.Asignacion1 == null)
                Semana.MejoresMaestros.Asignacion1 = text;
            else if (Semana.MejoresMaestros.Asignacion2 == null)
                Semana.MejoresMaestros.Asignacion2 = text;
            else if (Semana.MejoresMaestros.Asignacion3 == null)
                Semana.MejoresMaestros.Asignacion3 = text;
            else if (Semana.MejoresMaestros.Asignacion4 == null)
                Semana.MejoresMaestros.Asignacion4 = text;
        }

        private void AsignarVidaCristiana(string text)
        {
            // Se asegura que el primer caracter de la cadena sea un número porque todas las asignaciones empiezan con un numero. Hay nodos h3 que no tienen numero y se reemplaza el contenido de la propiedad
            // Si es así, asigna el texto a la propiedad correspondiente. 
            if (text.Contains("Estudio bíblico de la congregación"))
            {
                _seccionActual = Seccion.Ninguna;
                _h3Valido = true;
                Semana.VidaCristiana.EstudioBiblico = text;
            }
            else
            {
                string primerCaracter = text.Substring(0, 1);
                int _;
                if (int.TryParse(primerCaracter, out _))
                {
                    if (Semana.VidaCristiana.Asignacion1 == null)
                        Semana.VidaCristiana.Asignacion1 = text;
                    else if (Semana.VidaCristiana.Asignacion2 == null)
                        Semana.VidaCristiana.Asignacion2 = text;

                    _h3Valido = true;
                }
                else
                {
                    _h3Valido = false;
                }
            }
        }

        private void AsignarCancion(string text)
        {
            if (Semana.Cancion.Inicio == null)
                Semana.Cancion.Inicio = $"{text} y oración";
            else if (Semana.Cancion.Intermedio == null)
                Semana.Cancion.Intermedio = text;
            else
                Semana.Cancion.Final = $"{text} y oración";
        }

        private void AsignarTextoBiblico(string text)
        {
            // Se suma todo el texto que se encuentren en el primer h2 porque este llega a estar dividido entre varios strong
            Semana.TextoBiblico += text;
        }

        private void AsignarFecha(string text)
        {
            Semana.SemanaFecha = text;
        }

        private void AsignarDetallesNodoP(string text)
        {
            switch (_seccionActual)
            {
                case Seccion.Tesoros:
                    _firstPNode = false;
                    if (Semana.Tesoros.DetallesLecturaBiblica == null)
                        Semana.Tesoros.DetallesLecturaBiblica = text;
                    break;

                case Seccion.MejoresMaestros:
                    _firstPNode = false;
                    if (Semana.MejoresMaestros.Detalles1 == null)
                        Semana.MejoresMaestros.Detalles1 = text;
                    else if (Semana.MejoresMaestros.Detalles2 == null)
                        Semana.MejoresMaestros.Detalles2 = text;
                    else if (Semana.MejoresMaestros.Detalles3 == null)
                        Semana.MejoresMaestros.Detalles3 = text;
                    else
                        Semana.MejoresMaestros.Detalles4 = text;
                    break;

                case Seccion.VidaCristiana:
                    _firstPNode = false;
                    if (Semana.VidaCristiana.Detalles1 == null)
                        Semana.VidaCristiana.Detalles1 = text;
                    else if (Semana.VidaCristiana.Detalles2 == null)
                        Semana.VidaCristiana.Detalles2 = text;
                    break;

                case Seccion.Ninguna:
                    _firstPNode = false;
                    _h3Valido = false;
                        Semana.VidaCristiana.DetallesEstudioBiblico = text;
                    break;

                default:
                    break;
            }
        }
    }
}