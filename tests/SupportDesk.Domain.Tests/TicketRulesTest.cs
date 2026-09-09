
using SupportDesk.Domain;

namespace SupportDesk.Domain.Tests;

public class TicketRulesTest
{
    // 1. At least 4 meaningful backend unit tests, 
    // covering the status transition rules 
    // and the due date calculation.

    // ---------- 2. status transitions ----------

    // New -> In Progress
    [Fact]
    public void New_to_InProgress_isAllowed()
    {
        bool result=TicketRules.CanChange(TicketStatus.New, TicketStatus.InProgress);
        Assert.True(result);
    }

    // New (other than In Progress) -> Resolved
    [Fact]
    public void New_to_Resolved_isRejected()
    {
        bool result=TicketRules.CanChange(TicketStatus.New, TicketStatus.Resolved);
        Assert.False(result);
    }

    // Resolved -> In Progress (Reopening)
    [Fact]
    public void Resolved_to_InProgress_isAllowed()
    {
        bool result=TicketRules.CanChange(TicketStatus.Resolved, TicketStatus.InProgress);
        Assert.True(result);
    }

    // Closed -> Any
    [Fact]
    public void Closed_to_Any_isRejected()
    {
        List<TicketStatus> allowed=TicketRules.AllowedFrom(TicketStatus.Closed);
        Assert.Empty(allowed); // there should not be any status allowed to be changed from Closed
    }

    // ---------- 1. due date ----------

    // Critical priority adds 4 hours from creation date
    [Fact]
    public void CalculateDueDate_Critical_isFourHoursAfterCreation()
    {
        DateTime created=new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        DateTime due=TicketRules.CalculateDueDate(created, TicketPriority.Critical);
        Assert.Equal(new DateTime(2026, 1, 1, 13, 0, 0, DateTimeKind.Utc), due); // == 4 hours after creation date
    }

    // Low priority adds 7 days from creation date
    [Fact]
    public void CalculateDueDate_Low_isSevenDaysAfterCreation()
    {
        DateTime created=new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        DateTime due=TicketRules.CalculateDueDate(created, TicketPriority.Low);
        Assert.Equal(new DateTime(2026, 1, 8, 9, 0, 0, DateTimeKind.Utc), due); // == 7 days after creation date
    }

    // Priority change recalculates the due date from the ORIGINAL creation date.
    // 'created' and 'now' must differ, otherwise a buggy implementation that counts
    // from 'now' would pass this test too.
    [Fact]
    public void ChangePriority_recalculatesDueDateFromOriginalCreationDate()
    {
        DateTime created=new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        DateTime now=new DateTime(2026, 1, 4, 9, 0, 0, DateTimeKind.Utc); // 3 days after ticket created

        Ticket ticket=NewTicket(TicketPriority.Low, created);
        ticket.ChangePriority(TicketPriority.High, now);

        // correct: created + 1 day = 2 Jan. the bug would give: now + 1 day = 5 Jan
        Assert.Equal(new DateTime(2026, 1, 2, 9, 0, 0, DateTimeKind.Utc), ticket.DueAt);
    }

    // ---------- 3. In Progress needs an active agent ----------

    [Fact]
    public void ChangeStatus_to_InProgress_withoutAgent_isRejected()
    {
        DateTime now=new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        Ticket ticket=NewTicket(TicketPriority.Normal, now);

        Assert.Throws<DomainRuleViolationException>(()=>
            ticket.ChangeStatus(TicketStatus.InProgress, now));
    }

    // ---------- 4. inactive agents cannot be assigned ----------

    [Fact]
    public void Assign_inactiveAgent_isRejected()
    {
        DateTime now=new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        Ticket ticket=NewTicket(TicketPriority.Normal, now);

        Agent inactive=new Agent();
        inactive.Id=1;
        inactive.FullName="Priya Nair";
        inactive.Email="priya.nair@supportdesk.test";
        inactive.Department=Department.Billing;
        inactive.IsActive=false;

        Assert.Throws<DomainRuleViolationException>(()=>ticket.Assign(inactive, now));
    }

    // ---------- 5. a closed ticket is read-only ----------

    [Fact]
    public void AddComment_onClosedTicket_isRejected()
    {
        DateTime now=new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);

        Agent agent=new Agent();
        agent.Id=1;
        agent.FullName="Aisha Rahman";
        agent.Email="aisha.rahman@supportdesk.test";
        agent.Department=Department.Technical;
        agent.IsActive=true;

        // walk the ticket all the way to Closed through the legal path
        Ticket ticket=NewTicket(TicketPriority.Normal, now);
        ticket.Assign(agent, now);
        ticket.ChangeStatus(TicketStatus.InProgress, now);
        ticket.ChangeStatus(TicketStatus.Resolved, now);
        ticket.ChangeStatus(TicketStatus.Closed, now);

        Assert.Throws<DomainRuleViolationException>(()=>
            ticket.AddComment("Someone", "hello", now));
    }

    // ---------- 6. the system stamps the resolved date ----------

    [Fact]
    public void ChangeStatus_to_Resolved_stampsResolvedDate()
    {
        DateTime now=new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        DateTime resolvedOn=new DateTime(2026, 1, 2, 15, 30, 0, DateTimeKind.Utc);

        Agent agent=new Agent();
        agent.Id=1;
        agent.FullName="Aisha Rahman";
        agent.Email="aisha.rahman@supportdesk.test";
        agent.Department=Department.Technical;
        agent.IsActive=true;

        Ticket ticket=NewTicket(TicketPriority.Normal, now);
        ticket.Assign(agent, now);
        ticket.ChangeStatus(TicketStatus.InProgress, now);
        ticket.ChangeStatus(TicketStatus.Resolved, resolvedOn);

        Assert.Equal(resolvedOn, ticket.ResolvedAt);
    }

    // ---------- 7. overdue ----------

    [Fact]
    public void IsOverdue_isFalse_onceResolved()
    {
        DateTime created=new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        DateTime muchLater=new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc);

        Agent agent=new Agent();
        agent.Id=1;
        agent.FullName="Aisha Rahman";
        agent.Email="aisha.rahman@supportdesk.test";
        agent.Department=Department.Technical;
        agent.IsActive=true;

        Ticket ticket=NewTicket(TicketPriority.Critical, created);
        Assert.True(ticket.IsOverdue(muchLater)); // due 4 hours after creation, so overdue by February

        ticket.Assign(agent, created);
        ticket.ChangeStatus(TicketStatus.InProgress, created);
        ticket.ChangeStatus(TicketStatus.Resolved, created);

        Assert.False(ticket.IsOverdue(muchLater)); // resolved tickets are never overdue
    }

    // ---------- helper ----------

    private static Ticket NewTicket(TicketPriority priority, DateTime createdAt)
    {
        return new Ticket(
            "TCK-2026-0001",
            "Cannot log in to the customer portal",
            "The portal rejects my password.",
            "Zara Ismail",
            "zara.ismail@example.com",
            priority,
            createdAt);
    }
}
