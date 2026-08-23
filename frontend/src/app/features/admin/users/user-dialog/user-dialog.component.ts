import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { User, UserRole } from '../../../../core/models';

export interface UserDialogData {
  user?: User;
}

@Component({
  selector: 'app-user-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
  ],
  templateUrl: './user-dialog.component.html',
  styles: ['.full-width { width: 100%; margin-bottom: 0.5rem; }'],
})
export class UserDialogComponent {
  roles = Object.values(UserRole);
  isEdit = !!this.data.user;

  form = this.fb.group({
    fullName: [this.data.user?.fullName || '', Validators.required],
    email: [this.data.user?.email || '', [Validators.required, Validators.email]],
    password: [''],
    role: [this.data.user?.role || UserRole.Customer, Validators.required],
  });

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<UserDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: UserDialogData
  ) {
    if (!this.isEdit) {
      this.form.controls.password.addValidators([Validators.required, Validators.minLength(6)]);
    }
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.dialogRef.close(this.form.getRawValue());
  }
}
