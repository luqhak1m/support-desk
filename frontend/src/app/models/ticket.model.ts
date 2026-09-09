// Mirrors the DTOs returned by the API.

export type TicketStatus='New'|'InProgress'|'Resolved'|'Closed';
export type TicketPriority='Low'|'Normal'|'High'|'Critical';
export type Department='Technical'|'Billing'|'General';

export interface PagedResult<T>{
  items:T[];
  page:number;
  pageSize:number;
  totalCount:number;
  totalPages:number;
}

export interface TicketListItem{
  id:number;
  reference:string;
  title:string;
  customerName:string;
  priority:TicketPriority;
  status:TicketStatus;
  assignedAgentName:string|null;
  dueAt:string;
  isOverdue:boolean;
}

export interface Comment{
  id:number;
  authorName:string;
  body:string;
  createdAt:string;
}

export interface TicketDetail{
  id:number;
  reference:string;
  title:string;
  description:string;
  customerName:string;
  customerEmail:string;
  priority:TicketPriority;
  status:TicketStatus;
  assignedAgentId:number|null;
  assignedAgentName:string|null;
  createdAt:string;
  lastModifiedAt:string;
  resolvedAt:string|null;
  closedAt:string|null;
  dueAt:string;
  isOverdue:boolean;
  isReadOnly:boolean;
  availableTransitions:TicketStatus[];
  comments:Comment[];
}

export interface Agent{
  id:number;
  fullName:string;
  email:string;
  department:Department;
  isActive:boolean;
}

export interface TicketQuery{
  page?:number;
  pageSize?:number;
  search?:string;
  status?:string;
  priority?:string;
  agentId?:number|null;
  overdueOnly?:boolean;
}

export interface TicketPayload{
  title:string;
  description:string;
  customerName:string;
  customerEmail:string;
  priority:TicketPriority;
}
