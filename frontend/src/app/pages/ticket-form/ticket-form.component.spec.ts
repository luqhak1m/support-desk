import {ComponentFixture,TestBed} from '@angular/core/testing';
import {provideHttpClient} from '@angular/common/http';
import {provideHttpClientTesting} from '@angular/common/http/testing';
import {provideRouter} from '@angular/router';

import {TicketFormComponent} from './ticket-form.component';

describe('TicketFormComponent',()=>{
  let component:TicketFormComponent;
  let fixture:ComponentFixture<TicketFormComponent>;

  beforeEach(async()=>{
    await TestBed.configureTestingModule({
      imports:[TicketFormComponent],
      providers:[provideHttpClient(),provideHttpClientTesting(),provideRouter([])]
    }).compileComponents();

    fixture=TestBed.createComponent(TicketFormComponent);
    component=fixture.componentInstance;
    fixture.detectChanges();
  });

  it('starts invalid and rejects a malformed customer email',()=>{
    expect(component.form.valid).toBeFalse();

    component.form.setValue({
      title:'Cannot log in',
      description:'The portal rejects my password.',
      customerName:'Zara Ismail',
      customerEmail:'not-an-email',
      priority:'High'
    });

    expect(component.form.valid).toBeFalse();
    expect(component.form.get('customerEmail')?.hasError('email')).toBeTrue();
  });

  it('becomes valid once every required field is filled correctly',()=>{
    component.form.setValue({
      title:'Cannot log in',
      description:'The portal rejects my password.',
      customerName:'Zara Ismail',
      customerEmail:'zara.ismail@example.com',
      priority:'High'
    });

    expect(component.form.valid).toBeTrue();
  });
});
