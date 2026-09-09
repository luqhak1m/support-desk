import {Component,OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {RouterLink} from '@angular/router';
import {Subject,debounceTime,distinctUntilChanged} from 'rxjs';

import {TicketService} from '../../services/ticket.service';
import {AgentService} from '../../services/agent.service';
import {TicketListItem,Agent,TicketQuery} from '../../models/ticket.model';

@Component({
  selector:'app-ticket-list',
  standalone:true,
  imports:[CommonModule,FormsModule,RouterLink],
  templateUrl:'./ticket-list.component.html',
  styleUrl:'./ticket-list.component.css'
})
export class TicketListComponent implements OnInit{

  tickets:TicketListItem[]=[];
  agents:Agent[]=[];

  loading=false;
  errorMessage='';

  // filters
  search='';
  status='';
  priority='';
  agentId:number|null=null;
  overdueOnly=false;

  // paging
  page=1;
  pageSize=10;
  totalCount=0;
  totalPages=0;

  statuses=['New','InProgress','Resolved','Closed'];
  priorities=['Low','Normal','High','Critical'];

  // RxJS: typing in the search box should not fire a request per keystroke
  private searchChanged=new Subject<string>();

  constructor(private ticketService:TicketService,private agentService:AgentService){}

  ngOnInit():void{
    this.searchChanged.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(value=>{
      this.search=value;
      this.page=1;
      this.load();
    });

    this.agentService.getAgents().subscribe(agents=>{
      this.agents=agents;
    });

    this.load();
  }

  onSearchInput(value:string):void{
    this.searchChanged.next(value);
  }

  onFilterChange():void{
    this.page=1;
    this.load();
  }

  load():void{
    this.loading=true;
    this.errorMessage='';

    const query:TicketQuery={
      page:this.page,
      pageSize:this.pageSize,
      search:this.search,
      status:this.status,
      priority:this.priority,
      agentId:this.agentId,
      overdueOnly:this.overdueOnly
    };

    this.ticketService.getTickets(query).subscribe({
      next:result=>{
        this.tickets=result.items;
        this.totalCount=result.totalCount;
        this.totalPages=result.totalPages;
        this.loading=false;
      },
      error:()=>{
        this.errorMessage='Could not load tickets. Is the API running?';
        this.loading=false;
      }
    });
  }

  previousPage():void{
    if(this.page>1){
      this.page=this.page-1;
      this.load();
    }
  }

  nextPage():void{
    if(this.page<this.totalPages){
      this.page=this.page+1;
      this.load();
    }
  }
}
