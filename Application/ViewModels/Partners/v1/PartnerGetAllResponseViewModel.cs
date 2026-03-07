namespace Application.ViewModels.Partners.v1;

public class PartnerGetAllResponseViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } 
    public string SurName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}