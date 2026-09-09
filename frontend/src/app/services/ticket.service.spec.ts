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

  afterEach(()=>{
    httpMock.verify();
  });

  it('sends filters to the server as query parameters, not filtering on the client',()=>{
    const empty:PagedResult<TicketListItem>={items:[],page:1,pageSize:10,totalCount:0,totalPages:0};

    service.getTickets({
      page:2,
      pageSize:10,
      search:'login',
      status:'New',
      priority:'High',
      overdueOnly:true
    }).subscribe();

    const request=httpMock.expectOne(r=>r.url==='http://localhost:5116/api/tickets');

    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.get('search')).toBe('login');
    expect(request.request.params.get('status')).toBe('New');
    expect(request.request.params.get('priority')).toBe('High');
    expect(request.request.params.get('overdueOnly')).toBe('true');

    request.flush(empty);
  });

  it('posts a status change to the dedicated status endpoint',()=>{
    service.changeStatus(7,'Resolved').subscribe();

    const request=httpMock.expectOne('http://localhost:5116/api/tickets/7/status');

    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({status:'Resolved'});

    request.flush({});
  });
});
