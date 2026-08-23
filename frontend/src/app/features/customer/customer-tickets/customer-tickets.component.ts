import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { TicketService } from '../../../core/services/ticket.service';
import { Ticket } from '../../../core/models';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

@Component({
  selector: 'app-customer-tickets',
  standalone: true,
  imports: [CommonModule, RouterLink, MatTableModule, MatButtonModule, MatPaginatorModule, PageHeaderComponent],
  templateUrl: './customer-tickets.component.html',
  styleUrl: './customer-tickets.component.scss',
})
export class CustomerTicketsComponent implements OnInit {
  displayedColumns = ['title', 'status', 'priority', 'createdAt'];
  tickets: Ticket[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;

  constructor(private ticketService: TicketService, private router: Router) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.ticketService
      .getTickets({ page: this.pageIndex + 1, pageSize: this.pageSize })
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
