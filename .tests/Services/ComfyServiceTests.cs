// ComfyServiceTests.cs

using Microsoft.Extensions.Options;
using LLMRemote.Options;
using LLMRemote.Services;
using LLMRemote.Tests.Util;
using LLMRemote.Tests.Factories;
using Bogus;
using LLMRemote.Models;
using System.Net;

namespace LLMRemote.Tests.Services;

public class ComfyServiceTests{
    private readonly ComfyService _service;
    private readonly FakeProcess _process = new();
    private readonly Faker _faker = new Faker();
    private readonly OptionsWrapper<Apps> _apps;

    public ComfyServiceTests(){
        _apps = new OptionsWrapper<Apps>(new Apps {
                ComfyUI = new AppConfig { Path = _faker.System.FilePath(), Port = _faker.Internet.Port(), OtherSettings ="" }
        });

        _service = CreateService(HttpStatusCode.OK);
    }
    
    private ComfyService CreateService(HttpStatusCode code) => new ComfyService(_process, _apps, new HttpClient(new FakeHttpHandler(code)));
}
