# Appointment Management System

## Author

Arvin Jayson Castro
Senior Full-Stack Engineer
https://arvinjaysoncastro.com
https://linkedin.com/in/arvinjaysoncastro

## Overview

This project demonstrates an Appointment Management System built using Clean Architecture principles in .NET.

The purpose of this project is to demonstrate proper separation of concerns between the Domain, Application, Infrastructure, and API layers.

## Architecture

The solution follows Clean Architecture principles.

Dependency Direction:

API → Application → Domain
Infrastructure → Application → Domain

The Domain layer has no dependencies on other layers and represents the core business model.

* **Domain**
  Contains core business entities and rules.

* **Application**
  Contains use cases, services, and repository contracts.

* **Infrastructure**
  Implements data access and external services.

* **API**
  Exposes the application via ASP.NET Core Web API.

## Architectural Principles

The system follows the Dependency Rule of Clean Architecture:
source code dependencies can only point inward.

Business logic in the Domain and Application layers remains independent from infrastructure concerns such as databases, frameworks, or external services.

## Technologies

* .NET
* ASP.NET Core Web API
* Clean Architecture
* Dependency Injection
