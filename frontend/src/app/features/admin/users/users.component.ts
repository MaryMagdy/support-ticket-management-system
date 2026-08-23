import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { UserService } from '../../../core/services/user.service';
import { User } from '../../../core/models';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { UserDialogComponent } from './user-dialog/user-dialog.component';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatPaginatorModule,
    PageHeaderComponent,
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss',
})
export class UsersComponent implements OnInit {
  displayedColumns = ['name', 'email', 'role', 'actions'];
  users: User[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;

  constructor(private userService: UserService, private dialog: MatDialog) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.userService.getUsers(this.pageIndex + 1, this.pageSize).subscribe((res) => {
      this.users = res.items;
      this.totalCount = res.totalCount;
    });
  }

  onPage(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.load();
  }

  openCreate(): void {
    const ref = this.dialog.open(UserDialogComponent, { width: '400px', data: {} });
    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.userService.createUser(result).subscribe(() => this.load());
    });
  }

  openEdit(user: User): void {
    const ref = this.dialog.open(UserDialogComponent, { width: '400px', data: { user } });
    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.userService.updateUser(user.id, result).subscribe(() => this.load());
    });
  }

  remove(user: User): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      width: '350px',
      data: {
        title: 'Delete user',
        message: `Are you sure you want to delete ${user.fullName}?`,
      },
    });
    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.userService.deleteUser(user.id).subscribe(() => this.load());
    });
  }
}
