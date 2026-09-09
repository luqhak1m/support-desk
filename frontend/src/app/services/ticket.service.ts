import {Injectable} from '@angular/core';
import {HttpClient,HttpParams} from '@angular/common/http';
import {Observable} from 'rxjs';
import {PagedResult,TicketListItem,TicketDetail,TicketQuery,TicketPayload,TicketStatus} from '../models/ticket.model';

// All HTTP for tickets lives here. Components never touch HttpClient.
@Injectable({providedIn:'root'})
export class TicketService{

  private baseUrl='http://localhost:5116/api/tickets';

  constructor(private http:HttpClient){}

  getTickets(query:TicketQuery):Observable<PagedResult<TicketListItem>>{
    let params=new HttpParams();

    if(query.page){params=params.set('page',query.page);}
    if(query.pageSize){params=params.set('pageSize',query.pageSize);}
    if(query.search){params=params.set('search',query.search);}
    if(query.status){params=params.set('status',query.status);}
    if(query.priority){params=params.set('priority',query.priority);}
    if(query.agentId){params=params.set('agentId',query.agentId);}
    if(query.overdueOnly){params=params.set('overdueOnly',true);}

    return this.http.get<PagedResult<TicketListItem>>(this.baseUrl,{params});
  }

  getTicket(id:number):Observable<TicketDetail>{
    return this.http.get<TicketDetail>(this.baseUrl+'/'+id);
  }

  createTicket(payload:TicketPayload):Observable<TicketDetail>{
    return this.http.post<TicketDetail>(this.baseUrl,payload);
  }

  updateTicket(id:number,payload:TicketPayload):Observable<TicketDetail>{
    return this.http.put<TicketDetail>(this.baseUrl+'/'+id,payload);
  }

  deleteTicket(id:number):Observable<void>{
    return this.http.delete<void>(this.baseUrl+'/'+id);
  }

  changeStatus(id:number,status:TicketStatus):Observable<TicketDetail>{
    return this.http.post<TicketDetail>(this.baseUrl+'/'+id+'/status',{status});
  }

  assignAgent(id:number,agentId:number):Observable<TicketDetail>{
    return this.http.post<TicketDetail>(this.baseUrl+'/'+id+'/assign',{agentId});
  }

  unassignAgent(id:number):Observable<TicketDetail>{
    return this.http.post<TicketDetail>(this.baseUrl+'/'+id+'/unassign',{});
  }

  addComment(id:number,authorName:string,body:string):Observable<TicketDetail>{
    return this.http.post<TicketDetail>(this.baseUrl+'/'+id+'/comments',{authorName,body});
  }
}
