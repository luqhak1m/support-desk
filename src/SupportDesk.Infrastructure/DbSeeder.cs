using SupportDesk.Domain;

namespace SupportDesk.Infrastructure;

public static class DbSeeder
{
    public static void Seed(SupportDeskDbContext db)
    {
        if(db.Agents.Any()){return;}   // already seeded, do nothing

        // ---------- 5 agents (one inactive, to exercise rule 4) ----------
        Agent aisha=NewAgent("Aisha Rahman","aisha.rahman@supportdesk.test",Department.Technical,true);
        Agent daniel=NewAgent("Daniel Okafor","daniel.okafor@supportdesk.test",Department.Billing,true);
        Agent meiLin=NewAgent("Mei Lin Tan","meilin.tan@supportdesk.test",Department.Technical,true);
        Agent carlos=NewAgent("Carlos Mendes","carlos.mendes@supportdesk.test",Department.General,true);
        Agent priya=NewAgent("Priya Nair","priya.nair@supportdesk.test",Department.Billing,false);

        db.Agents.AddRange(aisha,daniel,meiLin,carlos,priya);
        db.SaveChanges();

        DateTime now=DateTime.UtcNow;
        List<Ticket> tickets=new List<Ticket>();

        // ---------- New, not yet overdue ----------
        tickets.Add(NewTicket("TCK-2026-0001","Cannot log in to the customer portal","Zara Ismail","zara.ismail@example.com",TicketPriority.High,now.AddHours(-2)));
        tickets.Add(NewTicket("TCK-2026-0002","Invoice shows the wrong currency","Tom Baker","tom.baker@example.com",TicketPriority.Normal,now.AddHours(-6)));
        tickets.Add(NewTicket("TCK-2026-0003","Request for an additional user seat","Nadia Haq","nadia.haq@example.com",TicketPriority.Low,now.AddDays(-1)));
        tickets.Add(NewTicket("TCK-2026-0004","Password reset email never arrives","Ken Watanabe","ken.watanabe@example.com",TicketPriority.Critical,now.AddMinutes(-30)));

        // ---------- New and OVERDUE ----------
        tickets.Add(NewTicket("TCK-2026-0005","Payment gateway rejects all cards","Lucia Moreno","lucia.moreno@example.com",TicketPriority.Critical,now.AddDays(-2)));
        tickets.Add(NewTicket("TCK-2026-0006","Monthly statement missing line items","Ahmed Yusuf","ahmed.yusuf@example.com",TicketPriority.High,now.AddDays(-4)));
        tickets.Add(NewTicket("TCK-2026-0007","Export to CSV truncates long fields","Grace Adeyemi","grace.adeyemi@example.com",TicketPriority.Normal,now.AddDays(-9)));

        // ---------- In Progress ----------
        Ticket t8=NewTicket("TCK-2026-0008","Two-factor codes arrive late","Marco Rossi","marco.rossi@example.com",TicketPriority.High,now.AddHours(-8));
        t8.Assign(aisha,now.AddHours(-7));
        t8.ChangeStatus(TicketStatus.InProgress,now.AddHours(-7));
        tickets.Add(t8);

        Ticket t9=NewTicket("TCK-2026-0009","Duplicate charge on renewal","Sofia Lindqvist","sofia.l@example.com",TicketPriority.Critical,now.AddHours(-3));
        t9.Assign(daniel,now.AddHours(-2));
        t9.ChangeStatus(TicketStatus.InProgress,now.AddHours(-2));
        tickets.Add(t9);

        Ticket t10=NewTicket("TCK-2026-0010","Dashboard charts fail to render in Safari","Ravi Shankar","ravi.shankar@example.com",TicketPriority.Normal,now.AddDays(-1));
        t10.Assign(meiLin,now.AddHours(-20));
        t10.ChangeStatus(TicketStatus.InProgress,now.AddHours(-20));
        tickets.Add(t10);

        // ---------- In Progress and OVERDUE ----------
        Ticket t11=NewTicket("TCK-2026-0011","API returns 500 on bulk import","Elena Petrova","elena.petrova@example.com",TicketPriority.Critical,now.AddDays(-3));
        t11.Assign(aisha,now.AddDays(-3));
        t11.ChangeStatus(TicketStatus.InProgress,now.AddDays(-3));
        tickets.Add(t11);

        Ticket t12=NewTicket("TCK-2026-0012","Refund not reflected after 10 days","Owen Price","owen.price@example.com",TicketPriority.High,now.AddDays(-5));
        t12.Assign(daniel,now.AddDays(-5));
        t12.ChangeStatus(TicketStatus.InProgress,now.AddDays(-5));
        tickets.Add(t12);

        // ---------- Resolved ----------
        Ticket t13=NewTicket("TCK-2026-0013","Cannot upload attachments over 5MB","Hana Kim","hana.kim@example.com",TicketPriority.Normal,now.AddDays(-6));
        t13.Assign(meiLin,now.AddDays(-6));
        t13.ChangeStatus(TicketStatus.InProgress,now.AddDays(-6));
        t13.ChangeStatus(TicketStatus.Resolved,now.AddDays(-4));
        tickets.Add(t13);

        Ticket t14=NewTicket("TCK-2026-0014","Tax rate incorrect for EU customers","Pierre Dubois","pierre.dubois@example.com",TicketPriority.High,now.AddDays(-8));
        t14.Assign(daniel,now.AddDays(-8));
        t14.ChangeStatus(TicketStatus.InProgress,now.AddDays(-8));
        t14.ChangeStatus(TicketStatus.Resolved,now.AddDays(-7));
        tickets.Add(t14);

        Ticket t15=NewTicket("TCK-2026-0015","Notification emails go to spam","Amara Diallo","amara.diallo@example.com",TicketPriority.Low,now.AddDays(-10));
        t15.Assign(carlos,now.AddDays(-10));
        t15.ChangeStatus(TicketStatus.InProgress,now.AddDays(-9));
        t15.ChangeStatus(TicketStatus.Resolved,now.AddDays(-5));
        tickets.Add(t15);

        Ticket t16=NewTicket("TCK-2026-0016","Search returns no results for partial names","Jonas Weber","jonas.weber@example.com",TicketPriority.Normal,now.AddDays(-7));
        t16.Assign(aisha,now.AddDays(-7));
        t16.ChangeStatus(TicketStatus.InProgress,now.AddDays(-6));
        t16.ChangeStatus(TicketStatus.Resolved,now.AddDays(-2));
        tickets.Add(t16);

        // ---------- Closed ----------
        Ticket t17=NewTicket("TCK-2026-0017","Change billing contact email","Ingrid Larsen","ingrid.larsen@example.com",TicketPriority.Low,now.AddDays(-14));
        t17.Assign(daniel,now.AddDays(-14));
        t17.ChangeStatus(TicketStatus.InProgress,now.AddDays(-13));
        t17.ChangeStatus(TicketStatus.Resolved,now.AddDays(-12));
        t17.ChangeStatus(TicketStatus.Closed,now.AddDays(-11));
        tickets.Add(t17);

        Ticket t18=NewTicket("TCK-2026-0018","Mobile app crashes on startup","Diego Alvarez","diego.alvarez@example.com",TicketPriority.Critical,now.AddDays(-16));
        t18.Assign(meiLin,now.AddDays(-16));
        t18.ChangeStatus(TicketStatus.InProgress,now.AddDays(-16));
        t18.ChangeStatus(TicketStatus.Resolved,now.AddDays(-15));
        t18.ChangeStatus(TicketStatus.Closed,now.AddDays(-14));
        tickets.Add(t18);

        Ticket t19=NewTicket("TCK-2026-0019","Request API rate limit increase","Fatima Zahra","fatima.zahra@example.com",TicketPriority.Normal,now.AddDays(-20));
        t19.Assign(carlos,now.AddDays(-20));
        t19.ChangeStatus(TicketStatus.InProgress,now.AddDays(-19));
        t19.ChangeStatus(TicketStatus.Resolved,now.AddDays(-18));
        t19.ChangeStatus(TicketStatus.Closed,now.AddDays(-17));
        tickets.Add(t19);

        // ---------- Reopened, then resolved again ----------
        Ticket t20=NewTicket("TCK-2026-0020","Timezone wrong on scheduled reports","Sam Okonkwo","sam.okonkwo@example.com",TicketPriority.High,now.AddDays(-12));
        t20.Assign(aisha,now.AddDays(-12));
        t20.ChangeStatus(TicketStatus.InProgress,now.AddDays(-12));
        t20.ChangeStatus(TicketStatus.Resolved,now.AddDays(-11));
        t20.ChangeStatus(TicketStatus.InProgress,now.AddDays(-9));   // reopened
        tickets.Add(t20);

        // ---------- A few comment threads ----------
        t8.AddComment("Aisha Rahman","Reproduced on staging. Looks like the SMS provider is throttling us.",now.AddHours(-6));
        t8.AddComment("Marco Rossi","Thanks - it happened again this morning around 09:15.",now.AddHours(-5));
        t11.AddComment("Aisha Rahman","Bulk import over 1000 rows times out. Investigating the batch size.",now.AddDays(-2));
        t13.AddComment("Mei Lin Tan","Raised the upload limit to 25MB and redeployed.",now.AddDays(-4));
        t20.AddComment("Sam Okonkwo","Reopening - reports are still an hour out for AEST customers.",now.AddDays(-9));

        db.Tickets.AddRange(tickets);
        db.SaveChanges();
    }

    private static Agent NewAgent(string fullName,string email,Department department,bool isActive)
    {
        Agent agent=new Agent();
        agent.FullName=fullName;
        agent.Email=email;
        agent.Department=department;
        agent.IsActive=isActive;
        return agent;
    }

    private static Ticket NewTicket(string reference,string title,string customerName,string customerEmail,TicketPriority priority,DateTime createdAt)
    {
        return new Ticket(reference,title,title+".",customerName,customerEmail,priority,createdAt);
    }
}