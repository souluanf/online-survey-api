using Bunit;
using MudBlazor.Services;

namespace OnlineSurvey.Web.Tests;

/// <summary>
/// Classe base para testes bUnit com MudBlazor.
/// Implementa IAsyncLifetime para que o xUnit chame DisposeAsync,
/// evitando o erro de KeyInterceptorService que só tem IAsyncDisposable.
/// </summary>
public abstract class MudBunitContext : BunitContext, IAsyncLifetime
{
    protected MudBunitContext()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }
}
