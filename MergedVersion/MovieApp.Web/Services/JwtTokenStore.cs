namespace MovieApp.Web.Services;

public class JwtTokenStore
{
    private string? _token;

    public string? Token => _token;

    public void SetToken(string token)
    {
        _token = token;
    }

    public bool HasToken => !string.IsNullOrEmpty(_token);
}