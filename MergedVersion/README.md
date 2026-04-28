# MovieApp Unified Repository (UBB-SE-2026-925-1)

This project is a unified movie management and social platform created by merging two distinct repositories. It provides features for catalog browsing, event management, movie battles, and user engagement.

## Tech Stack
* [cite_start]**Target Framework:** .NET 8[cite: 12].
* [cite_start]**UI:** WinUI 3 (Windows App SDK)[cite: 69].
* [cite_start]**Backend:** ASP.NET Core Web API[cite: 75].
* [cite_start]**Data Access:** Entity Framework Core (Code-First with Migrations)[cite: 139].
* [cite_start]**Database:** SQL Server[cite: 142].

## Project Structure
* [cite_start]**src/MovieApp.Core:** Domain models (Movies, Users, Events), interfaces, and business logic services[cite: 57].
* [cite_start]**src/MovieApp.Infrastructure:** Data access layer containing the `MovieAppDbContext`, EF Core configurations, and repository implementations[cite: 63].
* [cite_start]**src/MovieApp.WebAPI:** The server-side API that acts as the sole data provider for the application[cite: 75].
* [cite_start]**src/MovieApp.UI:** The WinUI 3 desktop application for end-users[cite: 69].

## Unified Features
* [cite_start]**Catalog & Details:** Browse movies with unified metadata, reviews, and upcoming screenings[cite: 164].
* [cite_start]**Social & Community:** User reviews, threaded comments, badges, and statistics[cite: 164].
* [cite_start]**Events & Gamification:** Screenings, marathons, movie battles with betting, slot machines, and trivia wheels[cite: 164].

## Getting Started
1. [cite_start]Run `dotnet ef database update` in the Infrastructure project to set up the schema[cite: 155].
2. Start the `MovieApp.WebAPI` project.
3. Start the `MovieApp.UI` project.