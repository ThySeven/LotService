# LotService

LotService is a microservice responsible for managing auction lots within our Auction Core Services architecture. It handles everything from the creation and assignment of lots to sending notifications regarding lot status changes. This service is built to integrate seamlessly with other essential services including User Service, Invoice Service, and external systems.

## Table of Contents

- [Setup](#setup)
- [Configuration](#configuration)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Endpoints](#endpoints)
- [Security](#security)
- [Monitoring & Logging](#monitoring--logging)
- [Continuous Deployment](#continuous-deployment)
- [License](#license)

## Setup

### Prerequisites

- Docker
- Docker Compose
- Nginx (for reverse proxy)
- HashiCorp Vault
- RabbitMQ
- Prometheus, Loki, and Grafana for monitoring

### Local Installation

1. Clone the repository:

    ```bash
    git clone https://github.com/yourusername/LotService.git
    cd LotService
    ```

2. Run the service using Docker Compose:

    ```bash
    docker-compose up --build
    ```

### Production Setup

For a production setup, ensure that you have configured Nginx, Vault, RabbitMQ, and monitoring systems accurately. You can deploy the service using your CI/CD pipeline.

## Configuration

### Environment Variables

- `LokiEndpoint`: Endpoint for Loki.
- `RabbitMQHostName`: Hostname for the RabbitMQ server.
- `VAULT_IP`: Address of the HashiCorp Vault service.
- `VAULT_SECRET`: Secret for accessing Vault.
- `MongoDBConnectionString`: Connection string for MongoDB.
- `GrafanaHostname`: Hostname for Grafana (default is `lotservice`).
- `RabbitMQQueueName`: Queue name for RabbitMQ (default is `BiddingQueue`).
- `UserServiceEndpoint`: Endpoint for the User Service (default is `userservice:3015`).
- `InvoiceServiceEndpoint`: Endpoint for the Invoice Service (default is `invoiceservice:3005`).
- `BiddingServiceEndpoint`: Endpoint for the Bidding Service (default is `biddingservice:3020`).
- `NginxEndpoint`: Endpoint for Nginx (default is `nginx:3001`).
- `PublicIP`: Public IP address (default is `http://localhost:3025`).
- `ASPNETCORE_URLS`: URL for the ASP.NET Core service (default is `http://+:3025`).

Rename the file to `.env` and fill in the necessary values.

## Architecture

LotService follows a microservice architecture within the broader Auction Core Services ecosystem. It interacts closely with the following components:

- **User Service**: For user-related data and verification.
- **Invoice Service**: To send data for invoicing.
- **RabbitMQ**: For sending notifications and updates.
- **Nginx**: For managing API traffic and load balancing.

![Architecture Diagram](https://s.icepanel.io/mB4kr95xX1FRKO/CgWk)

## Dependencies

- MongoDB
- RabbitMQ
- HashiCorp Vault
- UserService
- InvoiceService

## Endpoints

### API Endpoints

| Method | Endpoint           | Description                               |
|--------|--------------------|-------------------------------------------|
| GET    | /lots              | Retrieve all lots                         |
| POST   | /lots              | Create a new lot                          |
| GET    | /lots/:id          | Get details of a specific lot             |
| PUT    | /lots/:id          | Update an existing lot                    |
| DELETE | /lots/:id          | Delete a specific lot                     |
| PUT    | /lots/close/:id    | Closes an existing lot                    |


## Security

LotService uses HashiCorp Vault to manage secrets securely. JWT secrets are stored in Vault and fetched dynamically, ensuring that sensitive information is handled with care.
