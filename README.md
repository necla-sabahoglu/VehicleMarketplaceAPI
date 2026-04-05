# 🚗 Vehicle Marketplace API

A scalable backend project built with .NET to simulate a real-world vehicle marketplace system.

---

## 🚀 Features

- User authentication (JWT)
- Vehicle listing & filtering
- Favorites system
- Redis caching for performance
- RabbitMQ for event-driven communication
- Background jobs for scheduled tasks

---

## 🏗️ Architecture

- Clean Architecture (Domain, Application, Infrastructure, API)
- Repository Pattern
- Service Layer

---

## 🧰 Technologies

- .NET 8 Web API
- MSSQL
- Redis (Caching)
- RabbitMQ (Messaging)
- Docker

---

## ⚙️ How to Run

1. Clone the repository
2.	Run Docker:
docker-compose up -d
3.	Run the API:
dotnet run
4.	Access Swagger:
http://localhost:5000/swagger

---

## 📌 Key Concepts Implemented

- Caching strategy with Redis
- Event-driven architecture using RabbitMQ
- Layered architecture design
- Background job processing

---

## 📷 Architecture Diagram

```mermaid
flowchart LR

%% Client Layer
Client["Client (Postman / UI)"]

%% API Layer
API["API Layer - Controllers"]

%% Application Layer
App["Application Layer - Services / Use Cases"]

%% Infrastructure Layer
Repo["Repository Layer"]
Cache["Redis Cache"]
MQ["RabbitMQ"]

%% Data Layer
DB[("MSSQL Database")]
Consumer["Background Jobs / Consumers"]
Done(("End"))

%% Flow
Client -->|HTTP Request| API
API --> App

App -->|Cache First| Cache
Cache -->|Hit| App
App -->|Miss| Repo

Repo --> DB

App -->|Publish Event| MQ
MQ --> Consumer
Consumer --> DB
DB --> Done

```

---

## 💡 Author
Necla Sabahoğlu
