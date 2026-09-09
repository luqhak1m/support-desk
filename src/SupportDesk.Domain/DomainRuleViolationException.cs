
namespace SupportDesk.Domain;

public class DomainRuleViolationException : Exception
{
    public DomainRuleViolationException(string message) : base(message)
    {
        
    }
}