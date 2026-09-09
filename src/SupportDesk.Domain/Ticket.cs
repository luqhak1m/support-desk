
namespace SupportDesk.Domain;

public class Ticket
{
    public int Id{get; set;}
    public string Reference{get; set;}="";
    public string Title{get; set;}="";
    public string Description{get; set;}="";
    public string CustomerName{get; set;}="";
    public string CustomerEmail{get; set;}="";

    public TicketPriority Priority{get; private set;}
    public TicketStatus Status{get; private set;}
    public DateTime DueAt{get; private set;}
    public DateTime CreatedAt{get; private set;}
    public DateTime LastModifiedAt{get; private set;}
    public DateTime? ResolvedAt{get; private set;}
    public DateTime? ClosedAt{get; private set;}

    public int? AssignedAgentId{get; private set;}
    public Agent? AssignedAgent{get; private set;}

    public List<Comment> Comments{get; private set;}=new List<Comment>();

    private Ticket(){}

    public Ticket(
        string reference,
        string title,
        string description,
        string customerName,
        string customerEmail,
        TicketPriority priority,
        DateTime now
    )
    {
        Reference=reference;
        Title=title;
        Description=description;
        CustomerName=customerName;
        CustomerEmail=customerEmail;
        Priority=priority;
        Status=TicketStatus.New;
        CreatedAt=now;
        LastModifiedAt=now;
        DueAt=TicketRules.CalculateDueDate(now, priority);
    }

    // 5. A closed ticket is read-only — no edits, no status changes, no new comments.
    private void EnsureNotClosed()
    {
        if(Status == TicketStatus.Closed)
        {
            throw new DomainRuleViolationException("this ticket is closed and cannot be changed");
        }
    }

    private bool HasActiveAgent()
    {
        return AssignedAgent!=null && AssignedAgent.IsActive;
    }

    // lists which status can the current ticket go next
    public List<TicketStatus> AvailableTransitions()
    {
        List<TicketStatus> result=new List<TicketStatus>();
        foreach(TicketStatus candidate in TicketRules.AllowedFrom(Status))
        {
            if(candidate==TicketStatus.InProgress && !HasActiveAgent())
            {
                continue;
            }

            result.Add(candidate);
        }
        return result;
    }

    public void ChangeStatus(TicketStatus newStatus, DateTime now)
    {

        // rules check before changing the status:

        // 5. A closed ticket is read-only — no edits, no status changes, no new comments.
        EnsureNotClosed();

        // 2. Allowed status transitions: New → In Progress , In Progress → Resolved , Resolved → Closed , and Resolved → In Progress (reopening). Any other trasition must be rejected with a clear error. In particular, a ticket cannot skip straight from New to Resolved , and a Closed ticket can never be reopened.
        if(!TicketRules.CanChange(Status, newStatus)){throw new DomainRuleViolationException($"cannot change status from {Status} to {newStatus}");}

        // 3. A ticket cannot move to unless an active agent is assigned to it.
        if (newStatus == TicketStatus.InProgress)
        {
            if(!HasActiveAgent())
            {
                throw new DomainRuleViolationException("cannot change status to 'In Progress' without an active assigned agent");
            }
        }

        // rules check passes, set new status
        Status=newStatus;

        // if reopened remove the resolvedAt date
        if(newStatus==TicketStatus.InProgress){ResolvedAt=null;}

        // 6. Resolved date and closed date are set by the system when the corresponding transition happens, not by the client.
        if(Status==TicketStatus.Resolved){ResolvedAt=now;}
        else if(Status==TicketStatus.Closed){ClosedAt=now;}
        // reopening: the ticket is no longer resolved, so the resolved date must not linger
        else if(Status==TicketStatus.InProgress){ResolvedAt=null;}

        LastModifiedAt=now;
    }

    public void Assign(Agent agent, DateTime now)
    {
        // 5. A closed ticket is read-only — no edits, no status changes, no new comments.
        EnsureNotClosed();

        // 4. An inactive agent cannot be assigned to any ticket.
        if(!agent.IsActive)
        {
            throw new DomainRuleViolationException("an inactive agent cannot be assigned to tickets");
        }

        AssignedAgent=agent;
        AssignedAgentId=agent.Id;
        LastModifiedAt=now;
    }

    public void Unassign(DateTime now)
    {
        // 5. A closed ticket is read-only — no edits, no status changes, no new comments.
        EnsureNotClosed();

        AssignedAgent=null;
        AssignedAgentId=null;
        LastModifiedAt=now;
    }

    // 1. Due date is derived from the priority at creation time:
    // Critical → 4 hours after creation
    // High → 1 day
    // Normal → 3 days
    // Low → 7 days
    public void ChangePriority(TicketPriority newPriority, DateTime now)
    {
        // 5. A closed ticket is read-only — no edits, no status changes, no new comments.
        EnsureNotClosed();

        Priority=newPriority;
        // If the priority is changed while the ticket is still open, the due date is recalculated from the original creation date.
        DueAt=TicketRules.CalculateDueDate(CreatedAt, newPriority);
        LastModifiedAt=now;
    }

    // 5. A closed ticket is read-only - editable fields go through here so the guard cannot be skipped
    public void UpdateDetails(string title, string description, string customerName, string customerEmail, DateTime now)
    {
        EnsureNotClosed();

        Title=title;
        Description=description;
        CustomerName=customerName;
        CustomerEmail=customerEmail;
        LastModifiedAt=now;
    }

    public void AddComment(string authorName, string body, DateTime now)
    {
        // 5. A closed ticket is read-only — no edits, no status changes, no new comments.
        EnsureNotClosed();

        Comment comment=new Comment();
        comment.AuthorName=authorName;
        comment.Body=body;
        comment.CreatedAt=now;

        Comments.Add(comment);
        LastModifiedAt=now;
    }

    // 7. A ticket is overdue when its due date has passed and its status is neither Resolved nor Closed .    
    public bool IsOverdue(DateTime now)
    {
        if(Status==TicketStatus.Resolved || Status==TicketStatus.Closed)
        {
            return false;
        }

        return DueAt<now;
    }
}