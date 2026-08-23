import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [RouterLink, MatButtonModule],
  template: `
    <div class="status-page">
      <h1>403</h1>
      <p>You do not have permission to view this page.</p>
      <a mat-flat-button color="primary" routerLink="/">Go home</a>
    </div>
  `,
  styles: [
    `
      .status-page {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        min-height: 60vh;
        gap: 0.5rem;
        text-align: center;
      }
      h1 {
        font-size: 3rem;
        margin: 0;
      }
    `,
  ],
})
export class UnauthorizedComponent {}
