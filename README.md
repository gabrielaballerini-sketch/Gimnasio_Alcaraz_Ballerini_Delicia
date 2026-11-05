


#  Gimnasio — Sistema de Gestión (Proyecto Integrador Final)

Sistema web MVC para administrar un gimnasio: socios, planes, membresías, cobros, actividades, clases y control de asistencias.

*Integrantes* Alcaraz · Ballerini · Delicia

*Tecnologias Aplicadas:* ASP.NET Core MVC (.NET 10) · MySQL · Vue 3 (AJAX/JSON) · Web API REST con JWT · Bootstrap

---

##  Descripción general


### El problema
Un gimnasio chico suele llevar socios, cuotas y asistencias en planillas sueltas. Eso genera tres problemas concretos:

1. *No se sabe quién tiene la cuota al día*, así que cualquiera puede entrar sin haber pagado.

2. *Los planes con cantidad limitada de utilizaciones* (por ejemplo, "12 clases por mes") no se pueden controlar a mano.
3. *No hay trazabilidad* de quién cobró, quién anuló un pago ni cuánto se recaudó por período.

### Qué resuelve el sistema
- *Alta y administración de socios, planes, actividades, profesores y clases* (grilla semanal de horarios).
- *Membresías con ciclo de vida de pago:* cada socio tiene una membresía que se renueva. Queda pendiente de pago hasta que se registra un cobro, y recién ahí se habilita el período.
- *Cobros auditados:* cada pago guarda quién lo registró y, si se anula, quién lo anuló. El pago y la habilitación de la membresía se hacen en una *única transacción*.
- *Control de acceso por DNI:* el sistema decide si el socio puede ingresar (membresía vigente, pagada, con utilizaciones disponibles) y registra la asistencia.
- *Reportes para el administrador:* recaudación por período, membresías próximas a vencer, vigentes en una fecha, asistencias por actividad e historial.
- *Portal del socio:* cada socio ve su membresía, sus pagos y su utilización del mes.

### Roles

| Rol | Qué puede hacer |

| *Administrador* | Todo: usuarios, recaudación, historial de asistencias, reportes de membresías, ABM de todo el sistema |

| *Empleado* | Gestión operativa: socios, profesores, membresías, cobros, clases y asistencia |
| *Socio* | Consulta de sus propios datos: plan, membresía, pagos y utilización |

---

##  Diagrama de clases
https://app.diagrams.net/#G1FPVwLtrSQqKW9cfozjx3BES7mHRqP07R#%7B%22pageId%22%3A%22C5RBs43oDa-KdzZeNtuy%22%7D



## Matriz de Evidencias de Requerimientos Mínimos

| # | Requerimiento Mínimo | Estado | Enlace al Código / Controladores | Evidencia (Captura / GIF / Video) |
|---|---|---|---|---|
| **1** | **Modelo de datos: mínimo 4 tablas y relación 1:N** | ✅ Cumplido | `Models/`: [Socio.cs](Models/Socio.cs), [Usuario.cs](Models/Usuario.cs), [Membresia.cs](Models/Membresia.cs), [Pago.cs](Models/Pago.cs), [Clase.cs](Models/Clase.cs), [Asistencia.cs](Models/Asistencia.cs), [Actividad.cs](Models/Actividad.cs), [Plan.cs](Models/Plan.cs), [Profesor.cs](Models/Profesor.cs), [ActividadArchivo.cs](Models/ActividadArchivo.cs). | https://app.diagrams.net/#G1FPVwLtrSQqKW9cfozjx3BES7mHRqP07R#%7B%22pageId%22%3A%22C5RBs43oDa-KdzZeNtuy%22%7D |
| **2** | **Login, Roles y Avatar** | ✅ Cumplido | [CuentaController.cs](Controllers/CuentaController.cs) (Login y Logout), [Usuario.cs](Models/Usuario.cs) (roles), [UsuarioService.cs](Service/UsuarioService.cs) (hash PBKDF2 y gestión de avatar), [EditarPerfil.cshtml](Views/Cuenta/EditarPerfil.cshtml). |  [Perfil con avatar](docs/perfil.png) |
| **3** | **Manejo de Archivos (Extra)** | ✅ Cumplido | [ActividadArchivoApiController.cs](Controllers/API/ActividadArchivoApiController.cs) (subida y eliminación), [ActividadArchivoService.cs](Service/ActividadArchivoService.cs) (validación de extensión, ContentType, tamaño y firma real del archivo), [FirmasArchivoService.cs](Service/FirmasArchivoService.cs). | [Subida de archivos](docs/Boxeo.mp4) |
| **4** | **ABM en Vue.js + AJAX** | ✅ Cumplido | [Socio/Index.cshtml](Views/Socio/Index.cshtml) (Vue 3 y Fetch API: listado, alta, modificación, baja y reactivación), [SocioApiController.cs](Controllers/API/SocioApiController.cs). El mismo patrón se implementa en Planes, Actividades, Profesores, Clases, Membresías y Usuarios. | [ABM de socios](docs/socioEditar.mp4) |
| **5** | **Paginado por Servidor** | ✅ Cumplido | [PaginacionParametrosService.cs](Service/PaginacionParametrosService.cs), [ResultadoPaginado.cs](Models/ResultadoPaginado.cs), [RepositorioSocio.cs](DAO/RepositorioSocio.cs#L381) (SQL LIMIT/OFFSET), [SocioApiController.cs](Controllers/API/SocioApiController.cs#L24). | [Evidencia de paginado](docs/pag=2.png) |
| **6** | **Selección Asíncrona (AJAX)** | ✅ Cumplido | [Membresia/Index.cshtml](Views/Membresia/Index.cshtml#L698) (búsqueda de socios), [SocioApiController.cs](Controllers/API/SocioApiController.cs#L42), [Clase/Index.cshtml](Views/Clase/Index.cshtml#L501) (búsqueda de actividades y profesores), [ActividadApiController.cs](Controllers/API/ActividadApiController.cs#L194), [ProfesorApiController.cs](Controllers/API/ProfesorApiController.cs#L199). | [Búsqueda asíncrona](docs/busqueda.png) |
| **7** | **Web API REST + JWT** | ✅ Cumplido | [Program.cs](Program.cs#L35) (configuración JWT), [CuentaController.cs](Controllers/CuentaController.cs#L96) (emisión del token), [Controllers/API/](Controllers/API/) (endpoints REST protegidos con JWT). | [Petición autenticada con Bearer Token](docs/webTokens.png) |

**Nota:** El proyecto supera los requisitos mínimos solicitados: cuenta con más de cuatro tablas relacionadas, incluye relaciones 1:N y N:M, y utiliza autenticación JWT para proteger los endpoints de la Web API.






## Cómo ejecutar el proyecto

### Requisitos
- [.NET SDK 10](https://dotnet.microsoft.com/download)
- MySQL 8 (o compatible) corriendo en localhost:3306

### Pasos

1. *Clonar el repositorio*
   bash
   git clone https://github.com/gabrielaballerini-sketch/Gimnasio_Alcaraz_Ballerini_Delicia.git
   cd Gimnasio_Alcaraz_Ballerini_Delicia
   

2. *Crear la base de datos y cargar los datos de prueba*
   bash
   mysql -u root -p < DB/gimnasio_db.sql
   
   El script crea el esquema gimnasio con sus tablas y los datos de prueba (usuarios de cada rol, socios, planes, membresías, pagos, actividades y clases).

3. *Configurar appsettings.json*
   json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Port=3306;Database=gimnasio;User=root;Password=TU_PASSWORD;"
     },
     "Jwt": {
       "Key": "UNA_CLAVE_LARGA_DE_AL_MENOS_32_CARACTERES",
       "Issuer": "TuApp",
       "Audience": "TuApp",
       "ExpiraMinutos": 30
     },
     "Salt": "Gimnasio_Alcaraz_Ballerini_Delicia_Salt_2026"
   }
   
   

4. *Ejecutar*
   bash
   dotnet run
   
   La aplicación queda disponible en https://localhost:7101 (o http://localhost:5153).

---

##  Credenciales de usuarios de prueba

| Rol | Email | Contraseña |
|---|---|---|
| *Administrador* | gabriela@gmail.com | 123456 |
| *Empleado* | jose456@gmail.com | 123456 |
| *Socio* | romina@gmail.com | 123456 |

---



##  Autenticación: cookies + JWT

La aplicación usa dos esquemas a la vez, cada uno para lo que mejor resuelve:

- *Cookie* → protege las *páginas MVC*. Se genera en el login con los claims del usuario (id, email, rol).
- *JWT* → protege la *API*. Las vistas Vue piden un token a /Cuenta/Token (solo si ya hay sesión) y lo mandan como Bearer en cada fetch.

Las contraseñas *nunca se guardan en texto plano*: se almacena un hash PBKDF2 con salt, y al hacer login se hashea lo ingresado y se compara con el guardado.