import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { ChartConfiguration, ChartData } from 'chart.js';
import { NgChartsModule } from 'ng2-charts';
import { DashboardService } from '../../../core/services/dashboard.service';
import { DashboardSummary } from '../../../core/models';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatListModule, NgChartsModule, PageHeaderComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  summary: DashboardSummary | null = null;

  barChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    plugins: { legend: { display: false } },
  };

  barChartData: ChartData<'bar'> = {
    labels: ['Open', 'In Progress', 'Resolved', 'Closed'],
    datasets: [{ data: [0, 0, 0, 0], label: 'Tickets by status' }],
  };

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.dashboardService.getSummary().subscribe((summary) => {
      this.summary = summary;
      const counts = summary.countsByStatus || {};
      this.barChartData = {
        labels: ['Open', 'In Progress', 'Resolved', 'Closed'],
        datasets: [
          {
            data: [
              counts['Open'] || 0,
              counts['InProgress'] || 0,
              counts['Resolved'] || 0,
              counts['Closed'] || 0,
            ],
            label: 'Tickets by status',
          },
        ],
      };
    });
  }
}
