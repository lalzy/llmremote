// DatabaseTestBase.cs
using Xunit;

namespace LLMRemote.Tests;

public abstract class DatabaseTestBase : IClassFixture<DatabaseFixture>
{
    protected readonly DatabaseFixture _fixture;

    protected DatabaseTestBase(DatabaseFixture fixture)
    {
        fixture.Reset();
        _fixture = fixture;
    }
}
