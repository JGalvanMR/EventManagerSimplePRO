# 📦 Nombre del Módulo: EventManager (EventManagerSimple)

---

## 🧭 Propósito

Sistema de gestión de eventos empresariales para Windows (WinForms / .NET 9) que automatiza el ciclo completo de organización de eventos internos: carga masiva de invitados desde Excel, generación de códigos QR individuales, asignación proporcional de boletos de rifa según antigüedad laboral, envío de invitaciones por correo electrónico, y ejecución de sorteos aleatorios.

---

## ⚙️ Responsabilidades

- Cargar y procesar archivos Excel con múltiples hojas, donde cada hoja representa una unidad de negocio
- Calcular automáticamente los años de servicio de cada invitado a partir de su fecha de ingreso
- Asignar boletos de rifa proporcionalmente basándose en los años de servicio calculados
- Generar códigos QR únicos por invitado, firmados con hash SHA-256
- Generar y guardar invitaciones en formato HTML con el QR embebido como base64
- Enviar invitaciones individualmente o en lote mediante SMTP configurable
- Ejecutar sorteos aleatorios aplicando el algoritmo Fisher-Yates sobre el pool de boletos
- Exportar datos a Excel, CSV, HTML y JSON
- Decodificar códigos QR desde imágenes en carpeta local y exportar resultados como JSON

---

## 🔄 Flujo de Funcionamiento

1. **Carga de Archivo**
   El usuario selecciona un archivo `.xlsx`/`.xls`/`.csv`. `ExcelService.ProcesarArchivo()` itera cada hoja del libro, mapea columnas por nombre (`ID`, `NOMBRE`, `INGRESO`, `CORREO`) y crea objetos `Invitado` por fila válida. Cada hoja genera un objeto `UnidadNegocio`.

2. **Generación de QR y Boletos**
   Por cada invitado, `TicketService.GenerarNumerosBoletos()` produce `N` números únicos con formato `EMP-{ID:00000}-{seq:000}-{aleatorio6}`. Simultáneamente, `QRService.GenerarDatosQR()` serializa un JSON con ID, nombre, unidad, timestamp y hash SHA-256, luego `QRService.GenerarQRCode()` codifica ese JSON como imagen PNG. La imagen se guarda en disco y se almacena como `byte[]` en el objeto `Invitado`.

3. **Visualización**
   La pestaña "Invitados" construye dinámicamente un `TabControl` con un `DataGridView` por unidad de negocio, vinculado directamente a `List<Invitado>`.

4. **Generación de Invitaciones HTML**
   `EmailGenerator.GuardarInvitacionComoArchivo()` (en `EventManager.Services`) genera el HTML con la imagen QR embebida en base64 y lo persiste como archivo `.html`, organizando subcarpetas por unidad de negocio.

5. **Envío de Correo**
   Se configura un `SmtpClient` con los parámetros ingresados en la UI. El envío individual llama a `client.SendMailAsync()`. El envío masivo procesa en lotes con delay configurable (default 1500 ms entre emails, pausa de 5 s cada 10 envíos) y registra resultados en un CSV de log.

6. **Sistema de Rifa**
   Se construye un pool plano con todos los números de boletos de todos los invitados. Al preparar la rifa se aplica Fisher-Yates sobre ese pool. Al realizar el sorteo se ejecuta nuevamente Fisher-Yates y se extraen los `N` ganadores solicitados.

7. **Exportación**
   Soporta exportación a Excel (ClosedXML), CSV de boletos, HTML con QRs embebidos y JSON de resultados QR decodificados.

---

## 📐 Reglas de Negocio

### 🔒 Restricciones

- El archivo Excel **debe** contener columnas con exactamente los nombres: `ID`, `NOMBRE`, `INGRESO`, `CORREO` (insensible a mayúsculas). Si falta alguna, la hoja se descarta y se muestra error.
- No se procesan filas con `ID ≤ 0`, `NOMBRE` vacío, `CORREO` sin `@` o `.`, o `INGRESO` en formato no reconocible.
- El correo electrónico del destinatario **debe** estar presente para el envío; si está vacío se lanza excepción.
- Un invitado con **0 años de servicio** recibe **0 boletos** y no participa en la rifa.
- Los códigos QR son personales e intransferibles (restricción declarada en el HTML generado, sin enforcement técnico en la aplicación).
- El sistema de rifa requiere que se haya cargado al menos un archivo Excel antes de operar.

### ✅ Validaciones

- Fecha de ingreso aceptada en formatos: `dd/MM/yyyy`, `d/M/yyyy`, `dd-MM-yyyy`, `d-M-yyyy`, `yyyy-MM-dd`, con fallback a `DateTime.TryParse` genérico.
- El correo se valida únicamente mediante presencia de `@` y `.` (sin validación RFC-5321).
- La configuración SMTP se valida verificando que `SmtpServer`, `SenderEmail`, `Username` y `Password` no sean nulos o vacíos antes de cualquier envío.
- El sorteo valida que el pool de boletos no esté vacío antes de ejecutar.

### 🔁 Agrupaciones

- Cada **hoja del libro Excel** = una **Unidad de Negocio** independiente.
- Las estadísticas de totales (`TotalInvitados`, `TotalBoletos`) se calculan como propiedades derivadas de `List<Invitado>` en `UnidadNegocio`.
- Los boletos de rifa son un **pool global** que se construye concatenando todos los boletos de todas las unidades.

### ⚙️ Reglas Operativas

| Años de Servicio | Boletos Asignados |
|---|---|
| 0 años | 0 boletos |
| 1 – 5 años | 1 boleto |
| 6 – 10 años | 2 boletos |
| 11+ años | 3 boletos |

> **Nota crítica**: Esta tabla está **hard-codeada** en `Invitado.Boletos` mediante un `switch` expression. Las propiedades `Boletos1a5Anios`, `Boletos6a10Anios`, `Boletos11PlusAnios` de `EventoConfig` son **persistidas en JSON pero ignoradas** en el cálculo real.

- El cálculo de años de servicio ajusta correctamente si el aniversario aún no ha ocurrido en el año en curso.
- El formato de número de boleto es: `EMP-{ID:00000}-{sequencia:000}-{6 chars alfanuméricos sin 0/O/I/1}`.
- El hash en el QR se genera como Base64(SHA-256(`{ID}|{Nombre}|{FechaIngreso:yyyyMMdd}|{Ticks}`)), truncado a 16 caracteres.
- El envío masivo incluye reintentos automáticos (máx. 2) con backoff exponencial para errores `MailboxBusy`, `MailboxUnavailable` y `ServiceNotAvailable`.
- El log de envíos se persiste como CSV semicolon-delimited en `{StartupPath}/logs/email_log.csv`.
- Se mantienen únicamente los últimos 5 backups generados por `BackupService`.
- El sorteo requiere confirmación del usuario y la cantidad de ganadores está limitada a `min(50, totalBoletos)`.

---

## 🔗 Dependencias

| Componente | Tipo | Versión/Detalles |
|---|---|---|
| `ClosedXML` | NuGet | v0.102.1 — Lectura y escritura de archivos Excel |
| `QRCoder` | NuGet | v1.4.3 — Generación de códigos QR como PNG |
| `Newtonsoft.Json` | NuGet | v13.0.3 — Serialización de configuración y datos QR |
| `System.Drawing.Common` | NuGet | v7.0.0 — Manipulación de imágenes |
| `ZXing.Net` | NuGet | v0.16.11 — Decodificación de QR desde imágenes |
| `ZXing.Net.Bindings.Windows.Compatibility` | NuGet | v0.16.10 — Binding Windows para ZXing |
| `System.Text.Encoding.CodePages` | NuGet | v10.0.0 — Soporte de encodings adicionales |
| `System.Net.Mail.SmtpClient` | BCL | .NET 9 — Envío de correos SMTP |
| Archivo Excel (externo) | Archivo | Estructura fija con columnas ID/NOMBRE/INGRESO/CORREO |
| Archivo `email_config.json` | Archivo local | Configuración SMTP persistida en `Application.StartupPath` |
| Archivo `config.json` | Archivo local | Configuración del evento en `Application.StartupPath` |

---

## ⚠️ Riesgos Técnicos

**1. Configuración de tickets desacoplada del modelo**
`EventoConfig` expone `Boletos1a5Anios/6a10/11Plus` que el usuario puede configurar y persistir, pero `Invitado.Boletos` los ignora completamente. Esto crea una expectativa falsa de configurabilidad y puede generar confusión operativa o bugs al modificar los valores en la UI de configuración.

**2. Clase `EmailGenerators` compilada pero inaccesible**
`EmailGenerators.cs` (namespace `EventManager`, clase `EmailGenerators` plural) compila correctamente pero `Form1` nunca la instancia. Contiene una plantilla HTML diferente a la que se usa realmente, con el logo de la empresa hard-codeado como base64 directamente en el código fuente. Es código muerto que induce a confusión.

**3. Contraseña SMTP almacenada en texto plano**
`email_config.json` persiste la contraseña SMTP sin ningún tipo de cifrado en `Application.StartupPath`. Cualquier usuario con acceso al sistema de archivos puede leerla.

**4. Validación de email trivial**
La verificación de correo electrónico (`Contains("@") && Contains(".")`) no detecta direcciones malformadas como `@.`, `a@b`, o con espacios. Esto puede resultar en fallos de envío no capturados en la fase de carga.

**5. `Application.DoEvents()` en bucle de UI**
`BtnGenerarHTML_Click` llama `Application.DoEvents()` dentro del loop de generación de HTML, lo que puede provocar re-entradas al evento si el usuario interactúa durante el proceso.

**6. Instanciación de `Random` sin semilla**
`Form1` usa `new Random()` para el sorteo. En .NET moderno esto usa un seed aleatorio apropiado, pero el pool se mezcla dos veces con instancias distintas, lo que no aporta mayor aleatoriedad y dificulta la auditabilidad del sorteo.

**7. Fuga de imagen en `MostrarQR`**
`pictureBox.Image = Image.FromStream(ms)` dentro de un `using` block puede llevar a que la imagen sea desreferenciada antes de ser renderizada, o que la imagen anterior no sea liberada al cerrar el formulario modal.

**8. Archivos excluidos del build con código incompleto**
`BitmapLuminanceSource.cs` está excluido de compilación y su implementación está incompleta (faltan los miembros abstractos de `LuminanceSource`). Igualmente `GeneradorEmailOG.cs` y `GeneradorEmailOG2.cs` contienen duplicados de `EmailGenerator` que causarían conflicto de nombre si se reincorporan.

**9. Hora del evento hard-codeada**
El HTML de `EmailGenerators.cs` tiene `15:00 horas` hard-codeado. El HTML de `Services/EmailGenerator.cs` tiene `19:00 horas` hard-codeado (y también `18:30` para registro). Ninguno de estos valores es configurable desde la UI o el modelo.

**10. Lugar del evento en dos fuentes**
El lugar aparece como valor por defecto en `EventoConfig` (`"Salón La Gran Vía - DoubleTree by Hilton Celaya"`) y también hard-codeado en el HTML de `EmailGenerators.cs`. Si el usuario cambia el lugar en configuración, el HTML de `EmailGenerators` no lo reflejará.

---

## 🧪 Casos Edge

- **Invitado recién ingresado (hoy)**: `AniosServicio` devuelve 0, asigna 0 boletos; el invitado aparece en lista pero no participa en rifa.
- **Invitado con fecha de ingreso futura**: `Math.Max(0, years)` evita valores negativos, retorna 0 boletos.
- **Hoja de Excel sin filas de datos** (solo encabezados): Se crea `UnidadNegocio` con lista vacía, no causa error pero tampoco se muestra en la UI.
- **Todos los invitados ya con invitación enviada + filtro activo**: `BtnEnviarIndividual_Click` muestra mensaje "Todos los invitados ya han recibido su invitación" y cancela.
- **Sorteo con menos boletos que ganadores solicitados**: El máximo se limita a `totalBoletos`, pero la lógica de selección mediante Fisher-Yates puede extraer menos elementos de los esperados si el shuffle termina antes del límite.
- **Archivo Excel con nombres de columna en minúsculas o mixtos**: `ObtenerIndicesColumnas` usa `StringComparer.OrdinalIgnoreCase`, por lo que acepta `id`, `Nombre`, `CORREO`, etc.
- **Correo SMTP con error 5.7.x (autenticación)**: Se captura y relanza como `EmailException` con mensaje amigable; no reintenta (solo reintenta errores transitorios de buzón).
- **QR decodificado desde imagen sin código QR**: `lector.Decode()` retorna `null`, se loguea "No se encontró código QR" y continúa sin agregar al resultado JSON.
- **Nombre de invitado con caracteres inválidos para nombre de archivo**: `SanitizeFileName()` reemplaza caracteres con `_`, pero nombres extremadamente largos no se truncan.

---

## 🧱 Suposiciones Detectadas

- Se asume que el archivo Excel siempre tiene la primera fila como encabezado de columnas.
- Se asume que cada hoja del Excel representa exactamente una unidad de negocio válida con el nombre de la hoja como identificador.
- Se asume que el servidor SMTP configurado soporta STARTTLS en puerto 587 (configuración por defecto).
- Se asume que `Application.StartupPath` es escribible para almacenar configuración, logs, QRs y reportes.
- Se asume que los QR se generarán y almacenarán antes de cualquier intento de envío de correo.
- Se asume que el campo `Lugar` en el HTML final siempre provendrá de `EventoConfig.Lugar`, cuando en realidad `EmailGenerators.cs` lo ignora.
- Se asume que los boletos de rifa asignados seguirán siempre la proporción 1/2/3, independientemente de lo configurado en `EventoConfig`.
- Se asume que los números de boleto generados por `TicketService` son suficientemente únicos (usa `new Random()` sin pool compartido, puede colisionar bajo carga).

---

## 📈 Recomendaciones Técnicas

1. **Sincronizar la lógica de boletos con la configuración**: Modificar `Invitado.Boletos` para leer los valores desde `EventoConfig` inyectado, o eliminar las propiedades `Boletos*Anios` de `EventoConfig` para evitar la expectativa falsa de configurabilidad.

2. **Eliminar o integrar `EmailGenerators.cs`**: Decidir si la plantilla HTML con logo corporativo es la definitiva y reemplazar completamente a `Services/EmailGenerator.cs`, o descartarla. El código muerto debe eliminarse del proyecto.

3. **Cifrar credenciales SMTP**: Usar `System.Security.Cryptography.DataProtectionScope.CurrentUser` (DPAPI) para cifrar la contraseña antes de persistirla en JSON.

4. **Extraer valores hard-codeados a configuración**: La hora del evento (`15:00`/`19:00`), código de vestimenta (`Formal / Etiqueta`) y plazo de confirmación deben ser campos en `EventoConfig`.

5. **Mejorar validación de email**: Incorporar una expresión regular RFC-5321 básica o usar `System.Net.Mail.MailAddress` con try/catch para validar antes de agregar al listado de envíos.

6. **Reemplazar `Application.DoEvents()` con `async/await`**: Convertir `BtnGenerarHTML_Click` a método asíncrono usando `Task.Run()` para el procesamiento en background, reportando progreso mediante `IProgress<T>`.

7. **Gestionar correctamente el ciclo de vida de imágenes**: Almacenar la imagen anterior del `PictureBox` antes de asignar una nueva y llamar `Dispose()` en el formulario QR al cerrarse.

8. **Implementar `RandomNumberGenerator` para el sorteo**: Reemplazar `new Random()` con `RandomNumberGenerator.GetInt32()` para garantizar aleatoriedad criptográficamente segura y auditable.

9. **Completar o eliminar `BitmapLuminanceSource.cs`**: La clase hereda de `LuminanceSource` sin implementar sus miembros abstractos. Debe completarse o eliminarse del proyecto; su exclusión del build no es una solución sostenible.

10. **Centralizar la creación de nombres de archivo**: Extraer la lógica de sanitización y formateo de nombres a una clase utilitaria compartida entre `EmailGenerators.cs` y `Services/EmailGenerator.cs`.

---

## 🧾 Resumen Ejecutivo

EventManager es una aplicación de escritorio para Windows diseñada para gestionar eventos internos de empresa de principio a fin. Permite al equipo organizador cargar una lista de colaboradores desde Excel, donde el sistema calcula automáticamente cuántos boletos de rifa le corresponden a cada persona según sus años de servicio: un boleto para quienes llevan entre 1 y 5 años, dos boletos para quienes llevan entre 6 y 10 años, y tres boletos para quienes llevan más de 11 años. Con esa información, genera un código QR único para cada persona y produce una invitación digital personalizada que puede enviarse por correo electrónico o guardarse como archivo HTML. Al final del evento, la misma herramienta permite ejecutar el sorteo de rifa de forma aleatoria y justa, identificando a los ganadores por nombre y unidad de negocio.

El sistema funciona completamente offline para la generación de contenido y solo requiere conexión a internet para el envío de correos. Es apto para eventos de hasta varios cientos de colaboradores, aunque presenta limitaciones técnicas que deben resolverse antes de llevarse a un entorno de producción, principalmente relacionadas con la seguridad de credenciales y la consistencia entre las opciones de configuración visibles al usuario y el comportamiento real del sistema.
