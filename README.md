# LLM Remote Service

Runs llama.cpp remotely through a web interface.

## Requirements:
1. [llama.cpp](https://github.com/ggml-org/llama.cpp) - backend to run llm models
2. LLM models - You can find them through google or on [hugging face](https://huggingface.co/models?library=gguf)
3. (optional) [ComfyUI](https://github.com/Comfy-Org/ComfyUI)
4. [.net SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0) - to build and run

# Initial Setup
1. clone project (see the shiny green button)
2. ```cd {intoFolder}```
3. ```dotnet restore```
4. [Run service](#run-the-service) (or [run tests](#tests))
5. Open browser and go to [http://localhost:5242/](http://localhost:5242/)

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

# API Documentation:
Run the service, then go to [http://localhost:5242/swagger](http://localhost:5242/swagger)
