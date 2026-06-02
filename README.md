# Kim Scheduler

SaaS de gestión de citas para pequeños negocios locales como barberías, estéticas, consultorios y negocios similares.

## Stack tecnológico

### Frontend

- Astro
- Tailwind CSS
- TypeScript

### Backend

- ASP.NET Core 10 Web API
- Entity Framework Core
- JWT Authentication

### Base de datos

- MariaDB

## Objetivo

Crear una plataforma simple, responsive y escalable para permitir que negocios locales gestionen servicios, horarios y citas.

## Arquitectura

El proyecto usa una arquitectura por capas:

```txt
Api → Endpoints HTTP
Application → Casos de uso
Domain → Entidades y reglas del negocio
Infrastructure → Base de datos, EF Core e integraciones