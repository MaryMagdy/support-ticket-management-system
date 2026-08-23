import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink, MatButtonModule],
  template: `
    <div class="status-page">
      <h1>404</h1>
      <p>The page you are looking for does not exist.</p>
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
export class NotFoundComponent {}
