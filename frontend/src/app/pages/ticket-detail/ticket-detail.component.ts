import {Component,OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {ActivatedRoute,Router,RouterLink} from '@angular/router';

import {TicketService} from '../../services/ticket.service';
import {AgentService} from '../../services/agent.service';
import {TicketDetail,Agent,TicketStatus} from '../../models/ticket.model';

@Component({
  selector:'app-ticket-detail',
  standalone:true,
  imports:[CommonModule,FormsModule,RouterLink],
  templateUrl:'./ticket-detail.component.html',
  styleUrl:'./ticket-detail.component.css'
})
export class TicketDetailComponent implements OnInit{

  ticket:TicketDetail|null=null;
  agents:Agent[]=[];

  loading=false;
  errorMessage='';

  selectedAgentId:number|null=null;
  commentAuthor='';
  commentBody='';

  constructor(
    private route:ActivatedRoute,
    private router:Router,
    private ticketService:TicketService,
    private agentService:AgentService){}

  ngOnInit():void{
    const id=Number(this.route.snapshot.paramMap.get('id'));

    this.agentService.getAgents().subscribe(agents=>{
      this.agents=agents;
    });

    this.load(id);
  }

  load(id:number):void{
    this.loading=true;
    this.ticketService.getTicket(id).subscribe({
      next:ticket=>{
        this.apply(ticket);
        this.loading=false;
      },
      error:()=>{
        this.errorMessage='Could not load this ticket.';
        this.loading=false;
      }
    });
  }

  private apply(ticket:TicketDetail):void{
    this.ticket=ticket;
    this.selectedAgentId=ticket.assignedAgentId;
    this.errorMessage='';
  }

  // The server tells us which transitions are legal, so we just render them.
  changeStatus(status:TicketStatus):void{
    if(!this.ticket){return;}

    this.ticketService.changeStatus(this.ticket.id,status).subscribe({
      next:ticket=>this.apply(ticket),
      error:err=>this.showServerError(err)
    });
  }

  assign():void{
    if(!this.ticket||this.selectedAgentId===null){return;}

    this.ticketService.assignAgent(this.ticket.id,this.selectedAgentId).subscribe({
      next:ticket=>this.apply(ticket),
      error:err=>this.showServerError(err)
    });
  }

  unassign():void{
    if(!this.ticket){return;}

    this.ticketService.unassignAgent(this.ticket.id).subscribe({
      next:ticket=>this.apply(ticket),
      error:err=>this.showServerError(err)
    });
  }

  addComment():void{
    if(!this.ticket){return;}
    if(this.commentAuthor.trim()===''||this.commentBody.trim()===''){
      this.errorMessage='Author and comment are both required.';
      return;
    }

    this.ticketService.addComment(this.ticket.id,this.commentAuthor,this.commentBody).subscribe({
      next:ticket=>{
        this.apply(ticket);
        this.commentBody='';
      },
      error:err=>this.showServerError(err)
    });
  }

  deleteTicket():void{
    if(!this.ticket){return;}
    if(!confirm('Delete ticket '+this.ticket.reference+'? This cannot be undone.')){return;}

    this.ticketService.deleteTicket(this.ticket.id).subscribe({
      next:()=>this.router.navigate(['/tickets']),
      error:err=>this.showServerError(err)
    });
  }

  // Surface the API's ProblemDetails "detail" text, which explains why a rule rejected the call.
  private showServerError(err:any):void{
    if(err&&err.error&&err.error.detail){
      this.errorMessage=err.error.detail;
    }else{
      this.errorMessage='Something went wrong.';
    }
  }
}
