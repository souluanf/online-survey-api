using Microsoft.JSInterop;
using System.Text.Json.Serialization;

namespace OnlineSurvey.Web.Services;

public class FirebaseUser
{
    [JsonPropertyName("uid")] public string Uid { get; set; } = "";
    [JsonPropertyName("email")] public string Email { get; set; } = "";
    [JsonPropertyName("displayName")] public string? DisplayName { get; set; }
    [JsonPropertyName("token")] public string Token { get; set; } = "";
}

public class FirebaseAuthService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private DotNetObjectReference<FirebaseAuthService>? _dotnetRef;
    private FirebaseUser? _currentUser;

    public event Action? AuthStateChanged;
    public FirebaseUser? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser is not null;

    public FirebaseAuthService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync()
    {
        _dotnetRef = DotNetObjectReference.Create(this);
        await _js.InvokeVoidAsync("firebaseAuth.initialize", _dotnetRef);
    }

    [JSInvokable]
    public void OnAuthStateChanged(FirebaseUser? user)
    {
        _currentUser = user;
        AuthStateChanged?.Invoke();
    }

    public async Task<FirebaseUser> SignInWithGoogleAsync()
    {
        var user = await _js.InvokeAsync<FirebaseUser>("firebaseAuth.signInWithGoogle");
        _currentUser = user;
        AuthStateChanged?.Invoke();
        return user;
    }

    public async Task<FirebaseUser> SignInWithEmailAsync(string email, string password)
    {
        var user = await _js.InvokeAsync<FirebaseUser>("firebaseAuth.signInWithEmail", email, password);
        _currentUser = user;
        AuthStateChanged?.Invoke();
        return user;
    }

    public async Task<FirebaseUser> RegisterWithEmailAsync(string email, string password)
    {
        var user = await _js.InvokeAsync<FirebaseUser>("firebaseAuth.registerWithEmail", email, password);
        _currentUser = user;
        AuthStateChanged?.Invoke();
        return user;
    }

    public async Task SignOutAsync()
    {
        await _js.InvokeVoidAsync("firebaseAuth.signOut");
        _currentUser = null;
        AuthStateChanged?.Invoke();
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _js.InvokeAsync<string?>("firebaseAuth.getToken");
    }

    public async ValueTask DisposeAsync()
    {
        _dotnetRef?.Dispose();
    }
}
