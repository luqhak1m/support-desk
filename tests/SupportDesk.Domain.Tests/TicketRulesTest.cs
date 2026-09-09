
using SupportDesk.Domain;

namespace SupportDesk.Domain.Tests;

public class TicketRulesTest
{
    // 1. At least 4 meaningful backend unit tests, 
    // covering the status transition rules 
    // and the due date calculation.

    // New -> In Progress
    public void New_to_InProgress_isAllowed()
    {
        bool result=TicketRules.CanChange(TicketStatus.New, TicketStatus.InProgress);
        Assert.True(result);
    }

    // New (other than In Progress) -> Resolved
    public void New_to_Resolved_isRejected()
    {
        bool result=TicketRules.CanChange(TicketStatus.New, TicketStatus.Resolved);
        Assert.False(result);
    }

    // Resolved -> In Progress (Reopening)
    public void Resolved_to_InProgress_isAllowed()
    {
        bool result=TicketRules.CanChange(TicketStatus.Resolved, TicketStatus.InProgress);
        Assert.False(result);
    }

    // Closed -> Any
    public void Closed_to_Any_isRejected()
    {
        List<TicketStatus> allowed=TicketRules.AllowedFrom(TicketStatus.Closed);
        Assert.Empty(allowed); // there should not be any status allowed to be changed from Closed
    }

    // Critical priority adds 4 hours from creation date
    public void CalculateDueDate_Critical_isFourHoursAfterCreation()
    {
        DateTime created=new DateTime(2026, 1, 1, 9,0, 0, DateTimeKind.Utc);
        DateTime due=TicketRules.CalculateDueDate(created, TicketPriority.Critical);
        Assert.Equal(new DateTime(2026, 1, 1, 13, 0, 0, DateTimeKind.Utc), due); // == 7 days after creation date
    }

    // Low priority adds 7 days from creation date
    public void CalculateDueDate_Low_isSevenDaysAfterCreation()
    {
        DateTime created=new DateTime(2026, 1, 1, 9,0, 0, DateTimeKind.Utc);
        DateTime due=TicketRules.CalculateDueDate(created, TicketPriority.Low);
        Assert.Equal(new DateTime(2026, 1, 8, 9, 0, 0, DateTimeKind.Utc), due); // == 7 days after creation date
    }

    // Priority change recalculates the due date from the original date
    // public void CalculateDueDate_PriorityChange_isSevenDaysAfterCreation()
    // {
    //     DateTime created=new DateTime(2026, 1, 1, 9,0, 0, DateTimeKind.Utc);
    //     DateTime now=new DateTime(2026, 1, 4, 9, 0, 0, DateTimeKind.Utc); // 3 days after ticket created

    //     ticket.ChangePriority(TicketPriority.High, now);
        
    //     Assert.Equal(new DateTime(2026, 1, 2, 9, 0, 0, DateTimeKind.Utc), ticket.DueAt); // == 7 days after creation date
    // }


    
}