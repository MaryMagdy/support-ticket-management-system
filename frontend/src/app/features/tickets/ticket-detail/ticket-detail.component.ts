import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatCardModule } from '@angular/material/card';
import { TicketService } from '../../../core/services/ticket.service';
import { CommentService } from '../../../core/services/comment.service';
import { TimeEntryService } from '../../../core/services/time-entry.service';
import { AuthService } from '../../../core/services/auth.service';
import { UserService } from '../../../core/services/user.service';
import {
  ActivityLogEntry,
  Comment,
  Ticket,
  TicketPriority,
  TicketStatus,
  TimeEntry,
  User,
  UserRole,
} from '../../../core/models';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

@Component({
  selector: 'app-ticket-detail',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTabsModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatButtonModule,
    MatListModule,
    MatCardModule,
    PageHeaderComponent,
  ],
  templateUrl: './ticket-detail.component.html',
  styleUrl: './ticket-detail.component.scss',
})
export class TicketDetailComponent implements OnInit {
  ticket: Ticket | null = null;
  comments: Comment[] = [];
  timeEntries: TimeEntry[] = [];
  activity: ActivityLogEntry[] = [];
  agents: User[] = [];

  statuses = Object.values(TicketStatus);
  priorities = Object.values(TicketPriority);

  commentForm = this.fb.group({
    text: ['', Validators.required],
  });

  timeEntryForm = this.fb.group({
    durationMinutes: [30, [Validators.required, Validators.min(1)]],
    description: [''],
  });

  ticketId = 0;

  constructor(
    private route: ActivatedRoute,
    private fb: FormBuilder,
    private ticketService: TicketService,
    private commentService: CommentService,
    private timeEntryService: TimeEntryService,
    private userService: UserService,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    this.ticketId = Number(this.route.snapshot.paramMap.get('id')) || 0;
    if (this.ticketId) {
      this.loadTicket();
      this.loadComments();
      this.loadTimeEntries();
      this.loadActivity();
      if (this.isAdmin) this.loadAgents();
    }
  }

  get isAgentOrAdmin(): boolean {
    return this.authService.hasRole(UserRole.Admin, UserRole.SupportAgent);
  }

  get isAdmin(): boolean {
    return this.authService.hasRole(UserRole.Admin);
  }

  /** Customers can't change status/priority in general, but they can close a resolved ticket. */
  get canCloseAsCustomer(): boolean {
    return (
      this.authService.hasRole(UserRole.Customer) &&
      this.ticket?.status === TicketStatus.Resolved
    );
  }

  closeTicket(): void {
    this.updateStatus(TicketStatus.Closed);
  }

  loadAgents(): void {
    this.userService.getUsers(UserRole.SupportAgent).subscribe((agents) => (this.agents = agents));
  }

  get totalMinutes(): number {
    return this.timeEntries.reduce((sum, e) => sum + e.durationMinutes, 0);
  }

  loading = true;
  notFound = false;

  loadTicket(): void {
    this.loading = true;
    this.ticketService.getTicket(this.ticketId).subscribe({
      next: (t) => {
        this.ticket = t;
        this.loading = false;
      },
      error: () => {
        this.notFound = true;
        this.loading = false;
      },
    });
  }

  loadComments(): void {
    this.commentService.getComments(this.ticketId).subscribe((c) => (this.comments = c));
  }

  loadTimeEntries(): void {
    this.timeEntryService
      .getTimeEntries(this.ticketId)
      .subscribe((entries) => (this.timeEntries = entries));
  }

  loadActivity(): void {
    this.ticketService.getActivity(this.ticketId).subscribe((entries) => (this.activity = entries));
  }

  updateStatus(status: TicketStatus): void {
    this.ticketService
      .updateTicket(this.ticketId, { status, rowVersion: this.ticket?.rowVersion })
      .subscribe({
        next: (t) => {
          this.ticket = t;
          this.loadActivity();
        },
        error: () => this.reloadOnConflict(),
      });
  }

  updatePriority(priority: TicketPriority): void {
    this.ticketService
      .updateTicket(this.ticketId, { priority, rowVersion: this.ticket?.rowVersion })
      .subscribe({
        next: (t) => {
          this.ticket = t;
          this.loadActivity();
        },
        error: () => this.reloadOnConflict(),
      });
  }

  assignAgent(assignedAgentId: number | null): void {
    this.ticketService
      .updateTicket(this.ticketId, { assignedAgentId, rowVersion: this.ticket?.rowVersion })
      .subscribe({
        next: (t) => {
          this.ticket = t;
          this.loadActivity();
        },
        error: () => this.reloadOnConflict(),
      });
  }

  /** On a 409 (stale RowVersion) the error interceptor already shows the conflict message; reload so the form reflects the current state before the user retries. */
  private reloadOnConflict(): void {
    this.loadTicket();
    this.loadActivity();
  }

  addComment(): void {
    if (this.commentForm.invalid) return;
    const text = this.commentForm.value.text!;
    this.commentService.addComment(this.ticketId, { text }).subscribe((c) => {
      this.comments.push(c);
      this.commentForm.reset();
      this.loadActivity();
    });
  }

  addTimeEntry(): void {
    if (this.timeEntryForm.invalid) return;
    const { durationMinutes, description } = this.timeEntryForm.getRawValue();
    this.timeEntryService
      .addTimeEntry(this.ticketId, {
        workDate: new Date().toISOString(),
        durationMinutes: durationMinutes!,
        description: description || undefined,
      })
      .subscribe((entry) => {
        this.timeEntries.push(entry);
        this.timeEntryForm.reset({ durationMinutes: 30, description: '' });
      });
  }
}
