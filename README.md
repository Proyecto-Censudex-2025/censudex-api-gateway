## censudex-api-gateway

El **API Gateway** actúa como el **punto de entrada único** para todas las solicitudes externas del sistema Censudex. Su función principal es centralizar la seguridad, enrutar las peticiones a los microservicios internos adecuados y realizar la traducción entre los protocolos **HTTP** y **gRPC**.

Este módulo está compuesto por una aplicación en **ASP.NET Core** (`censudex-api`) que expone los endpoints HTTP y un balanceador de carga **NGINX** que gestiona el tráfico gRPC.
## Arquitectura y Patrón de Diseño

###  Arquitectura del repositorio: Microservicios (Orquestador)

El API Gateway implementa el patrón **API Gateway** y actúa como el **Orquestador** o API MAIN, sin poseer una base de datos propia.

Su responsabilidad es:
* Coordinar las llamadas a los microservicios.
* Asegurar la **autorización y autenticación** de los usuarios.
* Centralizar el **enrutamiento** y la **gestión del tráfico**.

### Patrones de diseño implementados:
1. **API Gateway:** Proporciona un único punto de entrada unificado y centraliza la lógica de seguridad y enrutamiento.
2. **Adapter/Proxy Pattern:** La capa de servicios en C# actúa como un *adapter* al envolver la comunicación gRPC para ser consumida por los controladores HTTP, traduciendo efectivamente los protocolos.
3. **Load Balancing (NGINX):** Se utiliza NGINX para construir un balanceador de cargas que distribuye las peticiones gRPC entre las réplicas de los microservicios.

---

### Tecnologías utilizadas
* **Framework:** ASP.NET Core 9.0.
* **Protocolo de Clientes:** **HTTP** (para clientes externos).
* **Comunicación Interna:** **gRPC** (para microservicios como Inventory) y **HTTP** (para el servicio de Autenticación).
* **Balanceador de Carga/Proxy:** **NGINX**.

### Modelo de Datos
El API Gateway **no posee una Base de Datos** propia.

---

### Endpoints del API Gateway (HTTP)

Los siguientes endpoints son expuestos por el API Gateway (la aplicación C# en el puerto 5114, accesible vía NGINX en 7000 para gRPC):

| Método | Ruta | Servicio Interno | Propósito |
|:-------|:-----|:-----------------|:----------|
| `GET` | `/validate-token` | Auth Service (HTTP) | Validación interna de JWT. |
| `POST` | `/login` | Auth Service (HTTP) | Autenticación de usuario. |
| `POST` | `/logout` | Auth Service (HTTP) | Invalidación de token. |
| `POST` | `/clients` | Clients Service | Crear nuevo cliente. |
| `GET` | `/clients` | Clients Service | Visualizar listado de usuarios. |
| `GET` | `/clients/{id}` | Clients Service | Obtener usuario por ID. |
| `PATCH` | `/clients/{id}` | Clients Service | Editar datos de usuario/Actualizar contraseña. |
| `DELETE` | `/clients/{id}` | Clients Service | Sofy delete de un cliente. |
| `POST` | `/products` | Products Service | Crear producto (Administrador). |
| `GET` | `/products` | Products Service | Visualizar listado de productos. |
| `GET` | `/products/{id}` | Products Service | Obtener producto por ID. |
| `PATCH` | `/products/{id}` | Products Service | Editar datos de producto/Eliminar producto (Soft Delete). |
| `GET` | `/inventory` | Inventory Service | Consultar listado de stock de productos. |
| `GET` | `/inventory/{productId}` | Inventory Service | Consultar stock de producto por ID. |
| `POST` | `/inventory` | Inventory Service | Agregar al inventario un producto. |
| `PATCH` | `/inventory/{productId}/stock` | Inventory Service | **Ajustar la cantidad de stock** (Entradas/Salidas). |
| `PATCH` | `/inventory/{productId}/minimum-stock` | Inventory Service | **Configurar umbral mínimo** de stock. |
| `POST` | `/orders` | Orders Service | Creación de nuevos pedidos. |
| `GET` | `/orders` | Orders Service | Ver historial de pedidos. |
| `GET` | `/orders/{id}` | Orders Service | Consultar estado de pedido. |
| `PUT` | `/orders/{id}/status` | Orders Service | Actualizar estado de pedido. |
| `PATCH` | `/orders/{id}` | Orders Service | Cancelar pedido (o similar). |

---

### Configuración Interna de Servicios

El API Gateway se conecta a los microservicios siguiendo la arquitectura definida:

* **Configuración del Balanceador gRPC (NGINX):**
    * NGINX escucha en el puerto **`7000`** para el tráfico gRPC.
    * Gestiona el enrutamiento de las peticiones a través de un pool de backends (upstream en NGINX) que mapea el tráfico al servicio interno correspondiente, utilizando la dirección `host.docker.internal` y el puerto específico configurado para cada microservicio (ejemplo:         `host.docker.internal:7004`).

* **Configuración del Cliente gRPC (ASP.NET Core):**
    * La configuración en `appsettings.json` dirige el cliente gRPC al balanceador de NGINX:
        ```json
        "Services": {
          "GrpcBalancer": "http://localhost:7000" 
        }
        ```

---

### Instalación y Ejecución

1.  **Requisitos Previos**
    * **.NET 9 SDK:** [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
    * **Docker Desktop:** Para ejecutar el balanceador NGINX.
    * **Microservicios Back-end:** Los microservicios deben estar levantados y accesibles en las direcciones configuradas (ej: Inventory Service en `host.docker.internal:7004`).

2.  **Clonar el Repositorio**
    ```bash
    git clone <URL del repositorio>
    cd <Directorio donde fue clonado>
    ```

3.  **Configurar y Levantar NGINX (Balanceador gRPC)**
    * Asegúrese de que Docker Desktop esté corriendo.
    * Levante el contenedor NGINX con Docker Compose:
    ```bash
    docker compose up -d nginx
    ```
    * Esto levantará el contenedor `censudex-nginx-lb` en el puerto `7000`.

4.  **Instalar Dependencias de la API C#**
    ```bash
    cd censudex-api
    dotnet restore
    ```

5.  **Ejecutar el Proyecto API Gateway**
    ```bash
    dotnet run
    ```
    * El API Gateway HTTP estará disponible por defecto en: `http://localhost:5114`

---

### Integrantes del Equipo

- Byron Syd Letelier Muriel - 19.968.262-8
- Nicolas Andrés Diaz Juica - 20.949.349-7
- Ignacio Javier Carvajal Canelo - 21.411.819.K
- Fernando Javier Quetzal Vega Flores  - 21.061.249-1
