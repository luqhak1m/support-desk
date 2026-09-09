import {Injectable} from '@angular/core';
import {HttpClient,HttpParams} from '@angular/common/http';
import {Observable} from 'rxjs';
import {Agent} from '../models/ticket.model';

@Injectable({providedIn:'root'})
export class AgentService{

  private baseUrl='http://localhost:5116/api/agents';

  constructor(private http:HttpClient){}

  getAgents(search?:string):Observable<Agent[]>{
    let params=new HttpParams();
    if(search){params=params.set('search',search);}
    return this.http.get<Agent[]>(this.baseUrl,{params});
  }

  getAgent(id:number):Observable<Agent>{
    return this.http.get<Agent>(this.baseUrl+'/'+id);
  }
}
