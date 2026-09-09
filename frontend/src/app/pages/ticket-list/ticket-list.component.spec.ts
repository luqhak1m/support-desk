import {ComponentFixture,TestBed,fakeAsync,tick} from '@angular/core/testing';
import {provideHttpClient} from '@angular/common/http';
import {HttpTestingController,provideHttpClientTesting} from '@angular/common/http/testing';
import {provideRouter} from '@angular/router';

import {TicketListComponent} from './ticket-list.component';
import {PagedResult,TicketListItem} from '../../models/ticket.model';

describe('TicketListComponent',()=>{
  let fixture:ComponentFixture<TicketListComponent>;
  let component:TicketListComponent;
  let httpMock:HttpTestingController;

  const ticketsUrl='http://localhost:5116/api/tickets';
  const agentsUrl='http://localhost:5116/api/agents';

  function page(items:TicketListItem[]):PagedResult<TicketListItem>{
    return {items,page:1,pageSize:10,totalCount:items.length,totalPages:1};
  }

  beforeEach(async()=>{
    await TestBed.configureTestingModule({
      imports:[TicketListComponent],
      providers:[provideHttpClient(),provideHttpClientTesting(),provideRouter([])]
    }).compileComponents();

    fixture=TestBed.createComponent(TicketListComponent);
    component=fixture.componentInstance;
    httpMock=TestBed.inject(HttpTestingController);
  });

  // Catches a search box wired straight to the API (one request per keystroke,
  // where the brief asks for a debounce) and overdue highlighting computed in
  // the browser instead of taken from the server's isOverdue flag.
  it('debounces the search and highlights only the rows the server marked overdue',fakeAsync(()=>{
    fixture.detectChanges();                    // ngOnInit
    httpMock.expectOne(r=>r.url===agentsUrl).flush([]);

    const overdue:TicketListItem={
      id:5,reference:'TCK-2026-0005',title:'Payment gateway rejects all cards',
      customerName:'Lucia Moreno',priority:'Critical',status:'New',
      assignedAgentName:null,dueAt:'2026-09-07T09:00:00Z',isOverdue:true
    };
    const onTime:TicketListItem={
      id:1,reference:'TCK-2026-0001',title:'Cannot log in',
      customerName:'Zara Ismail',priority:'High',status:'New',
      assignedAgentName:'Aisha Rahman',dueAt:'2026-09-30T09:00:00Z',isOverdue:false
    };

    httpMock.expectOne(r=>r.url===ticketsUrl).flush(page([overdue,onTime]));
    fixture.detectChanges();

    const rows:HTMLElement[]=fixture.nativeElement.querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
    expect(rows[0].classList).toContain('overdue');
    expect(rows[1].classList).not.toContain('overdue');

    // five keystrokes in quick succession must produce one request, not five
    component.onSearchInput('p');
    component.onSearchInput('pa');
    component.onSearchInput('pay');
    component.onSearchInput('paym');
    component.onSearchInput('payment');

    tick(300);

    const requests=httpMock.match(r=>r.url===ticketsUrl);
    expect(requests.length).toBe(1);
    expect(requests[0].request.params.get('search')).toBe('payment');
    expect(requests[0].request.params.get('page')).toBe('1');   // back to page 1
    requests[0].flush(page([]));

    httpMock.verify();
  }));
});
