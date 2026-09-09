using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportDesk.Api.Dtos;
using SupportDesk.Domain;
using SupportDesk.Infrastructure;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketsController:ControllerBase
{
    private readonly SupportDeskDbContext _db;

    public TicketsController(SupportDeskDbContext db)
    {
        _db=db;
    }

    // GET /api/tickets?page=1&pageSize=10&search=&status=&priority=&agentId=&overdueOnly=
    [HttpGet]
    public async Task<ActionResult<PagedResult<TicketListItemDto>>> GetTickets(
        int page=1,
        int pageSize=10,
        string? search=null,
        TicketStatus? status=null,
        TicketPriority? priority=null,
        int? agentId=null,
        bool overdueOnly=false)
    {
        if(page<1){page=1;}
        if(pageSize<1||pageSize>100){pageSize=10;}

        DateTime now=DateTime.UtcNow;

        IQueryable<Ticket> query=_db.Tickets.Include(t=>t.AssignedAgent).AsQueryable();

        // search by reference, title or customer name
        if(!string.IsNullOrWhiteSpace(search))
        {
            string term=search.Trim().ToLower();
            query=query.Where(t=>
                t.Reference.ToLower().Contains(term)||
                t.Title.ToLower().Contains(term)||
                t.CustomerName.ToLower().Contains(term));
        }

        if(status!=null){query=query.Where(t=>t.Status==status);}
        if(priority!=null){query=query.Where(t=>t.Priority==priority);}
        if(agentId!=null){query=query.Where(t=>t.AssignedAgentId==agentId);}

        // 7. overdue = due date passed and status is neither Resolved nor Closed
        if(overdueOnly)
        {
            query=query.Where(t=>t.DueAt<now
                &&t.Status!=TicketStatus.Resolved
                &&t.Status!=TicketStatus.Closed);
        }

        int totalCount=await query.CountAsync();

        List<Ticket> tickets=await query
            .OrderByDescending(t=>t.CreatedAt)
            .Skip((page-1)*pageSize)
            .Take(pageSize)
            .ToListAsync();

        PagedResult<TicketListItemDto> result=new PagedResult<TicketListItemDto>();
        result.Page=page;
        result.PageSize=pageSize;
        result.TotalCount=totalCount;
        result.TotalPages=(int)Math.Ceiling(totalCount/(double)pageSize);

        foreach(Ticket ticket in tickets)
        {
            result.Items.Add(TicketMapper.ToListItem(ticket,now));
        }

        return Ok(result);
    }

    // GET /api/tickets/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TicketDetailDto>> GetTicket(int id)
    {
        Ticket? ticket=await LoadTicket(id);
        if(ticket==null){return TicketNotFound(id);}

        return Ok(TicketMapper.ToDetail(ticket,DateTime.UtcNow));
    }

    // POST /api/tickets
    [HttpPost]
    public async Task<ActionResult<TicketDetailDto>> CreateTicket(CreateTicketRequest request)
    {
        DateTime now=DateTime.UtcNow;

        Ticket ticket=new Ticket(
            await NextReference(now),
            request.Title,
            request.Description,
            request.CustomerName,
            request.CustomerEmail,
            request.Priority,
            now);

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();

        TicketDetailDto dto=TicketMapper.ToDetail(ticket,now);
        return CreatedAtAction(nameof(GetTicket),new{id=ticket.Id},dto);
    }

    // PUT /api/tickets/5
    [HttpPut("{id}")]
    public async Task<ActionResult<TicketDetailDto>> UpdateTicket(int id, UpdateTicketRequest request)
    {
        Ticket? ticket=await LoadTicket(id);
        if(ticket==null){return TicketNotFound(id);}

        DateTime now=DateTime.UtcNow;

        ticket.UpdateDetails(request.Title,request.Description,request.CustomerName,request.CustomerEmail,now);

        // 1. changing priority recalculates the due date from the original creation date
        if(ticket.Priority!=request.Priority)
        {
            ticket.ChangePriority(request.Priority,now);
        }

        await _db.SaveChangesAsync();
        return Ok(TicketMapper.ToDetail(ticket,now));
    }

    // DELETE /api/tickets/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        Ticket? ticket=await LoadTicket(id);
        if(ticket==null){return TicketNotFound(id);}

        _db.Tickets.Remove(ticket);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // POST /api/tickets/5/status
    // Dedicated endpoint: a status change is a transition with its own rules and side effects,
    // not a field update. See README.
    [HttpPost("{id}/status")]
    public async Task<ActionResult<TicketDetailDto>> ChangeStatus(int id, ChangeStatusRequest request)
    {
        Ticket? ticket=await LoadTicket(id);
        if(ticket==null){return TicketNotFound(id);}

        DateTime now=DateTime.UtcNow;
        ticket.ChangeStatus(request.Status,now);

        await _db.SaveChangesAsync();
        return Ok(TicketMapper.ToDetail(ticket,now));
    }

    // POST /api/tickets/5/assign
    [HttpPost("{id}/assign")]
    public async Task<ActionResult<TicketDetailDto>> AssignAgent(int id, AssignAgentRequest request)
    {
        Ticket? ticket=await LoadTicket(id);
        if(ticket==null){return TicketNotFound(id);}

        Agent? agent=await _db.Agents.FirstOrDefaultAsync(a=>a.Id==request.AgentId);
        if(agent==null)
        {
            return Problem(
                title:"Agent not found",
                detail:$"No agent exists with id {request.AgentId}.",
                statusCode:StatusCodes.Status404NotFound);
        }

        DateTime now=DateTime.UtcNow;
        ticket.Assign(agent,now);

        await _db.SaveChangesAsync();
        return Ok(TicketMapper.ToDetail(ticket,now));
    }

    // POST /api/tickets/5/unassign
    [HttpPost("{id}/unassign")]
    public async Task<ActionResult<TicketDetailDto>> UnassignAgent(int id)
    {
        Ticket? ticket=await LoadTicket(id);
        if(ticket==null){return TicketNotFound(id);}

        DateTime now=DateTime.UtcNow;
        ticket.Unassign(now);

        await _db.SaveChangesAsync();
        return Ok(TicketMapper.ToDetail(ticket,now));
    }

    // POST /api/tickets/5/comments
    [HttpPost("{id}/comments")]
    public async Task<ActionResult<TicketDetailDto>> AddComment(int id, AddCommentRequest request)
    {
        Ticket? ticket=await LoadTicket(id);
        if(ticket==null){return TicketNotFound(id);}

        DateTime now=DateTime.UtcNow;
        ticket.AddComment(request.AuthorName,request.Body,now);

        await _db.SaveChangesAsync();
        return Ok(TicketMapper.ToDetail(ticket,now));
    }

    // ---------- helpers ----------

    private async Task<Ticket?> LoadTicket(int id)
    {
        return await _db.Tickets
            .Include(t=>t.AssignedAgent)
            .Include(t=>t.Comments)
            .FirstOrDefaultAsync(t=>t.Id==id);
    }

    private ActionResult TicketNotFound(int id)
    {
        return Problem(
            title:"Ticket not found",
            detail:$"No ticket exists with id {id}.",
            statusCode:StatusCodes.Status404NotFound);
    }

    // TCK-2026-0001 - needs to know about other tickets, so it cannot live in the domain
    private async Task<string> NextReference(DateTime now)
    {
        int year=now.Year;
        string prefix=$"TCK-{year}-";

        // Take the highest number already used this year rather than counting rows,
        // so deleting a ticket cannot make the next reference collide with an existing one.
        List<string> existing=await _db.Tickets
            .Where(t=>t.Reference.StartsWith(prefix))
            .Select(t=>t.Reference)
            .ToListAsync();

        int highest=0;
        foreach(string reference in existing)
        {
            int parsed;
            if(int.TryParse(reference.Substring(prefix.Length),out parsed)&&parsed>highest)
            {
                highest=parsed;
            }
        }

        return prefix+(highest+1).ToString("D4");
    }
}
