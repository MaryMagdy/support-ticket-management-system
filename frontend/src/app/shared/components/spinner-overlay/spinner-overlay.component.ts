import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LoadingService } from '../../../core/services/loading.service';

@Component({
  selector: 'app-spinner-overlay',
  standalone: true,
  imports: [CommonModule, MatProgressSpinnerModule],
  template: `
    <div class="spinner-overlay" *ngIf="loading.loading$ | async">
      <mat-spinner diameter="50"></mat-spinner>
    </div>
  `,
  styles: [
    `
      .spinner-overlay {
        position: fixed;
        inset: 0;
        background: rgba(0, 0, 0, 0.15);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 1000;
      }
    `,
  ],
})
export class SpinnerOverlayComponent {
  constructor(public loading: LoadingService) {}
}
