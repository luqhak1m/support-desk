using SupportDesk.Domain;

namespace SupportDesk.Api.Dtos;

// Turns domain entities into the shapes the API exposes.
// Kept separate so the EF entities are never returned directly.
public static class TicketMapper
{
    public static TicketListItemDto ToListItem(Ticket ticket, DateTime now)
    {
        TicketListItemDto dto=new TicketListItemDto();
        dto.Id=ticket.Id;
        dto.Reference=ticket.Reference;
        dto.Title=ticket.Title;
        dto.CustomerName=ticket.CustomerName;
        dto.Priority=ticket.Priority;
        dto.Status=ticket.Status;
        dto.AssignedAgentName=ticket.AssignedAgent==null?null:ticket.AssignedAgent.FullName;
        dto.DueAt=ticket.DueAt;
        dto.IsOverdue=ticket.IsOverdue(now);
        return dto;
    }

    public static TicketDetailDto ToDetail(Ticket ticket, DateTime now)
    {
        TicketDetailDto dto=new TicketDetailDto();
        dto.Id=ticket.Id;
        dto.Reference=ticket.Reference;
        dto.Title=ticket.Title;
        dto.Description=ticket.Description;
        dto.CustomerName=ticket.CustomerName;
        dto.CustomerEmail=ticket.CustomerEmail;
        dto.Priority=ticket.Priority;
        dto.Status=ticket.Status;
        dto.AssignedAgentId=ticket.AssignedAgentId;
        dto.AssignedAgentName=ticket.AssignedAgent==null?null:ticket.AssignedAgent.FullName;
        dto.CreatedAt=ticket.CreatedAt;
        dto.LastModifiedAt=ticket.LastModifiedAt;
        dto.ResolvedAt=ticket.ResolvedAt;
        dto.ClosedAt=ticket.ClosedAt;
        dto.DueAt=ticket.DueAt;
        dto.IsOverdue=ticket.IsOverdue(now);
        dto.IsReadOnly=ticket.Status==TicketStatus.Closed;
        dto.AvailableTransitions=ticket.AvailableTransitions();

        foreach(Comment comment in ticket.Comments.OrderBy(c=>c.CreatedAt))
        {
            dto.Comments.Add(ToComment(comment));
        }

        return dto;
    }

    public static CommentDto ToComment(Comment comment)
    {
        CommentDto dto=new CommentDto();
        dto.Id=comment.Id;
        dto.AuthorName=comment.AuthorName;
        dto.Body=comment.Body;
        dto.CreatedAt=comment.CreatedAt;
        return dto;
    }

    public static AgentDto ToAgent(Agent agent)
    {
        AgentDto dto=new AgentDto();
        dto.Id=agent.Id;
        dto.FullName=agent.FullName;
        dto.Email=agent.Email;
        dto.Department=agent.Department;
        dto.IsActive=agent.IsActive;
        return dto;
    }
}
