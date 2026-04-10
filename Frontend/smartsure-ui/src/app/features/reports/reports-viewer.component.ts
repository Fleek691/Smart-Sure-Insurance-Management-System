import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AdminService } from '../../core/services/admin.service';
import { SharedModule } from '../../shared/shared.module';

@Component({
  selector: 'app-reports-viewer',
  standalone: true,
  imports: [CommonModule, FormsModule, SharedModule],
  template: `
    <section class="reports-page">
      <div class="surface-card">
        <div class="section-head">
          <div>
            <p class="eyebrow">Reports</p>
            <h2>Policy, claims, and revenue reports</h2>
          </div>
          <div class="mini-tabs">
            <button type="button" [class.active]="reportType === 'policies'" (click)="setType('policies')">Policies</button>
            <button type="button" [class.active]="reportType === 'claims'" (click)="setType('claims')">Claims</button>
            <button type="button" [class.active]="reportType === 'revenue'" (click)="setType('revenue')">Revenue</button>
          </div>
        </div>

        <div class="filters">
          <label><span>From</span><input type="date" name="from" [(ngModel)]="filters.from" /></label>
          <label><span>To</span><input type="date" name="to" [(ngModel)]="filters.to" /></label>
          <label *ngIf="reportType === 'policies'"><span>Type</span><input name="type" [(ngModel)]="filters.type" placeholder="Vehicle" /></label>
          <label *ngIf="reportType === 'claims'"><span>Status</span><input name="status" [(ngModel)]="filters.status" placeholder="APPROVED" /></label>
          <div class="filters-actions">
            <button type="button" class="primary-button" (click)="loadReport()">Load report</button>
            <button type="button" class="secondary-button" (click)="exportCurrent()" [disabled]="!reportId">Export</button>
          </div>
        </div>

        <div class="surface-strong" *ngIf="summary">
          <div class="detail-grid">
            <div><span>Report id</span><strong>{{ reportId || 'Generated locally' }}</strong></div>
            <div><span>Period</span><strong>{{ summary.period || 'Custom range' }}</strong></div>
            <div><span>Trend</span><strong>{{ summary.trend || 'Stable' }}</strong></div>
            <div><span>Total items</span><strong>{{ summary.totalCount || 0 }}</strong></div>
          </div>
        </div>

        <div class="table-wrap" *ngIf="reportType === 'policies'">
          <table>
            <thead><tr><th>Name</th><th>Value</th></tr></thead>
            <tbody>
              <tr *ngFor="let row of summaryRows">
                <td>{{ row.label }}</td>
                <td>{{ row.value }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="table-wrap" *ngIf="reportType === 'claims'">
          <table>
            <thead><tr><th>Status</th><th>Count</th></tr></thead>
            <tbody>
              <tr *ngFor="let row of summaryRows">
                <td>{{ row.label }}</td>
                <td>{{ row.value }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="table-wrap" *ngIf="reportType === 'revenue'">
          <table>
            <thead><tr><th>Label</th><th>Revenue</th></tr></thead>
            <tbody>
              <tr *ngFor="let row of summaryRows">
                <td>{{ row.label }}</td>
                <td>{{ row.value | formatCurrency }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </section>
  `,
  styles: [`
    :host { display: block; }
    .reports-page { display: grid; gap: 1rem; }
    .surface-card, .surface-strong { border-radius: 24px; padding: 1.25rem; border: 1px solid rgba(84, 101, 255, 0.14); background: rgba(255,255,255,0.84); box-shadow: var(--shadow); }
    .surface-strong { background: rgba(255,255,255,0.72); box-shadow: none; }
    .section-head { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; margin-bottom: 1rem; }
    h2 { margin: 0; font-family: 'Space Grotesk', sans-serif; color: var(--ink); }
    .eyebrow { text-transform: uppercase; letter-spacing: 0.16em; font-size: 0.75rem; font-weight: 700; color: var(--muted); margin-bottom: 0.3rem; }
    .mini-tabs { display: flex; flex-wrap: wrap; gap: 0.55rem; }
    .mini-tabs button { border: 0; border-radius: 999px; padding: 0.7rem 0.95rem; background: rgba(84,101,255,0.1); color: var(--primary); font-weight: 700; cursor: pointer; }
    .mini-tabs button.active { background: linear-gradient(135deg, #5465ff, #788bff); color: white; }
    .filters { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 0.75rem; margin-bottom: 1rem; }
    label { display: grid; gap: 0.4rem; }
    label span { font-weight: 700; color: var(--ink); }
    input { border: 1px solid var(--line); background: rgba(255,255,255,0.82); border-radius: 16px; padding: 0.9rem 1rem; color: var(--ink); }
    .filters-actions { display: flex; gap: 0.75rem; align-items: end; }
    .primary-button, .secondary-button { border: 0; border-radius: 16px; padding: 0.95rem 1.1rem; font-weight: 700; cursor: pointer; height: fit-content; }
    .primary-button { color: white; background: linear-gradient(135deg, #5465ff, #788bff); }
    .secondary-button { background: rgba(84,101,255,0.1); color: var(--primary); }
    .detail-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 0.85rem; }
    .detail-grid div { border-radius: 16px; background: rgba(255,255,255,0.72); padding: 0.95rem; display: grid; gap: 0.35rem; border: 1px solid rgba(84,101,255,0.08); }
    .detail-grid span { color: var(--muted); }
    .detail-grid strong { color: var(--ink); }
    .table-wrap { margin-top: 1rem; overflow: auto; }
    table { width: 100%; border-collapse: collapse; }
    th, td { padding: 0.85rem; border-bottom: 1px solid rgba(84,101,255,0.1); text-align: left; }
    th { color: var(--muted); text-transform: uppercase; letter-spacing: 0.1em; font-size: 0.78rem; }
    .muted { color: var(--muted); }
    @media (max-width: 920px) { .filters, .detail-grid { grid-template-columns: 1fr; } }
  `]
})
export class ReportsViewerComponent implements OnInit {
  reportType: 'policies' | 'claims' | 'revenue' = 'policies';
  reportId = '';
  summary: { period?: string; trend?: string; totalCount?: number } | null = null;
  summaryRows: Array<{ label: string; value: number }> = [];
  filters = { from: '', to: '', type: '', status: '' };

  constructor(private readonly adminService: AdminService, private readonly route: ActivatedRoute, private readonly router: Router) {}

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      const type = params['type'] as string | undefined;
      if (type === 'claims' || type === 'revenue' || type === 'policies') {
        this.reportType = type;
      }
      this.loadReport();
    });

    if (this.router.url.endsWith('/reports')) {
      this.loadReport();
    }
  }

  setType(type: 'policies' | 'claims' | 'revenue'): void {
    void this.router.navigateByUrl(`/admin/reports/${type}`);
  }

  loadReport(): void {
    if (this.reportType === 'claims') {
      this.adminService.getClaimsReport(this.filters.from || undefined, this.filters.to || undefined, this.filters.status || undefined).subscribe((report) => {
        this.reportId = report.reportId ?? '';
        this.summary = { period: report.period, totalCount: report.totalCount };
        this.summaryRows = Object.entries(report.statusBreakdown ?? {}).map(([label, value]) => ({ label, value }));
      });
      return;
    }

    if (this.reportType === 'revenue') {
      this.adminService.getRevenueReport(this.filters.from || undefined, this.filters.to || undefined).subscribe((report) => {
        this.reportId = report.reportId ?? '';
        this.summary = { period: report.period, trend: report.trend, totalCount: report.data?.length ?? 0 };
        this.summaryRows = (report.data ?? []).map((point) => ({ label: point.label, value: point.value }));
      });
      return;
    }

    this.adminService.getPolicyReport(this.filters.from || undefined, this.filters.to || undefined, this.filters.type || undefined).subscribe((report) => {
      this.reportId = report.reportId ?? '';
      this.summary = { period: report.period, totalCount: report.totalCount };
      this.summaryRows = [
        { label: 'Rows returned', value: report.totalCount ?? 0 },
        { label: 'Filters applied', value: Number(Boolean(this.filters.from || this.filters.to || this.filters.type)) }
      ];
    });
  }

  exportCurrent(): void {
    if (!this.reportId) {
      return;
    }

    this.adminService.exportReport(this.reportId).subscribe((blob) => {
      const url = URL.createObjectURL(blob);
      const anchor = document.createElement('a');
      anchor.href = url;
      anchor.download = `${this.reportType}-report.pdf`;
      anchor.click();
      URL.revokeObjectURL(url);
    });
  }
}
