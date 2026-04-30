namespace Application.ViewModels.Users;

/// <summary>
/// Datos internos del usuario autenticado. No se expone directamente al cliente.
/// Se usa en la capa Presentation para generar el JWT.
/// </summary>
public class UserLoginResultViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
}

