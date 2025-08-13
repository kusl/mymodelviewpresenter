# MVP Pattern Demo - Clean Architecture

A demonstration of the Model-View-Presenter pattern using ASP.NET Web Forms with proper separation of concerns and clean architecture principles.

## Project Structure

```
MyModelViewPresenter/
├── .github/
│   └── workflows/
│       └── build.yml                    # CI/CD pipeline
├── documentation/                       # Project documentation
├── source/
│   └── MyModelViewPresenter/
│       ├── Core/                        # Domain Layer (Interfaces & Models)
│       │   ├── Models/
│       │   │   └── Product.cs
│       │   ├── Repositories/
│       │   │   └── IProductRepository.cs
│       │   ├── Services/
│       │   │   └── IProductService.cs
│       │   ├── Properties/
│       │   │   └── AssemblyInfo.cs
│       │   ├── bin/ & obj/              # Build outputs
│       │   └── Core.csproj
│       ├── Infrastructure/              # Data Access Layer
│       │   ├── Repositories/
│       │   │   └── ProductRepository.cs
│       │   ├── Properties/
│       │   │   └── AssemblyInfo.cs
│       │   ├── bin/ & obj/              # Build outputs
│       │   ├── packages.config
│       │   └── Infrastructure.csproj
│       ├── Presentation/                # Business Logic Layer
│       │   ├── Infrastructure/
│       │   │   └── PresenterFactory.cs
│       │   ├── Presenters/
│       │   │   └── ProductPresenter.cs
│       │   ├── Views/
│       │   │   └── IProductView.cs
│       │   ├── Properties/
│       │   │   └── AssemblyInfo.cs
│       │   ├── bin/ & obj/              # Build outputs
│       │   └── Presentation.csproj
│       ├── Web/                         # UI Layer
│       │   ├── App_Data/
│       │   ├── Properties/
│       │   │   └── AssemblyInfo.cs
│       │   ├── bin/                     # Build outputs + SQLite native DLLs
│       │   │   ├── x64/
│       │   │   └── x86/
│       │   ├── obj/                     # Build outputs
│       │   ├── Default.aspx
│       │   ├── Default.aspx.cs
│       │   ├── Default.aspx.designer.cs
│       │   ├── ProductManagement.aspx
│       │   ├── ProductManagement.aspx.cs
│       │   ├── ProductManagement.aspx.designer.cs
│       │   ├── Site.Master
│       │   ├── Site.Master.cs
│       │   ├── Site.Master.designer.cs
│       │   ├── Global.asax
│       │   ├── Global.asax.cs
│       │   ├── Web.config
│       │   └── Web.csproj
│       ├── packages/                    # NuGet packages (shared)
│       └── MyModelViewPresenter.sln
└── README.md
```

## Architecture Overview

### Clean Architecture Layers

```
┌─────────────────┐
│   Web (UI)      │ ── References ──┐
└─────────────────┘                 │
┌─────────────────┐                 │
│  Presentation   │ ── References ──┼── Core (Domain)
└─────────────────┘                 │
┌─────────────────┐                 │
│ Infrastructure  │ ── References ──┘
│ (Data Access)   │
└─────────────────┘
```

### Dependency Flow
- **Web** → Presentation + Infrastructure + Core
- **Presentation** → Infrastructure + Core  
- **Infrastructure** → Core only
- **Core** → No dependencies (pure domain)

## Technology Stack

- **.NET Framework 4.8**: Target framework
- **ASP.NET Web Forms**: UI technology with master pages
- **SQLite**: Lightweight database with auto-creation
- **Dapper**: Micro-ORM for data access
- **Bootstrap 5**: Responsive UI framework
- **GitHub Actions**: CI/CD pipeline

## Layer Responsibilities

### Core (Domain Layer)
- **Purpose**: Business entities and contracts
- **Contains**: Models, interfaces (repositories, services)
- **Dependencies**: Framework only
- **Key Files**:
  - `Product.cs` - Domain entity with validation attributes
  - `IProductRepository.cs` - Data access contract
  - `IProductService.cs` - Business logic contract

### Infrastructure (Data Access Layer)
- **Purpose**: Database operations and external services
- **Contains**: Repository implementations, database initialization
- **Dependencies**: Core + Dapper + SQLite
- **Key Features**:
  - Auto-creates SQLite database on first run
  - Populates with sample data
  - Async/await for all operations
  - Soft delete implementation

### Presentation (Business Logic Layer)
- **Purpose**: Business logic orchestration and presenter pattern
- **Contains**: Presenters, view interfaces, factory pattern
- **Dependencies**: Core + Infrastructure
- **Key Features**:
  - Event-driven communication with views
  - Business validation logic
  - Presenter lifecycle management
  - Factory pattern for dependency injection

### Web (UI Layer)
- **Purpose**: User interface and user interactions
- **Contains**: ASPX pages, master pages, web configuration
- **Dependencies**: All layers
- **Key Features**:
  - Implements IProductView interface
  - Bootstrap responsive design
  - AJAX UpdatePanels for smooth UX
  - Form validation with user feedback

## Key Design Patterns

### MVP (Model-View-Presenter)
- **Model**: Product entity and repository interfaces
- **View**: ASPX pages implementing IProductView
- **Presenter**: ProductPresenter orchestrating business logic

### Repository Pattern
- Interface in Core layer (`IProductRepository`)
- Implementation in Infrastructure layer (`ProductRepository`)
- Provides abstraction over data access

### Factory Pattern
- `PresenterFactory` creates and manages presenter instances
- Handles dependency injection manually
- Manages presenter lifecycle

### Event-Driven Architecture
- Views raise events for user actions
- Presenters handle events and update views
- Clean separation of UI and business logic

## Features Demonstrated

### Product Management
- **CRUD Operations**: Create, Read, Update, Delete products
- **Search Functionality**: Search by name or description
- **Validation**: Both client-side and server-side validation
- **Soft Delete**: Products marked inactive instead of deleted

### Database Features
- **Auto-Creation**: SQLite database creates itself on first run
- **Sample Data**: Pre-populated with demo products
- **Indexing**: Optimized queries with database indexes
- **Transactions**: Proper async database operations

### UI/UX Features
- **Responsive Design**: Bootstrap 5 for mobile-friendly interface
- **AJAX Updates**: Smooth partial page updates
- **Loading Indicators**: User feedback during operations
- **Form Validation**: Real-time validation with error display

## Package Dependencies

### Core Project
```xml
<!-- No external packages - pure domain -->
System.ComponentModel.DataAnnotations (for validation attributes)
```

### Infrastructure Project
```xml
Dapper 2.1.66                                          <!-- Micro-ORM -->
System.Data.SQLite.Core 1.0.119.0                     <!-- SQLite provider -->
Stub.System.Data.SQLite.Core.NetFramework 1.0.119.0   <!-- .NET Framework support -->
```

### Presentation Project
```xml
<!-- No external packages - pure business logic -->
```

### Web Project
```xml
Microsoft.AspNet.ScriptManager.MSAjax 5.0.0            <!-- AJAX support -->
Microsoft.AspNet.ScriptManager.WebForms 5.0.0         <!-- Web Forms AJAX -->
Microsoft.AspNet.Web.Optimization 1.1.3               <!-- Bundling/minification -->
bootstrap 5.3.7                                        <!-- UI framework -->
Newtonsoft.Json 13.0.3                                 <!-- JSON serialization -->
```

## Getting Started

### Prerequisites
- Visual Studio 2019+ or VS Code
- .NET Framework 4.8
- IIS Express (included with Visual Studio)

### Running the Application
1. **Clone the repository**
2. **Open solution**: `source/MyModelViewPresenter/MyModelViewPresenter.sln`
3. **Build solution**: Ctrl+Shift+B
4. **Set Web as startup project**
5. **Run**: F5 or Ctrl+F5

### Database
- SQLite database auto-creates at: `Web/App_Data/products.db`
- Sample data automatically populated
- No external database setup required

## Project Configuration

### Web.config Settings
```xml
<appSettings>
  <!-- Optional: Override default database path -->
  <add key="SQLiteDbPath" value="custom/path/products.db" />
</appSettings>
```

### Default Database Location
- **Web Application**: `~/App_Data/products.db`
- **Console/Test**: `{BaseDirectory}/App_Data/products.db`

## CI/CD Pipeline

GitHub Actions workflow validates:
- Build Verification: All projects compile successfully
- Project Structure: Required files and folders exist
- Dependency Check: Project references are valid
- Output Validation: Build artifacts are generated

## Best Practices Demonstrated

### Separation of Concerns
- Each layer has single responsibility
- Clean dependency direction (inward)
- Interface-based design for testability

### SOLID Principles
- **Single Responsibility**: Each class has one reason to change
- **Open/Closed**: Extensible through interfaces
- **Liskov Substitution**: Interface implementations are interchangeable
- **Interface Segregation**: Focused, cohesive interfaces
- **Dependency Inversion**: Depend on abstractions, not concretions

### Error Handling
- Comprehensive exception handling at all layers
- User-friendly error messages in UI
- Proper logging and debugging support

### Performance
- Async/await throughout data access layer
- Connection management with using statements
- Database indexing for query optimization

## Learning Objectives

This demo teaches:
- **MVP Pattern Implementation** in Web Forms
- **Clean Architecture** with proper layer separation
- **Repository Pattern** for data access abstraction
- **Event-Driven Communication** between layers
- **Dependency Management** without IoC container
- **Database Design** with SQLite
- **Modern C# Practices** (async/await, using statements)

## Notes for Production

While this demo shows proper architectural patterns, production applications would typically include:
- **IoC Container** (instead of factory pattern)
- **Service Layer** (business rules and validation)
- **Unit Testing** (comprehensive test coverage)
- **Logging Framework** (structured logging)
- **Configuration Management** (environment-specific settings)
- **Error Handling Middleware** (global exception handling)
- **Security** (authentication, authorization, input validation)

## Architecture Benefits

### Maintainability
- Clear separation makes code easy to understand
- Changes to one layer don't affect others
- Easy to locate and fix issues

### Testability
- Interface-based design enables unit testing
- Each layer can be tested in isolation
- Mock objects can replace dependencies

### Flexibility
- Easy to swap implementations (e.g., SQLite → SQL Server)
- UI changes don't affect business logic
- Business logic changes don't affect data access

### Scalability
- Layers can be deployed separately if needed
- Easy to add new features following established patterns
- Clear extension points through interfaces

---

*This project demonstrates clean architecture principles in a practical, runnable application that showcases the MVP pattern with proper separation of concerns.*

LLM Notice: This project contains code generated by Large Language Models such as Claude and Gemini. All code is experimental whether explicitly stated or not.