namespace SupportDesk.Domain;

public class Comment{
    public int Id{get; set;}
    public int TicketId{get; set;}
    public string AuthorName{get; set;}="";
    public string Body{get; set;}="";
    public DateTime CreatedAt{get; set;}
}