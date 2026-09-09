using System.ComponentModel.DataAnnotations;
using SupportDesk.Domain;

namespace SupportDesk.Api.Dtos;

// One page of results, used by the ticket list
public class PagedResult<T>
{
    public List<T> Items{get; set;}=new List<T>();
    public int Page{get; set;}
    public int PageSize{get; set;}
    public int TotalCount{get; set;}
    public int TotalPages{get; set;}
}

// Slim shape for the list page
public class TicketListItemDto
{
    public int Id{get; set;}
    public string Reference{get; set;}="";
    public string Title{get; set;}="";
    public string CustomerName{get; set;}="";
    public TicketPriority Priority{get; set;}
    public TicketStatus Status{get; set;}
    public string? AssignedAgentName{get; set;}
    public DateTime DueAt{get; set;}
    public bool IsOverdue{get; set;}
}

public class CommentDto
{
    public int Id{get; set;}
    public string AuthorName{get; set;}="";
    public string Body{get; set;}="";
    public DateTime CreatedAt{get; set;}
}

// Full shape for the detail page
public class TicketDetailDto
{
    public int Id{get; set;}
    public string Reference{get; set;}="";
    public string Title{get; set;}="";
    public string Description{get; set;}="";
    public string CustomerName{get; set;}="";
    public string CustomerEmail{get; set;}="";
    public TicketPriority Priority{get; set;}
    public TicketStatus Status{get; set;}
    public int? AssignedAgentId{get; set;}
    public string? AssignedAgentName{get; set;}
    public DateTime CreatedAt{get; set;}
    public DateTime LastModifiedAt{get; set;}
    public DateTime? ResolvedAt{get; set;}
    public DateTime? ClosedAt{get; set;}
    public DateTime DueAt{get; set;}
    public bool IsOverdue{get; set;}
    public bool IsReadOnly{get; set;}
    public List<TicketStatus> AvailableTransitions{get; set;}=new List<TicketStatus>();
    public List<CommentDto> Comments{get; set;}=new List<CommentDto>();
}

public class CreateTicketRequest
{
    [Required][MaxLength(200)]
    public string Title{get; set;}="";

    [Required]
    public string Description{get; set;}="";

    [Required][MaxLength(200)]
    public string CustomerName{get; set;}="";

    [Required][EmailAddress][MaxLength(200)]
    public string CustomerEmail{get; set;}="";

    [Required]
    public TicketPriority Priority{get; set;}
}

public class UpdateTicketRequest
{
    [Required][MaxLength(200)]
    public string Title{get; set;}="";

    [Required]
    public string Description{get; set;}="";

    [Required][MaxLength(200)]
    public string CustomerName{get; set;}="";

    [Required][EmailAddress][MaxLength(200)]
    public string CustomerEmail{get; set;}="";

    [Required]
    public TicketPriority Priority{get; set;}
}

public class ChangeStatusRequest
{
    [Required]
    public TicketStatus Status{get; set;}
}

public class AssignAgentRequest
{
    [Required]
    public int AgentId{get; set;}
}

public class AddCommentRequest
{
    [Required][MaxLength(200)]
    public string AuthorName{get; set;}="";

    [Required][MaxLength(4000)]
    public string Body{get; set;}="";
}

public class AgentDto
{
    public int Id{get; set;}
    public string FullName{get; set;}="";
    public string Email{get; set;}="";
    public Department Department{get; set;}
    public bool IsActive{get; set;}
}
