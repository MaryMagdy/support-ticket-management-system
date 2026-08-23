import { CommonModule } from '@angular/common';
import { Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { TicketService } from '../../../core/services/ticket.service';
import { Ticket, TicketPriority, TicketStatus } from '../../../core/models';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    PageHeaderComponent,
  ],
  templateUrl: './ticket-list.component.html',
  styleUrl: './ticket-list.component.scss',
})
export class TicketListComponent implements OnInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  displayedColumns = ['title', 'status', 'priority', 'assignedAgentName', 'createdAt', 'actions'];
  tickets: Ticket[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;

  searchControl = new FormControl('');
  statusControl = new FormControl<TicketStatus | null>(null);
  priorityControl = new FormControl<TicketPriority | null>(null);

  statuses = Object.values(TicketStatus);
  priorities = Object.values(TicketPriority);

  sortBy = 'CreatedAt';
  descending = true;

  constructor(private ticketService: TicketService, private router: Router) {}

  ngOnInit(): void {
    this.load();
    this.searchControl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe(() => {
        this.pageIndex = 0;
        this.load();
      });
    this.statusControl.valueChanges.subscribe(() => {
      this.pageIndex = 0;
      this.load();
    });
    this.priorityControl.valueChanges.subscribe(() => {
      this.pageIndex = 0;
      this.load();
    });
  }

  load(): void {
    this.ticketService
      .getTickets({
        page: this.pageIndex + 1,
        pageSize: this.pageSize,
        status: this.statusControl.value,
        priority: this.priorityControl.value,
        search: this.searchControl.value,
        sortBy: this.sortBy,
        descending: this.descending,
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

  onSort(sort: Sort): void {
    if (!sort.direction) {
      this.sortBy = 'CreatedAt';
      this.descending = true;
    } else {
      this.sortBy = sort.active;
      this.descending = sort.direction === 'desc';
    }
    this.load();
  }

  openTicket(id: number): void {
    this.router.navigate(['/tickets', id]);
  }
}
