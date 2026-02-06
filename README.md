# SaaS Billing & Inventory Management System

A modern, multi-tenant SaaS application for billing and inventory management built with .NET 8, PostgreSQL, Nuxt 3, and PrimeVue.

## 🏗️ Architecture

- **Frontend**: Nuxt 3 + TypeScript + PrimeVue (Teal theme)
- **Backend**: .NET 8 + Entity Framework Core + PostgreSQL
- **Infrastructure**: Docker + Docker Compose
- **Multi-tenant**: Schema-based tenant isolation

## 📁 Project Structure

```
ai-vibe-coding.project/
├── docs/                           # Agent documentation
│   ├── backend-agent.md
│   ├── frontend-agent.md
│   └── project-architecture-agent.md
├── frontend/                       # Nuxt 3 frontend application
│   ├── assets/
│   ├── components/
│   ├── composables/
│   ├── layouts/
│   ├── locales/
│   ├── middleware/
│   ├── pages/
│   ├── plugins/
│   ├── stores/
│   ├── types/
│   ├── Dockerfile
│   ├── Dockerfile.dev
│   └── package.json
├── backend/                        # .NET 8 backend (to be created)
├── docker-compose.yml              # Development environment
├── docker-compose.prod.yml         # Production environment
└── AGENTS.md                       # Agent system documentation
```

## 🚀 Quick Start

### Prerequisites

- **Node.js** 20+
- **Docker** & **Docker Compose**
- **.NET 8 SDK** (for backend development)
- **PostgreSQL** (or use Docker)

### Frontend Setup

1. **Navigate to frontend directory**:
```bash
cd frontend
```

2. **Install dependencies**:
```bash
npm install
```

3. **Configure environment**:
```bash
cp .env.example .env
```

4. **Run development server**:
```bash
npm run dev
```

Frontend will be available at `http://localhost:3000`

### Docker Development (Recommended)

Start the entire stack with Docker:

```bash
# Start development environment with hot reload
docker-compose up

# Stop environment
docker-compose down
```

### Docker Production

Build and run optimized production containers:

```bash
# Build and start production environment
docker-compose -f docker-compose.prod.yml up --build -d

# Stop production environment
docker-compose -f docker-compose.prod.yml down
```

## ✨ Features

### Frontend ✅ (Completed)

- ✅ Nuxt 3 with TypeScript
- ✅ PrimeVue 4+ with Teal theme
- ✅ Dark/Light mode support
- ✅ Multi-language (EN, ES, FR, DE)
- ✅ Multi-tenant architecture
- ✅ JWT authentication setup
- ✅ Pinia state management
- ✅ Tailwind CSS integration
- ✅ Responsive design
- ✅ Docker containerization

### Backend 🚧 (Planned)

- 🚧 .NET 8 Web API
- 🚧 Entity Framework Core
- 🚧 PostgreSQL database
- 🚧 Multi-tenant (schema-based)
- 🚧 CQRS with MediatR
- 🚧 JWT authentication
- 🚧 Swagger/OpenAPI
- 🚧 Docker containerization

### Billing Module 📋 (Planned)

- Invoice management
- Customer management
- Payment processing
- Subscription management
- Reports and analytics

### Inventory Module 📦 (Planned)

- Product catalog
- Warehouse management
- Stock tracking
- Purchase orders
- Stock movements

## 🎨 UI/UX

### Theme
- **Primary Color**: Teal
- **Themes**: Lara Light Teal / Lara Dark Teal
- **Components**: PrimeVue default components
- **Layout**: Tailwind CSS utilities

### Supported Languages
- 🇺🇸 English (en)
- 🇪🇸 Spanish (es)
- 🇫🇷 French (fr)
- 🇩🇪 German (de)

## 🔐 Authentication & Security

- JWT-based authentication
- Refresh token rotation
- Multi-tenant isolation
- Role-based access control
- Secure headers and CORS

## 📚 Documentation

### Agent System

This project uses specialized AI agents for development:

- **Project Architecture Agent** - System design and architecture
- **Backend Agent** - .NET 8 backend development
- **Frontend Agent** - Nuxt 3 frontend development

See [AGENTS.md](AGENTS.md) for details.

### Component Documentation

- [Frontend README](frontend/README.md) - Detailed frontend documentation
- Backend README (coming soon)

## 🛠️ Development

### Frontend Development

```bash
cd frontend

# Install dependencies
npm install

# Run dev server
npm run dev

# Build for production
npm run build

# Type check
npm run typecheck

# Lint
npm run lint
```

### Backend Development

Coming soon...

## 📦 Deployment

### Using Docker Compose (Production)

```bash
# Build and start all services
docker-compose -f docker-compose.prod.yml up --build -d

# View logs
docker-compose -f docker-compose.prod.yml logs -f

# Stop all services
docker-compose -f docker-compose.prod.yml down
```

### Environment Variables

**Frontend (.env)**:
```
NUXT_PUBLIC_API_BASE=http://localhost:5000/api/v1
```

**Backend (.env)** (coming soon):
```
DATABASE_CONNECTION_STRING=...
JWT_SECRET=...
```

## 🧪 Testing

Coming soon...

## 📄 License

MIT

## 👥 Team

SaaS Billing & Inventory Development Team

## 🤝 Contributing

1. Follow the agent-based development approach
2. Maintain TypeScript strict mode
3. Use PrimeVue components without heavy customization
4. Write internationalized content
5. Test in both light and dark modes
6. Ensure multi-tenant isolation

## 📞 Support

For issues and questions, please open an issue in the repository.
