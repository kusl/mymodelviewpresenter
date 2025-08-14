# MVP Pattern Demo Application

A comprehensive demonstration of the Model-View-Presenter (MVP) pattern using ASP.NET Web Forms and SQLite, implementing SOLID principles and clean architecture practices.

## 🏗️ Architecture Overview

This application demonstrates proper implementation of the MVP pattern with clear separation of concerns:

### 📁 Project Structure

```
MyModelViewPresenter/
├── Core/                           # Domain layer - Business entities and contracts
│   ├── Models/                     # Domain entities
│   │   └── Product.cs              # Product entity with validation
│   ├── Repositories/               # Repository contracts
│   │   └── IProductRepository.cs   # Data access contract
│   └── Services/                   # Business service contracts
│       └── IProductService.cs      # Business logic contract
├── Infrastructure/                 # Infrastructure layer - Data access and services
│   ├── Repositories/               # Repository implementations
│   │   └── ProductRepository.cs    # SQLite data access implementation
│   ├── Services/                   # Service implementations
│   │   └── ProductService.cs       # Business logic implementation
│   └── DependencyInjection/        # DI container
│       └── ServiceContainer.cs     # Simple IoC container
├── Presentation/                   # Presentation layer - MVP components
│   ├── Views/                      # View contracts
│   │   └── IProductView.cs         # View interface
│   ├── Presenters/                 # Presenter implementations
│   │   └── ProductPresenter.cs     # MVP presenter
│   └── Infrastructure/             # Presentation infrastructure
│       └── PresenterFactory.cs     # Presenter factory
├── Web/                           # Web layer - ASP.NET Web Forms
│   ├── Default.aspx               # Home page
│   ├── ProductManagement.aspx     # Product management page
│   └── Site.Master               # Master page with modern UI
└── Tests/                         # Unit tests
    └── ProductServiceTests.cs     # Service layer tests
```

## 🎯 Design Patterns Implemented

### 1. **Model-View-Presenter (MVP)**
- **Model**: Domain entities (`Product`) and business logic (`IProductService`)
- **View**: ASP.NET Web Forms pages implementing view interfaces (`IProductView`)
- **Presenter**: Orchestrates between View and Model (`ProductPresenter`)

### 2. **Repository Pattern**
- Abstracts data access through `IProductRepository`
- Enables easy testing and database switching
- Implements async operations for better performance

### 3. **Service Layer Pattern**
- Business logic encapsulated in `IProductService`
- Validation and business rules centralized
- Returns structured results with error handling

### 4. **Factory Pattern**
- `PresenterFactory` creates presenters with proper dependencies
- Manages presenter lifecycle
- Supports dependency injection

### 5. **Dependency Injection**
- Simple IoC container (`ServiceContainer`)
- Constructor injection throughout the application
- Enables testability and loose coupling

## 🔧 SOLID Principles Applied

### **S** - Single Responsibility Principle
- Each class has one reason to change
- `ProductService` handles only business logic
- `ProductRepository` handles only data access
- `ProductPresenter` handles only presentation logic

### **O** - Open/Closed Principle
- Interfaces allow extension without modification
- New repositories can be added implementing `IProductRepository`
- New services can implement `IProductService`

### **L** - Liskov Substitution Principle
- All implementations can replace their interfaces
- `ProductRepository` can be substituted with any `IProductRepository`
- Mock objects used in tests demonstrate this principle

### **I** - Interface Segregation Principle
- Interfaces are focused and cohesive
- `IProductView` contains only view-related operations
- `IProductRepository` contains only data operations

### **D** - Dependency Inversion Principle
- High-level modules don't depend on low-level modules
- Presenters depend on service abstractions
- Services depend on repository abstractions
- Dependencies are injected through constructors

## 🚀 Features

### **Functional Features**
- ✅ Create, Read, Update, Delete (CRUD) products
- ✅ Search products by name and description
- ✅ Form validation with custom business rules
- ✅ Responsive modern UI with dark/light theme
- ✅ Real-time feedback with loading states
- ✅ SQLite database with sample data

### **Technical Features**
- ✅ Async/await throughout the application
- ✅ Proper error handling and logging
- ✅ Input validation and sanitization
- ✅ Unit testing with mocking
- ✅ Dependency injection
- ✅ Clean architecture with separation of concerns

## 🛠️ Technology Stack

- **Framework**: .NET Framework 4.8
- **Web**: ASP.NET Web Forms
- **Database**: SQLite with Dapper ORM
- **UI**: Bootstrap 5 with custom styling
- **Testing**: MSTest with Moq
- **Icons**: Bootstrap Icons

## 📋 Prerequisites

- Visual Studio 2019 or later
- .NET Framework 4.8
- IIS Express (included with Visual Studio)

## 🚀 Getting Started

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd MyModelViewPresenter
   ```

2. **Open in Visual Studio**
   - Open `MyModelViewPresenter.sln`

3. **Build the solution**
   - Build → Build Solution (Ctrl+Shift+B)

4. **Run the application**
   - Set `Web` as startup project
   - Press F5 to run

5. **Database Setup**
   - Database is created automatically on first run
   - Sample data is inserted automatically
   - Database location: `Web/App_Data/products.db`

## 🧪 Running Tests

1. **Open Test Explorer**
   - Test → Test Explorer

2. **Run All Tests**
   - Click "Run All" in Test Explorer
   - Tests demonstrate proper mocking and isolation

## 📊 Key Benefits

### **For Developers**
- **Testability**: Easy unit testing with mocked dependencies
- **Maintainability**: Clear separation of concerns
- **Extensibility**: New features can be added easily
- **Readability**: Well-organized code with clear responsibilities

### **For Business**
- **Reliability**: Comprehensive validation and error handling
- **Performance**: Async operations and optimized database access
- **User Experience**: Modern, responsive interface
- **Scalability**: Clean architecture supports growth

## 📚 Learning Objectives

This application demonstrates:

1. **MVP Pattern Implementation**
   - Proper separation of View, Model, and Presenter
   - Event-driven communication between layers

2. **SOLID Principles in Practice**
   - How to apply each principle in real code
   - Benefits of following these principles

3. **Clean Architecture**
   - Dependency flow from outer to inner layers
   - Interface-based design for testability

4. **Modern Web Development**
   - Responsive design principles
   - Progressive enhancement
   - Accessibility considerations

5. **Best Practices**
   - Error handling strategies
   - Validation patterns
   - Testing approaches

## 🔍 Code Quality Features

- **Comprehensive Documentation**: XML comments on all public members
- **Error Handling**: Structured error handling with user-friendly messages
- **Validation**: Both client-side and server-side validation
- **Logging**: Debug logging for troubleshooting
- **Resource Management**: Proper disposal of resources
- **Performance**: Async operations and efficient database queries

## 🎨 UI/UX Features

- **Modern Design**: Clean, professional interface
- **Responsive Layout**: Works on desktop and mobile devices
- **Dark/Light Theme**: User preference support
- **Loading States**: Visual feedback during operations
- **Form Validation**: Real-time validation feedback
- **Accessibility**: Proper ARIA labels and keyboard navigation

## 🚀 Future Enhancements

Potential improvements for learning purposes:

- **Authentication & Authorization**: User management system
- **Caching**: In-memory or Redis caching
- **Logging Framework**: NLog or Serilog integration
- **API Layer**: REST API for external access
- **Advanced Validation**: FluentValidation library
- **Database Migrations**: Structured database versioning
- **Performance Monitoring**: Application insights
- **Containerization**: Docker support

## 📖 Additional Resources

- [MVP Pattern Explained](https://docs.microsoft.com/en-us/previous-versions/msp-n-p/ff649571(v=pandp.10))
- [SOLID Principles](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/architectural-principles)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)

---

This application serves as a comprehensive reference for implementing clean architecture principles in .NET applications, demonstrating both theoretical concepts and practical implementation details.

LLM Notice: This project contains code generated by Large Language Models such as Claude and Gemini. All code is experimental whether explicitly stated or not.