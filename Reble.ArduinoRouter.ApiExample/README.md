# Reble.ArduinoRouter.ApiExample

Example ASP.NET Core Web API demonstrating how to use the `Reble.ArduinoRouter` library.

> [!NOTE]
> This example is designed to run on Linux ARM64 devices (e.g., Arduino UNO Q) where the Arduino Router service is available.
> 
## Running with Docker (Recommended)

The Arduino UNO Q comes with Docker pre-installed. This is the easiest way to run the example.

### Setup

1. **SSH into your Arduino device**:
   ```bash
   ssh arduino@<device-ip>
   ```

2. **Clone the repository**:
   ```bash
   cd ~/ArduinoApps
   git clone https://github.com/maxreb/Reble.ArduinoRouter.git
   cd Reble.ArduinoRouter
   ```

3. **Build the Docker image**:
   ```bash 
   # Running this the first time will take some time 
   # as the dotnet sdk docker image needs to be downloaded
   docker build -t arduino-dotnet-router-example -f Reble.ArduinoRouter.ApiExample/Dockerfile .
   ```
4. **Run the container**:
   ```bash
   docker run -d \
     --name reble-router-api \
     -p 8080:8080 \
     -v /var/run/arduino-router.sock:/var/run/arduino-router.sock \
     arduino-dotnet-router-example
   ```

5. **Access Swagger UI** from your browser:
   ```
   http://<device-ip>:8080/swagger
   ```

### Docker Commands

```bash
# View logs
docker logs -f reble-router-api

# Stop the container
docker stop reble-router-api

# Remove the container
docker rm reble-router-api

# Rebuild after changes
docker build -t arduino-dotnet-router-example -f Reble.ArduinoRouter.ApiExample/Dockerfile .
```

---

## Endpoints

### Local Test Endpoints (No Additional Arduino Setup Required)

These endpoints demonstrate the Provider pattern where this .NET app acts as the RPC method provider.

#### Testing Flow

1. **Open Swagger UI** and call `GET /register-test`
   - This registers a method called `my-local-test` with the Arduino Router
   - The connection stays open (Server-Sent Events) waiting for incoming calls
   - Keep this browser tab open

2. **In a new browser tab**, call `GET /call-test` with `myText = "hello"`
   - Since "hello" has exactly 5 characters, it succeeds
   - The `/register-test` tab will show the request was processed
   - You'll get a response with a random favourite number

3. **Call `/call-test` again** with `myText = "hi"`
   - Since "hi" does not have 5 characters, it fails
   - The `/register-test` endpoint returns an error and closes the connection

---

### Arduino Endpoints (Requires RouterBridge Example Running)

These endpoints call methods provided by the Arduino microcontroller running the RouterBridge example.

| Endpoint | Description |
|----------|-------------|
| `GET /set-led` | Turn LEDs on/off (`numLed`: 0-1, `ledState`: true/false) |
| `GET /add` | Add two numbers (`num1`, `num2`) |
| `GET /greet` | Get a greeting message from the Arduino |

#### Setup Instructions

1. **Start Arduino App Lab** on the device

2. **Navigate to "My Apps"** and select the **RouterBridge** example

3. **Click "Run"** to start the Arduino-side RPC provider

4. **Test the endpoints** via Swagger UI:
   - `/set-led?numLed=0&ledState=true` - Turn on LED 0
   - `/add?num1=5&num2=3` - Returns 8
   - `/greet` - Returns a greeting from the Arduino

#### Background Service

The `MultiplierBackgroundService` automatically registers a `multiply` method on startup for the Arduino example. This demonstrates how to provide RPC methods as a background service in ASP.NET Core.
