namespace Application.ViewModels.Partners.v1;

public class PartnerCreateRequestViewModel
{
    public string Name { get; set; }
    public string SurName { get; set; }
    public string UserName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Password { get; set; }
}