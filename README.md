# LLM Remote Service

Runs llama.cpp remotely through a web interface.

## Disclaimer
App has not been tested on MAC or Linux. While I tried to make it so it can run on MAC and Linux, having not tested it I can not verify if it works or not.

## Requirements
1. [llama.cpp](https://github.com/ggml-org/llama.cpp) - backend to run llm models
2. LLM models - You can find them through google or on [hugging face](https://huggingface.co/models?library=gguf)
3. (optional) [ComfyUI](https://github.com/Comfy-Org/ComfyUI)
4. [.net SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0) - to build and run

# Initial Setup
1. clone project (see the shiny green button)
2. ```cd {intoFolder}```
3. ```dotnet restore```
4. [Configure](#configure) pathing
5. [Run service](#run-the-service) (or [run tests](#tests))
6. Open browser and go to [http://localhost:5242/](http://localhost:5242/)

# Configure
1. Open appsettings.json
2. Change the entries under "apps" to whatever you need/want.
2.1 make sure to at least place "" on the paths for processes you don't plan to use.
2.2 Do not use single '\'  use \\ or / (can be single that way)
3. (optional) change the ports and otherSettings.
4. (optional) If you want to change port, or make it run remotely, change the "urls" (0.0.0.0 will give remote access)

 
Open appsettings.json Change the entries under "apps" to whatever you want / where you need (if you do not want to use comfy, make sure the path is set to "" (blank).

### LLama
By default these options are set (GPU-layers ensure full GPU usage, flash-attn is a "new" faster compute attention for LLM):
--gpu-layers -1
--flash-attn on

### ComfyUI
By default these options (Allows SIllyTavern to communicate with ComfyUI):
--listen
--enable-cors-header



# Run The Service
Type ```dotnet Run``` in a terminal/command prompt (bash, powershell, cmd) pointed to the root (where you cloned the project to).

# Building (for standalone execution)
```dotnet publish -c Release -r {OS-Architecture} --self-contained -o publish```
Replace {os-architecture} with whatever you have. Example
- Windows: win-x64
- Mac: osx-x64
- Linux: linux-x64
- RaspberryPi: linux-arm64

# Tests
Run tests with ```dotnet test```

# Project structure

```
src/
  ..Controllers/ - API Endpoints
  ..Data/ - Database Definition
  ..Models/ - Data Models
  ..Services/ - Business Logic
  ..Views/ - Razor Pages
.Tests/ - Unit tests
```

# API Documentation
Run the service, then go to [http://localhost:5242/swagger](http://localhost:5242/swagger)
