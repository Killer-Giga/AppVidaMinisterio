using AppVidaMinisterio.Models;
using HtmlAgilityPack;

namespace AppVidaMinisterio.Services
{
    internal class WebScrapingService
    {
        // Es clase llena al objeto Semana con la información obtenida de la url
        private enum Seccion
        {
            Ninguna,
            Tesoros,
            MejoresMaestros,
            VidaCristiana
        }
        private Seccion _seccionActual = Seccion.Ninguna;
        private bool _h3Valido = false;
        private bool _firstPNode = false;

        private static readonly HttpClient httpClient = new HttpClient();

        public async Task WebScrappingService(string url, Semana semana)
        {
            try
            {
                //Se inicializa el cliente http y se obtiene el contenido de la url
                HttpResponseMessage response = await httpClient.GetAsync(url);

                string htmlContent = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);

                var h1Node = doc.DocumentNode.SelectSingleNode("//h1");
                var h2h3pNodes = doc.DocumentNode.SelectNodes("//h2 | //h3 | //p");

                if (h1Node == null)
                {
                    return;
                }
                else
                {
                    AsignarFecha(h1Node.InnerText, semana);
                }

                if (h2h3pNodes == null)
                {
                    return;
                }
                else
                {
                    SelectorDeNodo(h2h3pNodes, semana);
                }
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Error: ", ex);
            }
        }

        private void SelectorDeNodo(HtmlNodeCollection h2h3pNodes, Semana semana)
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
                                        AsignarTextoBiblico(text, semana);
                                    }
                                    AsignarParte(node, semana);
                                    break;
                            }
                        }
                    }
                }
                else if (node.Name == "h3")
                {
                    AsignarParte(node, semana);
                    _firstPNode = true;
                }
                else if (node.Name == "p")
                {
                    // Toma el nodo siguiente al h3 y asigna los detalles
                    if (_seccionActual == Seccion.Tesoros && _firstPNode && _h3Valido)
                        AsignarDetallesNodoP(node.InnerText.Trim(), semana);
                    else if (_seccionActual == Seccion.MejoresMaestros && _firstPNode)
                        AsignarDetallesNodoP(node.InnerText.Trim(), semana);
                    else if (_seccionActual == Seccion.VidaCristiana && _firstPNode && _h3Valido)
                        AsignarDetallesNodoP(node.InnerText.Trim(), semana);
                    else if (_seccionActual == Seccion.Ninguna && _firstPNode && _h3Valido)
                        AsignarDetallesNodoP(node.InnerText.Trim(), semana);
                    else
                        continue;
                }
            }
        }

        private void AsignarParte(HtmlNode node, Semana semana)
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
                    SeleccionarSeccion(textAcumulado, semana);
                }
            }
            else
            {
                if (strongNodes != null)
                {
                    foreach (var strongNode in strongNodes)
                    {
                        var text = strongNode.InnerText.Trim();
                        SeleccionarSeccion(text, semana);
                    }
                }
            }
        }

        private void SeleccionarSeccion(string text, Semana semana)
        {
            // Selecciona la sección correspondiente segun el texto del <strong> y asigna el texto a la propiedad correspondiente
            if (text.Contains("Canción"))
                AsignarCancion(text, semana);
            else if (_seccionActual != Seccion.Ninguna)
            {
                switch (_seccionActual)
                {
                    case Seccion.Tesoros:
                        AsignarTesorosDeLaBiblia(text, semana);
                            break;
                    case Seccion.MejoresMaestros:
                        AsignarMejoresMaestros(text, semana);
                        break;
                    case Seccion.VidaCristiana:
                        AsignarVidaCristiana(text, semana);
                        break;
                    default:
                        break;
                }
            }
        }

        private void AsignarTesorosDeLaBiblia(string text, Semana semana)
        {
            if (semana.TituloTesoros == null)
            {
                semana.TituloTesoros = text;
            }
            else if (text.StartsWith("3"))
            {
                _h3Valido = true;
            }
        }

        private void AsignarMejoresMaestros(string text, Semana semana)
        {
            if (semana.AsignacionMejoresMaestros1 == null)
                semana.AsignacionMejoresMaestros1 = text;
            else if (semana.AsignacionMejoresMaestros2 == null)
                semana.AsignacionMejoresMaestros2 = text;
            else if (semana.AsignacionMejoresMaestros3 == null)
                semana.AsignacionMejoresMaestros3 = text;
            else if (semana.AsignacionMejoresMaestros4 == null)
                semana.AsignacionMejoresMaestros4 = text;
        }

        private void AsignarVidaCristiana(string text, Semana semana)
        {
            // Se asegura que el primer caracter de la cadena sea un número
            // si es así, asigna el texto a la propiedad correspondiente
            if (text.Contains("Estudio bíblico de la congregación"))
            {
                _seccionActual = Seccion.Ninguna;
                _h3Valido = true;
                semana.EstudioBiblico = text;
            }
            else
            {
                string primerCaracter = text.Substring(0, 1);
                int _;
                if (int.TryParse(primerCaracter, out _))
                {
                    if (semana.AsignacionVidaCristiana1 == null)
                        semana.AsignacionVidaCristiana1 = text;
                    else if (semana.AsignacionVidaCristiana2 == null)
                        semana.AsignacionVidaCristiana2 = text;

                    _h3Valido = true;
                }
                else
                {
                    _h3Valido = false;
                }
            }
        }

        private void AsignarCancion(string text, Semana semana)
        {
            if (semana.CancionInicio == null)
                semana.CancionInicio = $"{text} y oración";
            else if (semana.CancionIntermedio == null)
                semana.CancionIntermedio = text;
            else
                semana.CancionFinal = $"{text} y oración";
        }

        private void AsignarTextoBiblico(string text, Semana semana)
        {
            // Se suma todo el texto que se encuentren en el primer h2 porque este llega a estar dividido entre varios strong
            semana.TextoBiblico += text;
        }

        private void AsignarFecha(string text, Semana semana)
        {
            semana.SemanaFecha = text;
        }

        private void AsignarDetallesNodoP(string text, Semana semana)
        {
            switch (_seccionActual)
            {
                case Seccion.Tesoros:
                    _firstPNode = false;
                    if (semana.DetallesLecturaBiblica == null)
                        semana.DetallesLecturaBiblica = text;
                    break;

                case Seccion.MejoresMaestros:
                    _firstPNode = false;
                    if (semana.DetallesAsignacionMejoresMaestros1 == null)
                        semana.DetallesAsignacionMejoresMaestros1 = text;
                    else if (semana.DetallesAsignacionMejoresMaestros2 == null)
                        semana.DetallesAsignacionMejoresMaestros2 = text;
                    else if (semana.DetallesAsignacionMejoresMaestros3 == null)
                        semana.DetallesAsignacionMejoresMaestros3 = text;
                    else
                        semana.DetallesAsignacionMejoresMaestros4 = text;
                    break;

                case Seccion.VidaCristiana:
                    _firstPNode = false;
                    if (semana.DetallesAsignacionVidaCristiana1 == null)
                        semana.DetallesAsignacionVidaCristiana1 = text;
                    else if (semana.DetallesAsignacionVidaCristiana2 == null)
                        semana.DetallesAsignacionVidaCristiana2 = text;
                    break;

                case Seccion.Ninguna:
                    _firstPNode = false;
                    _h3Valido = false;
                        semana.DetallesEstudioBiblico = text;
                    break;

                default:
                    break;
            }
        }
    }
}