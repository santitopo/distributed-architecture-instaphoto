# ProgRedesNajsonTopolansky

InstaPhoto is a distributed system, composed of five applications
- A server in which photo data and comments related to them must be saved
- A client for said server that will be in charge of supplying the data and files to the server via a CLI
- Supplementary infrastructure to allow the administration and auditing of the system:
  - A Logs server where log registries from the main server can be published and read from
  - An Administrative server with root access to the ABM of InstaPhoto users
  - An Administrative client that allows to interact with the Admin server and with the Logs server through a CLI.

Done using:
- .NET Core (ASP NetCore)
- RabbitMQ
- gRPC protocol
- TCP connections using WebSockets

## Main challenges:
💡 Parallel client sessions (using threads)  
💡 Publisher-Subscriber pattern using a queues provider  
💡 Interface exposure for gRPC interaction


## High-level architecture:
![Arquitectura Obligatorio](https://github.com/santitopo/ProgRedesNajsonTopolansky/assets/43559181/030cf548-702d-4f4d-ae21-4e5550077b77)

Submitted during the course Networks Programming
