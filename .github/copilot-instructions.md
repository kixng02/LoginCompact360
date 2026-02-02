# AI Coding Agent Instructions for LoginCompact 360

## Project Overview
LoginCompact 360 is a Blazor Server application serving as the frontend for a credential stuffing detection system. It handles user authentication (login/signup) while the backend API (running separately on port 7000) manages security analysis, PostgreSQL logging, and alerting.

## Architecture
- **Frontend**: Blazor Server with Interactive Server render mode
- **UI Framework**: BlazorStrap (Bootstrap components) - prefer `<BS*>` components over raw HTML
- **Authentication**: JWT-based via HttpClient calls to backend API
- **Structure**:
  - `Components/Pages/`: Razor pages (e.g., Login.razor uses static HTML, Signup.razor uses Blazor forms)
  - `Services/`: Dependency injection for business logic (IAuthService interface with AuthService implementation)
  - `Models/`: Data transfer objects with DataAnnotations validation

## Key Patterns
- **Forms**: Use `EditForm` with `DataAnnotationsValidator` and `ValidationSummary` for client-side validation
- **API Calls**: HttpClient injected services make async calls to `https://localhost:7000/` endpoints
- **Navigation**: `NavigationManager` for programmatic routing
- **Styling**: Bootstrap classes via BlazorStrap components, custom CSS in `.razor.css` files

## Development Workflow
- **Run**: `dotnet run` launches on `https://localhost:7000` (matches backend API URL)
- **Backend**: Separate .NET Core API project expected on port 7000 for auth endpoints
- **Build**: Standard .NET build process, no custom scripts
- **Debug**: Use VS Code debugger for Blazor Server debugging

## Conventions
- **File Naming**: PascalCase for classes/components, kebab-case for routes
- **Async/Await**: All service methods are async, use `Task.Delay` for simulation in development
- **Error Handling**: Try-catch in services with logging, throw user-friendly exceptions
- **Validation**: Model-level validation with `[Required]`, `[StringLength]`, `[EmailAddress]` attributes

## Common Tasks
- **Add New Page**: Create in `Components/Pages/`, add `@page` directive, use `@layout` for auth pages
- **API Integration**: Add methods to `IAuthService`, implement in `AuthService` with HttpClient
- **UI Components**: Use BlazorStrap (e.g., `<BSButton>`, `<BSInput>`) instead of raw Bootstrap
- **State Management**: Services for shared state, inject via `@inject`

## References
- `Program.cs`: DI setup, HttpClient configuration
- `Signup.razor`: Example of complete Blazor form with validation
- `IAuthService.cs`: Service interface pattern
- `README.md`: Full system architecture and backend details</content>
<parameter name="filePath">/Users/masi/Desktop/LC/LC360/.github/copilot-instructions.md