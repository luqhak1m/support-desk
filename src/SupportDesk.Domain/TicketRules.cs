namespace SupportDesk.Domain;

public static class TicketRules{

/* Rule #2: Allowed status transitions: 

case 1: New → In Progress , 
case 2: In Progress → Resolved , 
case 3: Resolved → Closed ,  
case 4: Resolved → In Progress (reopening). 

Any other transition must be rejected with a clear error. 
In particular, a ticket cannot skip straight from New to Resolved , 
and a Closed ticket can never be reopened.*/

    public static List<TicketStatus> AllowedFrom(TicketStatus status)
    {

        List<TicketStatus> allowed=new List<TicketStatus>();

        switch (status)
        {
            case TicketStatus.New: // case 1
                allowed.Add(TicketStatus.InProgress); 
                break;

            case TicketStatus.InProgress: // case 2
                allowed.Add(TicketStatus.Resolved);
                break;

            case TicketStatus.Resolved: // case 3 & 4
                allowed.Add(TicketStatus.Closed);
                allowed.Add(TicketStatus.InProgress);
                break;

            case TicketStatus.Closed:
                break;
        }

        return allowed;

    }

    public static bool CanChange(TicketStatus from, TicketStatus to)
    {
        return AllowedFrom(from).Contains(to); // check if the 'from' status can be changed into the 'to' status
    }

    // Due date (calculated by the system, never sent by the client)
    /* Due date is derived from the priority at creation time:
        
        Critical → 4 hours after creation
        High → 1 day
        Normal → 3 days
        Low → 7 days
        
        If the priority is changed while the ticket is still open, 
        the due date is recalculated from the original creation date. */

    public static DateTime CalculateDueDate(DateTime createdAt, TicketPriority priority)
    {
        switch (priority)
        {
            case TicketPriority.Critical:
                return createdAt.AddHours(4);
            
            case TicketPriority.High:
                return createdAt.AddDays(1);
                
            case TicketPriority.Normal:
                return createdAt.AddDays(3);

            case TicketPriority.Low:
                return createdAt.AddDays(7);
            
            default:
                Console.WriteLine($"[CalculateDueDate] invalid parameter: {priority}");
                throw new ArgumentOutOfRangeException(nameof(priority));
        }
    }

}