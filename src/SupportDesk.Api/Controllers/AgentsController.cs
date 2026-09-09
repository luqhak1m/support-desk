using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportDesk.Api.Dtos;
using SupportDesk.Domain;
using SupportDesk.Infrastructure;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Route("api/agents")]
public class AgentsController:ControllerBase
{
    private readonly SupportDeskDbContext _db;

    public AgentsController(SupportDeskDbContext db)
    {
        _db=db;
    }

    // GET /api/agents?search=
    [HttpGet]
    public async Task<ActionResult<List<AgentDto>>> GetAgents(string? search=null)
    {
        IQueryable<Agent> query=_db.Agents.AsQueryable();

        if(!string.IsNullOrWhiteSpace(search))
        {
            string term=search.Trim().ToLower();
            query=query.Where(a=>
                a.FullName.ToLower().Contains(term)||
                a.Email.ToLower().Contains(term));
        }

        List<Agent> agents=await query.OrderBy(a=>a.FullName).ToListAsync();

        List<AgentDto> result=new List<AgentDto>();
        foreach(Agent agent in agents)
        {
            result.Add(TicketMapper.ToAgent(agent));
        }

        return Ok(result);
    }

    // GET /api/agents/5
    [HttpGet("{id}")]
    public async Task<ActionResult<AgentDto>> GetAgent(int id)
    {
        Agent? agent=await _db.Agents.FirstOrDefaultAsync(a=>a.Id==id);

        if(agent==null)
        {
            return Problem(
                title:"Agent not found",
                detail:$"No agent exists with id {id}.",
                statusCode:StatusCodes.Status404NotFound);
        }

        return Ok(TicketMapper.ToAgent(agent));
    }
}
