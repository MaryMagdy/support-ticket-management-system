import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { TicketService } from '../../../core/services/ticket.service';
import { Ticket, TicketPriority, TicketStatus } from '../../../core/models';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

@Component({
  selector: 'app-agent-tickets',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    PageHeaderComponent,
  ],
  templateUrl: './agent-tickets.component.html',
  styleUrl: './agent-tickets.component.scss',
})
export class AgentTicketsComponent implements OnInit {
  displayedColumns = ['title', 'status', 'priority', 'createdAt'];
  tickets: Ticket[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;

  searchControl = new FormControl('');
  statusControl = new FormControl<TicketStatus | null>(null);
  priorityControl = new FormControl<TicketPriority | null>(null);

  statuses = Object.values(TicketStatus);
  priorities = Object.values(TicketPriority);

  constructor(private ticketService: TicketService, private router: Router) {}

  ngOnInit(): void {
    this.load();
    this.searchControl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe(() => this.load());
    this.statusControl.valueChanges.subscribe(() => this.load());
    this.priorityControl.valueChanges.subscribe(() => this.load());
  }

  load(): void {
    this.ticketService
      .getTickets({
        page: this.pageIndex + 1,
        pageSize: this.pageSize,
        status: this.statusControl.value,
        priority: this.priorityControl.value,
        search: this.searchControl.value,
      })
      .subscribe((res) => {
        this.tickets = res.items;
        this.totalCount = res.totalCount;
      });
  }

  onPage(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.load();
  }

  openTicket(id: number): void {
    this.router.navigate(['/tickets', id]);
  }
}
