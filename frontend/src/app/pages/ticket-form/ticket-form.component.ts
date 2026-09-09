import {Component,OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormBuilder,FormGroup,Validators,ReactiveFormsModule} from '@angular/forms';
import {ActivatedRoute,Router,RouterLink} from '@angular/router';

import {TicketService} from '../../services/ticket.service';
import {TicketPayload} from '../../models/ticket.model';

@Component({
  selector:'app-ticket-form',
  standalone:true,
  imports:[CommonModule,ReactiveFormsModule,RouterLink],
  templateUrl:'./ticket-form.component.html',
  styleUrl:'./ticket-form.component.css'
})
export class TicketFormComponent implements OnInit{

  form:FormGroup;
  ticketId:number|null=null;
  saving=false;
  errorMessage='';

  priorities=['Low','Normal','High','Critical'];

  constructor(
    private fb:FormBuilder,
    private route:ActivatedRoute,
    private router:Router,
    private ticketService:TicketService){

    this.form=this.fb.group({
      title:['',[Validators.required,Validators.maxLength(200)]],
      description:['',[Validators.required]],
      customerName:['',[Validators.required,Validators.maxLength(200)]],
      customerEmail:['',[Validators.required,Validators.email]],
      priority:['Normal',[Validators.required]]
    });
  }

  ngOnInit():void{
    const idParam=this.route.snapshot.paramMap.get('id');

    if(idParam){
      this.ticketId=Number(idParam);
      this.ticketService.getTicket(this.ticketId).subscribe(ticket=>{
        this.form.patchValue({
          title:ticket.title,
          description:ticket.description,
          customerName:ticket.customerName,
          customerEmail:ticket.customerEmail,
          priority:ticket.priority
        });
      });
    }
  }

  get isEdit():boolean{
    return this.ticketId!==null;
  }

  // Show a field's error only once the user has touched it
  showError(field:string):boolean{
    const control=this.form.get(field);
    return control!==null&&control.invalid&&(control.dirty||control.touched);
  }

  save():void{
    this.errorMessage='';

    if(this.form.invalid){
      this.form.markAllAsTouched();
      return;
    }

    this.saving=true;
    const payload=this.form.value as TicketPayload;

    if(this.isEdit){
      this.ticketService.updateTicket(this.ticketId!,payload).subscribe({
        next:ticket=>this.router.navigate(['/tickets',ticket.id]),
        error:err=>this.showServerError(err)
      });
    }else{
      this.ticketService.createTicket(payload).subscribe({
        next:ticket=>this.router.navigate(['/tickets',ticket.id]),
        error:err=>this.showServerError(err)
      });
    }
  }

  // The API returns ProblemDetails: either a rule violation (detail) or field errors (errors)
  private showServerError(err:any):void{
    this.saving=false;

    if(err&&err.error&&err.error.errors){
      const messages:string[]=[];
      for(const key of Object.keys(err.error.errors)){
        for(const message of err.error.errors[key]){
          messages.push(message);
        }
      }
      this.errorMessage=messages.join(' ');
    }else if(err&&err.error&&err.error.detail){
      this.errorMessage=err.error.detail;
    }else{
      this.errorMessage='Could not save the ticket.';
    }
  }
}
