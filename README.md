# 📅 Kim Scheduler

> **SaaS premium de gestión y reserva de citas diseñado para pequeños negocios locales (barberías, estéticas, consultorios médicos y más).** 
>
> Una solución moderna, robusta, altamente escalable y responsive que simplifica la interacción entre los comercios locales y sus clientes.

---

## 🌟 Características Destacadas

*   **Multitenant Ready:** Arquitectura preparada para soportar múltiples negocios independientes con identidades personalizadas mediante slugs únicos.
*   **Arquitectura Limpia (Clean Architecture):** Separación estricta de responsabilidades que garantiza mantenibilidad, testeabilidad y desacoplamiento.
*   **Diseño de Base de Datos Optimizado:** Índices compuestos, relaciones eficientes y precisión exacta para precios y horarios de atención.
*   **Developer & Recruiter Friendly:** Código limpio, tipado fuerte con C# .NET 10 y TypeScript, y despliegue rápido automatizado con Docker.

---

## 🛠️ Stack Tecnológico

### Backend (Core de Alto Rendimiento)
*   **Framework:** `.NET 10.0 Web API` (última tecnología de Microsoft)
*   **ORM:** `Entity Framework Core 9` (patrón Code-First)
*   **Base de Datos:** `MariaDB 11.4` (motor rápido de grado de producción)
*   **Proveedor BD:** `Pomelo.EntityFrameworkCore.MySql 9.0.0`
*   **Documentación:** `Swagger / OpenAPI` integrada para pruebas rápidas de API

### Frontend (Experiencia de Usuario Premium)
*   **Framework:** `Astro` (máximo rendimiento e islas de interactividad)
*   **Estilos:** `Tailwind CSS` (diseño premium, adaptativo y moderno)
*   **Lenguaje:** `TypeScript` (tipado estricto extremo a extremo)

### Infraestructura y Despliegue
*   **Contenedores:** `Docker` & `Docker Compose`
*   **Control de Versiones:** `Git` con convenciones de Git Flow y Conventional Commits.

---

## 📐 Arquitectura del Backend

El proyecto implementa una **Arquitectura Limpia Simplificada por Capas**, asegurando que las reglas de negocio sean completamente independientes de los frameworks y detalles de infraestructura.

```text
    ┌──────────────────────────────────────────────┐
    │           SaaS.Appointments.Api              │  ◄── Capa de Presentación (Controllers, HTTP)
    └──────────────────────┬───────────────────────┘
                           │
                           ▼
    ┌──────────────────────────────────────────────┐
    │     SaaS.Appointments.Infrastructure         │  ◄── EF Core, MariaDB, Migraciones, DI
    └──────────────────────┬───────────────────────┘
                           │
                           ▼
    ┌──────────────────────────────────────────────┐
    │      SaaS.Appointments.Application           │  ◄── Casos de Uso, Servicios y DTOs (Próximamente)
    └──────────────────────┬───────────────────────┘
                           │
                           ▼
    ┌──────────────────────────────────────────────┐
    │         SaaS.Appointments.Domain             │  ◄── Entidades Puras, Enums y Reglas de Negocio
    └──────────────────────────────────────────────┘
```

### Roles por Proyecto

1.  **Api:** Punto de entrada. Expone los endpoints HTTP y maneja la configuración inicial (`Program.cs` y `appsettings.json`).
2.  **Application:** Contiene los casos de uso, lógica de aplicación e interfaces.
3.  **Infrastructure:** Implementa el acceso a datos (`AppDbContext`), las migraciones de base de datos y la inyección de dependencias (`DependencyInjection.cs`).
4.  **Domain:** El corazón del sistema. Entidades del negocio puras (`Business`, `Service`, `BusinessHour`, `Appointment`) libres de dependencias de terceros.

---

## 🔗 Endpoints de la API

La API cuenta con endpoints de salud de infraestructura y la base del módulo de negocios:

| Método | Endpoint | Descripción | Estado |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/health/database` | Comprueba el estado de la conexión a MariaDB |  Activo |
| **POST** | `/api/businesses` | Registra un nuevo negocio local (Nombre, Slug, etc.) |  Activo |
| **GET** | `/api/businesses` | Lista todos los negocios registrados |  Activo |
| **GET** | `/api/businesses/{id}` | Obtiene los detalles de un negocio por su ID |  Activo |

---

## 🚀 Guía de Inicio Rápido (Desarrollo)

Sigue estos sencillos pasos para levantar el entorno de desarrollo local en minutos:

### 1. Requisitos Previos
*   [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
*   [Docker Desktop](https://www.docker.com/products/docker-desktop/) (debe estar corriendo)
*   [Git](https://git-scm.com/)

### 2. Clonar y Configurar Infraestructura
Clona el repositorio e inicia el contenedor de la base de datos MariaDB:

```powershell
# Iniciar contenedor de base de datos
docker compose up -d
```

### 3. Aplicar Migraciones de EF Core
EF Core generará automáticamente el esquema de tablas en MariaDB:

```powershell
# Moverse al directorio del backend
cd backend

# Aplicar las migraciones a la base de datos
dotnet ef database update --project src/SaaS.Appointments.Infrastructure --startup-project src/SaaS.Appointments.Api
```

### 4. Ejecutar la API
Inicia el servidor de desarrollo del backend:

```powershell
# Ejecutar el proyecto Api
dotnet run --project src/SaaS.Appointments.Api
```

*   **Swagger UI:** Una vez iniciado el servidor, accede a la documentación interactiva en: `http://localhost:5083/swagger` (o el puerto asignado en la consola).

---

## 📅 Próximos Pasos (Roadmap)

- [ ]  Implementación de Autenticación mediante **JWT (JSON Web Tokens)**.
- [ ]  Creación de Servicios de Aplicación y Casos de Uso del Backend.
- [ ]  Módulos CRUD para `Service`, `BusinessHour` y `Appointment`.
- [ ]  Validaciones avanzadas de negocio utilizando **FluentValidation**.
- [ ]  Desarrollo del Frontend interactivo en **Astro + Tailwind CSS**.