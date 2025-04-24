
📋 AppVidaMinisterio

Aplicación .NET MAUI para generar automáticamente el rol de la reunión “Vida y Ministerio” semanal, a partir de datos obtenidos por web-scraping, almacenados en JSON, editables en la UI y exportables a PDF.

🔍 Descripción  
AppVidaMinisterio descarga los datos de la reunión semanal (web-scraping), los guarda en un archivo JSON local, permite al usuario añadir o modificar los participantes de cada semana y finalmente genera un PDF listo para compartir o imprimir.

🚀 Características principales  
- 🔎 **Web Scraping** de la fuente oficial para obtener la información semanal.  
- 💾 **Almacenamiento JSON** en local para persistencia de datos.  
- ➕ **Interfaz MAUI** para agregar, editar y eliminar participantes por semana.  
- 📄 **Generación de PDF** con el rol de la reunión (usa plantillas en `Services/PdfTemplates`).  
- ⚙️ **Configuración** de la URL de origen y opciones de formato en código.

📦 Tecnologías y librerías  
- Lenguaje: C#  
- Framework: .NET MAUI (multi-plataforma móvil y desktop)  
- Web Scraping: HtmlAgilityPack (`Services/WebScrapingService.cs`)  
- JSON: System.Text.Json (`Services/DataStorageService.cs`)  
- PDF: QuestPDF (`Services/PdfGeneratorService.cs`)  
- MVVM: carpetas ViewModels y Views para separación de capas  
- Control de versiones: Git + GitHub

🔧 Requisitos  
- .NET 6.0 SDK o superior con workload .NET MAUI instalado  
- Visual Studio 2022 (u otra IDE con soporte MAUI)  
- Conexión a Internet para el scraping  

⚙️ Instalación y ejecución  

1. **Clonar el repositorio**  
   ```bash
   git clone https://github.com/Killer-Giga/AppVidaMinisterio.git
   cd AppVidaMinisterio
   ```  
2. **Abrir la solución**  
   - En Visual Studio: abre `AppVidaMinisterio.sln`.  
3. **Restaurar paquetes NuGet**  
   - VS lo hará automáticamente al cargar la solución, o manualmente:  
     ```bash
     dotnet restore
     ```  
4. **Configurar (opcional)**  
   - Si necesitas cambiar la URL de scraping, edita `Services/WebScrapingService.cs`.  
5. **Ejecutar**  
   - Selecciona la plataforma (Android, Windows, macOS) y presiona **Run** en Visual Studio, o desde CLI:  
     ```bash
     dotnet build -t:Run -f net6.0-android
     ```  

🗂️ Estructura de carpetas  
```text
AppVidaMinisterio/           citeturn0view0  
├─ Models/                   # Clases de datos (Semana, Participante, etc.)  
├─ Services/                 # Lógica de Scraping, JSON y PDF  
│   ├─ WebScrapingService.cs  
│   ├─ DataStorageService.cs  
│   └─ PdfGeneratorService.cs  
├─ ViewModels/               # Clases MVVM que exponen datos a la UI  
├─ Views/                    # Páginas XAML (.xaml + .xaml.cs)  
├─ Resources/                # Imágenes, estilos y plantillas PDF  
├─ Platforms/                # Código específico de cada plataforma  
├─ App.xaml                  # Definición de recursos globales  
├─ AppShell.xaml             # Navegación de la aplicación  
├─ MauiProgram.cs            # Configuración MAUI  
└─ AppVidaMinisterio.sln      # Solución Visual Studio  
```


 📈 Ejemplos de uso  

> **Descarga de datos**: al iniciar la app pulsa “Descargar Semanas” para scrapear la información de la semana.  
>
> **Edición**: añade o quita participantes desde la lista.  
>
> **Exportar PDF**: pulsa “Generar PDF” y guarda el archivo para imprimir o compartir.


🤝 Cómo contribuir  

1. Haz un **fork** del repositorio.  
2. Crea una rama de funcionalidad:  
   ```bash
   git checkout -b feature/nueva-funcionalidad
   ```  
3. Haz **commit** de tus cambios:  
   ```bash
   git commit -m "Agrega X funcionalidad"
   ```  
4. Envía tu rama a tu fork y abre un **Pull Request**.  

---

📝 Licencia  
Este proyecto está bajo la licencia **MIT**. Consulta el archivo [LICENSE](LICENSE) para más detalles.


📬 Contacto  
- **Autor:** Killer-Giga  
- **Email:** DD-Developer@outlook.com  
- **GitHub:** https://github.com/Killer-Giga  
