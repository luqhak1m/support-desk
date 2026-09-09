import {TestBed} from '@angular/core/testing';
import {provideHttpClient} from '@angular/common/http';
import {HttpTestingController,provideHttpClientTesting} from '@angular/common/http/testing';

import {TicketService} from './ticket.service';
import {PagedResult,TicketListItem} from '../models/ticket.model';

describe('TicketService',()=>{
  let service:TicketService;
  let httpMock:HttpTestingController;

  beforeEach(()=>{
    TestBed.configureTestingModule({
      providers:[provideHttpClient(),provideHttpClientTesting()]
    });
    service=TestBed.inject(TicketService);
    httpMock=TestBed.inject(HttpTestingController);
  });

  afterEach(()=>httpMock.verify());

  // Catches the two things the brief is strict about: filtering done in the
  // browser instead of on the server, and a status change folded into a
  // generic update instead of its own endpoint.
  it('filters on the server and changes status through its own endpoint',()=>{
    const empty:PagedResult<TicketListItem>={items:[],page:1,pageSize:10,totalCount:0,totalPages:0};

    service.getTickets({
      page:2,pageSize:25,search:'login',status:'New',
      priority:'High',agentId:3,overdueOnly:true
    }).subscribe();

    const list=httpMock.expectOne(r=>r.url==='http://localhost:5116/api/tickets');
    expect(list.request.method).toBe('GET');
    expect(list.request.params.get('page')).toBe('2');
    expect(list.request.params.get('search')).toBe('login');
    expect(list.request.params.get('status')).toBe('New');
    expect(list.request.params.get('priority')).toBe('High');
    expect(list.request.params.get('agentId')).toBe('3');
    expect(list.request.params.get('overdueOnly')).toBe('true');
    list.flush(empty);

    // filters that are not set must not be sent as blanks, or the API
    // would treat an empty string as a real filter
    service.getTickets({page:1,pageSize:10}).subscribe();
    const bare=httpMock.expectOne(r=>r.url==='http://localhost:5116/api/tickets');
    expect(bare.request.params.has('search')).toBeFalse();
    expect(bare.request.params.has('status')).toBeFalse();
    bare.flush(empty);

    service.changeStatus(7,'Resolved').subscribe();
    const status=httpMock.expectOne('http://localhost:5116/api/tickets/7/status');
    expect(status.request.method).toBe('POST');
    expect(status.request.body).toEqual({status:'Resolved'});
    status.flush({});
  });
});
