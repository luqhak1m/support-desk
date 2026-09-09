namespace SupportDesk.Domain;

public class Agent{
    public int Id{get; set;} // id=0, 1, 2, ...
    public string FullName{get; set;}="";
    public string Email{get; set;}="";
    public Department Department{get; set;}
    public bool IsActive{get; set;}
}