using Microsoft.AspNetCore.Identity;

namespace LitackaApi.Data.Entities;

public class Card
{
    public int Id { get; set; }                     

    public string UserId { get; set; } = null!;     
    public IdentityUser User { get; set; } = null!; 

    public DateOnly ExpirationDate { get; set; }    

    public int StatusId { get; set; }               
    public CardStatus Status { get; set; } = null!;
}
