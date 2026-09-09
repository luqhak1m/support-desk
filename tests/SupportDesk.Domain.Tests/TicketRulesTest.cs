
using SupportDesk.Domain;

namespace SupportDesk.Domain.Tests;

// Four tests, one per area the assignment is graded on.
// Each names the wrong implementation it exists to catch.
public class TicketRulesTest
{
    // Rule 2. Catches a map that lets a ticket skip the workflow, forgets
    // reopening, or treats Closed as an ordinary status instead of terminal.
    [Fact]
    public void TheWorkflow_allowsOnlyTheFourLegalMoves()
    {
        // the four that are allowed
        Assert.True(TicketRules.CanChange(TicketStatus.New, TicketStatus.InProgress));
        Assert.True(TicketRules.CanChange(TicketStatus.InProgress, TicketStatus.Resolved));
        Assert.True(TicketRules.CanChange(TicketStatus.Resolved, TicketStatus.Closed));
        Assert.True(TicketRules.CanChange(TicketStatus.Resolved, TicketStatus.InProgress)); // reopening

        // no skipping the queue
        Assert.False(TicketRules.CanChange(TicketStatus.New, TicketStatus.Resolved));
        Assert.False(TicketRules.CanChange(TicketStatus.New, TicketStatus.Closed));

        // closed is final, whatever the target
        foreach(TicketStatus target in Enum.GetValues<TicketStatus>())
        {
            Assert.False(TicketRules.CanChange(TicketStatus.Closed, target));
        }

        // and nothing may transition to itself
        foreach(TicketStatus status in Enum.GetValues<TicketStatus>())
        {
            Assert.False(TicketRules.CanChange(status, status));
        }
    }

    // Rule 1. Catches the wrong unit (4 days instead of 4 hours) and, more
    // importantly, a recalculation that counts from "now" instead of the
    // original creation date. 'created' and 'later' MUST differ, or a buggy
    // implementation passes this test too.
    [Fact]
    public void DueDate_comesFromPriority_andIsRecalculatedFromTheOriginalCreationDate()
    {
        DateTime created=new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);

        Assert.Equal(created.AddHours(4), TicketRules.CalculateDueDate(created, TicketPriority.Critical));
        Assert.Equal(created.AddDays(1),  TicketRules.CalculateDueDate(created, TicketPriority.High));
        Assert.Equal(created.AddDays(3),  TicketRules.CalculateDueDate(created, TicketPriority.Normal));
        Assert.Equal(created.AddDays(7),  TicketRules.CalculateDueDate(created, TicketPriority.Low));

        DateTime later=created.AddDays(3);
        Ticket ticket=NewTicket(TicketPriority.Low, created);
        ticket.ChangePriority(TicketPriority.High, later);

        // correct: 1 Jan + 1 day = 2 Jan.   the bug: 4 Jan + 1 day = 5 Jan
        Assert.Equal(created.AddDays(1), ticket.DueAt);
    }

    // Rules 3 and 4. Catches an implementation that only checks whether an agent
    // exists and ignores IsActive, and one where AvailableTransitions offers a
    // button the API would then reject.
    [Fact]
    public void WorkCannotStart_withoutAnActiveAssignedAgent()
    {
        DateTime now=new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        Ticket ticket=NewTicket(TicketPriority.Normal, now);

        // unassigned: rejected, and the UI is offered nothing
        Assert.Throws<DomainRuleViolationException>(()=>ticket.ChangeStatus(TicketStatus.InProgress, now));
        Assert.Empty(ticket.AvailableTransitions());
        Assert.Equal(TicketStatus.New, ticket.Status);

        // an inactive agent cannot be assigned at all
        Assert.Throws<DomainRuleViolationException>(()=>ticket.Assign(NewAgent(false), now));
        Assert.Null(ticket.AssignedAgentId);

        // assigned and active: now it is offered, and it works
        Agent agent=NewAgent(true);
        ticket.Assign(agent, now);
        Assert.Equal(new List<TicketStatus>{TicketStatus.InProgress}, ticket.AvailableTransitions());

        // but if that agent is later deactivated, work can no longer start
        agent.IsActive=false;
        Assert.Throws<DomainRuleViolationException>(()=>ticket.ChangeStatus(TicketStatus.InProgress, now));
        Assert.Empty(ticket.AvailableTransitions());
    }

    // Rules 5, 6 and 7. Catches a guard applied to ChangeStatus but forgotten on
    // the other mutations, timestamps accepted from the caller or overwritten,
    // a stale resolved date left on a reopened ticket, and an overdue check that
    // ignores status.
    [Fact]
    public void TheSystemOwnsTheTimestamps_andAClosedTicketIsReadOnly()
    {
        DateTime created=new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        DateTime resolvedOn=created.AddDays(2);
        DateTime closedOn=created.AddDays(3);
        DateTime muchLater=created.AddMonths(6);

        Ticket ticket=NewTicket(TicketPriority.Critical, created);
        ticket.Assign(NewAgent(true), created);
        ticket.ChangeStatus(TicketStatus.InProgress, created);

        Assert.Null(ticket.ResolvedAt);                  // starting work resolves nothing
        Assert.True(ticket.IsOverdue(muchLater));        // due after 4 hours, still open

        ticket.ChangeStatus(TicketStatus.Resolved, resolvedOn);
        Assert.Equal(resolvedOn, ticket.ResolvedAt);
        Assert.False(ticket.IsOverdue(muchLater));       // finished work is never overdue

        ticket.ChangeStatus(TicketStatus.InProgress, created.AddDays(2)); // reopen
        Assert.Null(ticket.ResolvedAt);                  // the stale date must not linger

        ticket.ChangeStatus(TicketStatus.Resolved, resolvedOn);
        ticket.ChangeStatus(TicketStatus.Closed, closedOn);
        Assert.Equal(closedOn, ticket.ClosedAt);
        Assert.Equal(resolvedOn, ticket.ResolvedAt);     // closing does not overwrite it

        // closed means closed. every mutation is refused
        Assert.Throws<DomainRuleViolationException>(()=>ticket.ChangeStatus(TicketStatus.InProgress, closedOn));
        Assert.Throws<DomainRuleViolationException>(()=>ticket.AddComment("Sam", "hello", closedOn));
        Assert.Throws<DomainRuleViolationException>(()=>ticket.ChangePriority(TicketPriority.Low, closedOn));
        Assert.Throws<DomainRuleViolationException>(()=>ticket.UpdateDetails("hijacked", "d", "c", "c@e.com", closedOn));
        Assert.Throws<DomainRuleViolationException>(()=>ticket.Assign(NewAgent(true), closedOn));
        Assert.Throws<DomainRuleViolationException>(()=>ticket.Unassign(closedOn));

        // and a refused change left nothing behind
        Assert.Equal("Cannot log in to the customer portal", ticket.Title);
        Assert.Equal(TicketStatus.Closed, ticket.Status);
    }

    // ---------- helpers ----------

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

    private static Agent NewAgent(bool isActive)
    {
        Agent agent=new Agent();
        agent.Id=1;
        agent.FullName="Aisha Rahman";
        agent.Email="aisha.rahman@supportdesk.test";
        agent.Department=Department.Technical;
        agent.IsActive=isActive;
        return agent;
    }
}
