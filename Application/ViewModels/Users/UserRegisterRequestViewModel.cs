namespace Application.ViewModels.Users;

public class UserRegisterRequestViewModel
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}