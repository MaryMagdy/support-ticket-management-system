import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page-header">
      <h1>{{ title }}</h1>
      <p *ngIf="subtitle">{{ subtitle }}</p>
    </div>
  `,
  styles: [
    `
      .page-header {
        margin-bottom: 1.5rem;
      }
      .page-header h1 {
        margin: 0 0 0.25rem 0;
        font-size: 1.5rem;
        font-weight: 600;
      }
      .page-header p {
        margin: 0;
        color: rgba(0, 0, 0, 0.6);
      }
    `,
  ],
})
export class PageHeaderComponent {
  @Input() title = '';
  @Input() subtitle = '';
}
