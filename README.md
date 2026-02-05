# Sistema de Facturación e Inventario - Ecuador

Sistema completo de facturación electrónica e inventario diseñado para cumplir con las normativas del SRI (Servicio de Rentas Internas) de Ecuador.

## 🚀 Características Principales

- ✅ **Facturación Electrónica SRI**: Generación de XML, firma electrónica, y autorización automática
- 📦 **Gestión de Inventario**: Multi-bodega, seguimiento en tiempo real, alertas de stock
- 👥 **Multi-empresa**: Arquitectura multi-tenant para gestionar múltiples empresas
- 🔐 **Control de Acceso**: Sistema de roles y permisos (RBAC)
- 📊 **Reportes**: Ventas, inventario, reportes tributarios (IVA, retenciones)
- 💾 **Modo Offline**: Cola de autorización para operar sin internet
- 🔍 **Auditoría Completa**: Registro de todas las operaciones

## 📋 Documentos SRI Soportados

- Facturas Electrónicas
- Notas de Crédito
- Notas de Débito
- Guías de Remisión
- Comprobantes de Retención

## 🛠️ Stack Tecnológico

### Backend
- Node.js + Express
- TypeScript
- PostgreSQL
- JWT Authentication

### Frontend
- Vue.js 3
- TypeScript
- Pinia (State Management)
- Vue Router
- Vite

### Infrastructure
- Docker & Docker Compose
- PostgreSQL 15

## 📁 Estructura del Proyecto

```
ai-vibe-coding.project/
├── backend/                 # API REST (Node.js + Express + TypeScript)
│   ├── src/
│   │   ├── config/         # Configuraciones (DB, Logger)
│   │   ├── controllers/    # Controladores HTTP
│   │   ├── services/       # Lógica de negocio
│   │   ├── models/         # Modelos de datos
│   │   ├── routes/         # Rutas de la API
│   │   ├── middleware/     # Middlewares (auth, errors)
│   │   ├── types/          # Tipos TypeScript
│   │   └── utils/          # Utilidades
│   ├── Dockerfile
│   └── package.json
├── frontend/                # Aplicación web (Vue.js)
│   ├── src/
│   │   ├── views/          # Vistas/Páginas
│   │   ├── components/     # Componentes reutilizables
│   │   ├── stores/         # Pinia stores
│   │   ├── router/         # Configuración de rutas
│   │   ├── api/            # Cliente API
│   │   └── types/          # Tipos TypeScript
│   ├── Dockerfile
│   └── package.json
├── database/
│   └── migrations/         # Scripts SQL de migración
├── docs/
│   ├── database-schema.md  # Esquema completo de la BD
│   └── ...
├── .github/
│   └── copilot-instructions.md
├── docker-compose.yml
└── README.md
```

## 🚀 Inicio Rápido

### Prerrequisitos

- Node.js 20+
- Docker & Docker Compose
- Git

### 1. Clonar el Repositorio

```bash
git clone <repository-url>
cd ai-vibe-coding.project
```

### 2. Configurar Variables de Entorno

**Backend:**
```bash
cd backend
cp .env.example .env
# Editar .env con tus configuraciones
```

**Frontend:**
```bash
cd frontend
cp .env.example .env
# Editar .env con tus configuraciones
```

### 3. Iniciar con Docker (Recomendado)

```bash
# Desde la raíz del proyecto
docker-compose up -d
```

Esto iniciará:
- PostgreSQL en `localhost:5432`
- Backend API en `http://localhost:3000`
- Frontend en `http://localhost:5173`

### 4. Instalación Manual (Sin Docker)

**Backend:**
```bash
cd backend
npm install
cp .env.example .env
npm run dev
```

**Frontend:**
```bash
cd frontend
npm install
cp .env.example .env
npm run dev
```

**PostgreSQL:**
- Crea una base de datos llamada `billing_inventory`
- Ejecuta los scripts de migración en `database/migrations/`

## 📊 Base de Datos

Esquema completo documentado en [docs/database-schema.md](docs/database-schema.md)

### Migraciones

```bash
# Aplicar migraciones (TODO: implementar)
cd backend
npm run migrate
```

### Datos de Prueba

```bash
# Cargar datos de prueba (TODO: implementar)
cd backend
npm run seed
```

## 🔐 Autenticación

El sistema usa JWT para autenticación. Endpoints principales:

- `POST /api/v1/auth/login` - Iniciar sesión
- `POST /api/v1/auth/register` - Registrar usuario
- `POST /api/v1/auth/logout` - Cerrar sesión
- `GET /api/v1/auth/me` - Obtener usuario actual

## 📝 API Endpoints (Planificados)

```
/api/v1/
├── auth/           # Autenticación
├── companies/      # Empresas
├── users/          # Usuarios
├── invoices/       # Facturas
├── products/       # Productos
├── inventory/      # Inventario
├── customers/      # Clientes
├── reports/        # Reportes
└── sri/            # Integración SRI
```

## 🧪 Testing

```bash
# Backend tests
cd backend
npm test

# Frontend tests
cd frontend
npm test
```

## 🚢 Deployment

### Producción con Docker

```bash
docker-compose -f docker-compose.prod.yml up -d
```

### Variables de Entorno para Producción

- Cambiar `NODE_ENV=production`
- Usar contraseñas seguras
- Configurar `SRI_ENVIRONMENT=2` (producción)
- Actualizar URLs de servicios SRI
- Configurar certificados digitales

## 📖 Documentación Adicional

- [Esquema de Base de Datos](docs/database-schema.md)
- [Instrucciones para Copilot](.github/copilot-instructions.md)

## 🔧 Configuración SRI

Para utilizar facturación electrónica:

1. Obtener certificado de firma electrónica (.p12)
2. Registrarse en el SRI
3. Configurar ambiente (pruebas/producción)
4. Colocar certificado en `backend/storage/certificates/`
5. Actualizar configuración en `.env`

### URLs SRI

**Ambiente de Pruebas:**
- Recepción: `https://celdes.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline?wsdl`
- Autorización: `https://celdes.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline?wsdl`

**Ambiente de Producción:**
- Recepción: `https://cel.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline?wsdl`
- Autorización: `https://cel.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline?wsdl`

## 🤝 Contribuir

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📄 Licencia

ISC

## 📧 Soporte

Para soporte y preguntas, contactar a [tu-email@example.com]

---

**Nota:** Este proyecto está en desarrollo activo. Algunas características pueden no estar completamente implementadas.
