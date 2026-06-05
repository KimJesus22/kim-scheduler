# Roadmap - Kim Scheduler

Roadmap de desarrollo para Kim Scheduler, un SaaS de gestión de citas para pequeños negocios locales.

## v0.1.0 - Estructura inicial

Estado: completado.

Incluye:

* Monorepo inicial.
* Backend con ASP.NET Core Web API.
* Arquitectura por capas:

  * Api
  * Application
  * Domain
  * Infrastructure
* Entidades iniciales:

  * Business
  * Service
  * BusinessHour
  * Appointment
* Configuración inicial de EF Core.
* MariaDB con Docker Compose.
* Migración inicial.
* Health check de base de datos.
* Release inicial en GitHub.

## v0.2.0 - Autenticación

Estado: completado.

Incluye:

* Registro público de usuarios.
* Primer usuario registrado como Admin.
* Usuarios siguientes registrados como Staff.
* Login con email y contraseña.
* Password hashing.
* JWT.
* Validación de JWT con JwtBearer.
* Endpoint protegido `GET /api/auth/me`.
* Roles básicos:

  * Admin
  * Staff
* Protección de endpoints administrativos:

  * `POST /api/businesses`
  * `PATCH /api/businesses/{id}/deactivate`
  * `POST /api/services`
* Frontend:

  * `/register`
  * `/login`
  * `/playground`
* Almacenamiento temporal del JWT en `localStorage`.
* Cierre de sesión.
* Protección visual de `/playground`.

## v0.3.0 - Appointments Core

Estado: siguiente versión.

Objetivo:

Crear el núcleo funcional de citas.

Incluye planeado:

* Gestión de horarios por negocio.
* Crear horarios laborales.
* Listar horarios por negocio.
* Marcar días como cerrados.
* Crear citas.
* Listar citas por negocio.
* Cancelar citas.
* Validar que una cita esté dentro del horario laboral.
* Validar que no existan citas encimadas.
* Calcular duración de la cita usando la duración del servicio.

## v0.4.0 - Admin Dashboard

Estado: planeado.

Objetivo:

Crear el primer panel administrativo real para gestionar el SaaS sin depender del playground técnico.

Incluye planeado:

- Dashboard administrativo protegido.
- Layout base para `/admin`.
- Resumen general del negocio.
- Estadísticas básicas:
  - total de citas
  - citas pendientes
  - citas confirmadas
  - citas canceladas
  - servicios activos
  - negocios activos
- Gestión visual de citas.
- Gestión básica de clientes.
- Navegación administrativa.
- Separación progresiva entre playground técnico y panel real.

Pendiente para versiones futuras:

- Gráficas avanzadas.
- Reportes de ingresos.
- Clientes frecuentes.
- Permisos finos por negocio.
- Panel multi-negocio avanzado.

## v0.5.0 - Public Booking

Estado: planeado.

Incluye planeado:

* Página pública para reservar cita.
* Ruta por negocio usando slug.
* Visualización de servicios públicos.
* Selección de fecha y hora.
* Confirmación básica de cita.

## Futuro

Ideas para versiones posteriores:

* Integración con Telegram.
* Recordatorios automáticos.
* Notificaciones por email.
* Integraciones con IA.
* Multi-tenant más avanzado.
* Asociar usuarios a negocios.
* Permisos finos por negocio.
* Deploy en Render y Vercel.
