import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AdminService } from '../../core/services/admin.service';
import { ClaimsService } from '../../core/services/claims.service';
import { PolicyService } from '../../core/services/policy.service';
import { ClaimDto, PolicyDto } from '../../models/api-models';
import { SharedModule } from '../../shared/shared.module';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

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
          <label *ngIf="reportType === 'policies'"><span>Type</span><input name="type" [(ngModel)]="filters.type" placeholder="e.g. Vehicle" /></label>
          <label *ngIf="reportType === 'claims'"><span>Status</span>
            <select name="status" [(ngModel)]="filters.status">
              <option value="">All</option>
              <option value="DRAFT">Draft</option>
              <option value="SUBMITTED">Submitted</option>
              <option value="UNDER_REVIEW">Under review</option>
              <option value="APPROVED">Approved</option>
              <option value="REJECTED">Rejected</option>
            </select>
          </label>
          <div class="filters-actions">
            <button type="button" class="primary-button" (click)="loadReport()">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="1 4 1 10 7 10"/><path d="M3.51 15a9 9 0 102.13-9.36L1 10"/></svg>
              Load report
            </button>
            <button type="button" class="secondary-button" (click)="downloadReport()" [disabled]="!loaded">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
              Download PDF
            </button>
          </div>
        </div>

        <!-- ── SUMMARY STRIP ── -->
        <div class="summary-strip" *ngIf="loaded">
          <div class="summary-item" *ngFor="let s of summaryCards">
            <span>{{ s.label }}</span>
            <strong>{{ s.value }}</strong>
          </div>
        </div>

        <!-- ── POLICIES TABLE ── -->
        <div class="table-wrap" *ngIf="reportType === 'policies' && loaded">
          <table>
            <thead><tr>
              <th>Policy #</th><th>Type</th><th>Sub-type</th><th>Status</th>
              <th>Coverage</th><th>Premium/mo</th><th>Issued</th>
            </tr></thead>
            <tbody>
              <tr *ngFor="let p of filteredPolicies">
                <td class="mono">{{ p.policyNumber }}</td>
                <td>{{ p.typeName }}</td>
                <td>{{ p.subTypeName }}</td>
                <td><app-status-badge [value]="p.status"></app-status-badge></td>
                <td>{{ p.coverageAmount | formatCurrency }}</td>
                <td>{{ p.monthlyPremium | formatCurrency }}</td>
                <td>{{ p.issuedDate | date:'mediumDate' }}</td>
              </tr>
              <tr *ngIf="!filteredPolicies.length"><td colspan="7" class="empty-cell">No policies match the current filters</td></tr>
            </tbody>
          </table>
        </div>

        <!-- ── CLAIMS TABLE ── -->
        <div class="table-wrap" *ngIf="reportType === 'claims' && loaded">
          <table>
            <thead><tr>
              <th>Claim #</th><th>Description</th><th>Status</th>
              <th>Amount</th><th>Filed on</th>
            </tr></thead>
            <tbody>
              <tr *ngFor="let c of filteredClaims">
                <td class="mono">{{ c.claimNumber }}</td>
                <td class="desc-cell">{{ c.description | slice:0:50 }}{{ c.description.length > 50 ? '...' : '' }}</td>
                <td><app-status-badge [value]="c.status"></app-status-badge></td>
                <td>{{ c.claimAmount | formatCurrency }}</td>
                <td>{{ c.createdDate | date:'mediumDate' }}</td>
              </tr>
              <tr *ngIf="!filteredClaims.length"><td colspan="5" class="empty-cell">No claims match the current filters</td></tr>
            </tbody>
          </table>
        </div>

        <!-- ── REVENUE TABLE ── -->
        <div class="table-wrap" *ngIf="reportType === 'revenue' && loaded">
          <table>
            <thead><tr>
              <th>Policy #</th><th>Type</th><th>Status</th>
              <th>Coverage</th><th>Monthly premium</th><th>Total collected</th>
            </tr></thead>
            <tbody>
              <tr *ngFor="let p of filteredPolicies">
                <td class="mono">{{ p.policyNumber }}</td>
                <td>{{ p.typeName }}</td>
                <td><app-status-badge [value]="p.status"></app-status-badge></td>
                <td>{{ p.coverageAmount | formatCurrency }}</td>
                <td>{{ p.monthlyPremium | formatCurrency }}</td>
                <td>{{ estimateRevenue(p) | formatCurrency }}</td>
              </tr>
              <tr *ngIf="!filteredPolicies.length"><td colspan="6" class="empty-cell">No revenue data</td></tr>
              <tr class="total-row" *ngIf="filteredPolicies.length">
                <td colspan="5"><strong>Total revenue</strong></td>
                <td><strong>{{ totalRevenue | formatCurrency }}</strong></td>
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
    .surface-card { border-radius: var(--radius-lg); padding: 1.5rem; border: 1px solid rgba(84, 101, 255, 0.1); background: rgba(255,255,255,0.88); backdrop-filter: blur(12px); box-shadow: 0 1px 3px rgba(0,0,0,0.04); }
    .section-head { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; margin-bottom: 1rem; }
    h2 { margin: 0; font-family: 'Space Grotesk', sans-serif; color: var(--ink); font-size: 1.4rem; }
    .eyebrow { text-transform: uppercase; letter-spacing: 0.14em; font-size: 0.68rem; font-weight: 700; color: var(--primary-2); margin-bottom: 0.2rem; }

    .mini-tabs { display: flex; flex-wrap: wrap; gap: 0.4rem; }
    .mini-tabs button { border: 0; border-radius: var(--radius-full); padding: 0.6rem 1rem; background: rgba(84,101,255,0.08); color: var(--primary); font-weight: 700; font-size: 0.84rem; cursor: pointer; transition: all var(--transition); }
    .mini-tabs button:hover { background: rgba(84,101,255,0.14); }
    .mini-tabs button.active { background: linear-gradient(135deg, #5465ff, #788bff); color: white; box-shadow: 0 4px 14px rgba(84,101,255,0.3); }

    .filters { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 0.75rem; margin-bottom: 1.25rem; }
    label { display: grid; gap: 0.3rem; }
    label span { font-weight: 600; color: var(--ink); font-size: 0.86rem; }
    input, select { border: 1.5px solid rgba(84,101,255,0.12); background: rgba(255,255,255,0.85); border-radius: var(--radius-sm); padding: 0.78rem 1rem; color: var(--ink); font-size: 0.9rem; transition: all var(--transition); }
    input:focus, select:focus { border-color: var(--primary-2); background: white; box-shadow: 0 0 0 4px rgba(84,101,255,0.08); outline: none; }

    .filters-actions { display: flex; gap: 0.6rem; align-items: end; }
    .primary-button, .secondary-button { border: 0; border-radius: var(--radius-sm); padding: 0.78rem 1.1rem; font-weight: 700; font-size: 0.86rem; cursor: pointer; display: flex; align-items: center; gap: 0.4rem; transition: all var(--transition); white-space: nowrap; }
    .primary-button { color: white; background: linear-gradient(135deg, #5465ff, #788bff); box-shadow: 0 4px 14px rgba(84,101,255,0.25); }
    .primary-button:hover { transform: translateY(-1px); box-shadow: 0 6px 20px rgba(84,101,255,0.35); }
    .secondary-button { background: rgba(84,101,255,0.07); color: var(--primary); }
    .secondary-button:hover:not(:disabled) { background: rgba(84,101,255,0.13); }
    .secondary-button:disabled { opacity: 0.4; cursor: not-allowed; }

    .summary-strip { display: grid; grid-template-columns: repeat(auto-fit, minmax(140px, 1fr)); gap: 0.65rem; margin-bottom: 1.25rem; }
    .summary-item { padding: 1rem; border-radius: var(--radius); background: rgba(84,101,255,0.04); border: 1px solid rgba(84,101,255,0.08); display: grid; gap: 0.25rem; }
    .summary-item span { font-size: 0.76rem; color: var(--muted); font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; }
    .summary-item strong { font-size: 1.3rem; font-weight: 700; color: var(--ink); font-family: 'Space Grotesk', sans-serif; }

    .table-wrap { overflow-x: auto; }
    table { width: 100%; border-collapse: collapse; font-size: 0.88rem; }
    th, td { padding: 0.75rem 0.85rem; border-bottom: 1px solid rgba(84,101,255,0.08); text-align: left; }
    th { color: var(--muted); text-transform: uppercase; letter-spacing: 0.08em; font-size: 0.72rem; font-weight: 700; background: rgba(84,101,255,0.03); position: sticky; top: 0; }
    tr:hover td { background: rgba(84,101,255,0.03); }
    .mono { font-family: 'SF Mono', 'Fira Code', monospace; font-size: 0.82rem; }
    .desc-cell { max-width: 220px; }
    .empty-cell { text-align: center; color: var(--muted); padding: 2rem; }
    .total-row td { border-top: 2px solid rgba(84,101,255,0.2); background: rgba(84,101,255,0.04); }

    @media (max-width: 920px) { .filters { grid-template-columns: 1fr 1fr; } }
    @media (max-width: 600px) { .filters { grid-template-columns: 1fr; } }
  `]
})
export class ReportsViewerComponent implements OnInit {
  reportType: 'policies' | 'claims' | 'revenue' = 'policies';
  filters = { from: '', to: '', type: '', status: '' };
  loaded = false;

  policies: PolicyDto[] = [];
  claims: ClaimDto[] = [];
  summaryCards: Array<{ label: string; value: string }> = [];

  constructor(
    private readonly adminService: AdminService,
    private readonly claimsService: ClaimsService,
    private readonly policyService: PolicyService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {}

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

  // ── Computed filtered data ──

  get filteredPolicies(): PolicyDto[] {
    let result = [...this.policies];
    if (this.filters.from) {
      result = result.filter(p => p.issuedDate >= this.filters.from);
    }
    if (this.filters.to) {
      result = result.filter(p => p.issuedDate <= this.filters.to + 'T23:59:59');
    }
    if (this.filters.type) {
      const q = this.filters.type.toLowerCase();
      result = result.filter(p => p.typeName.toLowerCase().includes(q) || p.subTypeName.toLowerCase().includes(q));
    }
    return result;
  }

  get filteredClaims(): ClaimDto[] {
    let result = [...this.claims];
    if (this.filters.from) {
      result = result.filter(c => c.createdDate >= this.filters.from);
    }
    if (this.filters.to) {
      result = result.filter(c => c.createdDate <= this.filters.to + 'T23:59:59');
    }
    if (this.filters.status) {
      result = result.filter(c => c.status === this.filters.status);
    }
    return result;
  }

  get totalRevenue(): number {
    return this.filteredPolicies.reduce((sum, p) => sum + this.estimateRevenue(p), 0);
  }

  estimateRevenue(policy: PolicyDto): number {
    const start = new Date(policy.issuedDate);
    const end = policy.endDate ? new Date(policy.endDate) : new Date();
    const months = Math.max(1, Math.round((end.getTime() - start.getTime()) / (30 * 24 * 60 * 60 * 1000)));
    return policy.monthlyPremium * months;
  }

  // ── Load data ──

  loadReport(): void {
    this.loaded = false;

    forkJoin({
      policies: this.policyService.getAllPoliciesForAdmin(),
      claims: this.claimsService.getAllClaimsForAdmin()
    }).subscribe({
      next: ({ policies, claims }) => {
        this.policies = policies;
        this.claims = claims;
        this.buildSummary();
        this.loaded = true;
      }
    });
  }

  private buildSummary(): void {
    if (this.reportType === 'policies') {
      const data = this.filteredPolicies;
      const active = data.filter(p => p.status === 'ACTIVE').length;
      this.summaryCards = [
        { label: 'Total policies', value: String(data.length) },
        { label: 'Active', value: String(active) },
        { label: 'Avg coverage', value: data.length ? `₹${Math.round(data.reduce((s, p) => s + p.coverageAmount, 0) / data.length).toLocaleString('en-IN')}` : '₹0' },
        { label: 'Avg premium', value: data.length ? `₹${Math.round(data.reduce((s, p) => s + p.monthlyPremium, 0) / data.length).toLocaleString('en-IN')}/mo` : '₹0/mo' }
      ];
    } else if (this.reportType === 'claims') {
      const data = this.filteredClaims;
      const approved = data.filter(c => c.status === 'APPROVED').length;
      const rejected = data.filter(c => c.status === 'REJECTED').length;
      this.summaryCards = [
        { label: 'Total claims', value: String(data.length) },
        { label: 'Approved', value: String(approved) },
        { label: 'Rejected', value: String(rejected) },
        { label: 'Total amount', value: `₹${data.reduce((s, c) => s + c.claimAmount, 0).toLocaleString('en-IN')}` }
      ];
    } else {
      const data = this.filteredPolicies;
      this.summaryCards = [
        { label: 'Policies', value: String(data.length) },
        { label: 'Total revenue', value: `₹${this.totalRevenue.toLocaleString('en-IN')}` },
        { label: 'Avg premium', value: data.length ? `₹${Math.round(data.reduce((s, p) => s + p.monthlyPremium, 0) / data.length).toLocaleString('en-IN')}/mo` : '₹0/mo' },
        { label: 'Highest coverage', value: data.length ? `₹${Math.max(...data.map(p => p.coverageAmount)).toLocaleString('en-IN')}` : '₹0' }
      ];
    }
  }

  // ── PDF download ──

  async downloadReport(): Promise<void> {
    if (!this.loaded) {
      return;
    }

    const doc = new jsPDF({ orientation: this.reportType === 'policies' ? 'landscape' : 'portrait' });
    const now = new Date();
    const dateStr = now.toISOString().slice(0, 10);
    const pageWidth = doc.internal.pageSize.getWidth();
    const reportTitle = `${this.reportType.charAt(0).toUpperCase() + this.reportType.slice(1)} Report`;

    // ── Header ──
    doc.setFillColor(84, 101, 255);
    doc.rect(0, 0, pageWidth, 35, 'F');

    doc.setTextColor(255, 255, 255);
    doc.setFontSize(20);
    doc.setFont('helvetica', 'bold');
    doc.text('SmartSure', 14, 16);

    doc.setFontSize(10);
    doc.setFont('helvetica', 'normal');
    doc.text(reportTitle, 14, 25);

    doc.setFontSize(8);
    doc.text(`Generated: ${now.toLocaleDateString()} at ${now.toLocaleTimeString()}`, pageWidth - 14, 25, { align: 'right' });

    // ── Filters line ──
    let yPos = 44;
    doc.setTextColor(120, 120, 120);
    doc.setFontSize(8);
    const filterParts: string[] = [];
    if (this.filters.from) { filterParts.push(`From: ${this.filters.from}`); }
    if (this.filters.to) { filterParts.push(`To: ${this.filters.to}`); }
    if (this.filters.type) { filterParts.push(`Type: ${this.filters.type}`); }
    if (this.filters.status) { filterParts.push(`Status: ${this.filters.status}`); }
    if (filterParts.length) {
      doc.text(`Filters: ${filterParts.join('  •  ')}`, 14, yPos);
      yPos += 8;
    }

    // ── Summary cards row ──
    doc.setFontSize(8);
    const cardText = this.summaryCards.map(c => `${c.label}: ${c.value}`).join('    |    ');
    doc.setTextColor(80, 80, 80);
    doc.text(cardText, 14, yPos);
    yPos += 10;

    // ── Data table ──
    if (this.reportType === 'policies') {
      const rows = this.filteredPolicies.map(p => [
        p.policyNumber, p.typeName, p.subTypeName, p.status,
        `₹${p.coverageAmount.toLocaleString('en-IN')}`,
        `₹${p.monthlyPremium.toLocaleString('en-IN')}`,
        new Date(p.issuedDate).toLocaleDateString()
      ]);
      autoTable(doc, {
        startY: yPos,
        head: [['Policy #', 'Type', 'Sub-type', 'Status', 'Coverage', 'Premium/mo', 'Issued']],
        body: rows.length ? rows : [['No policies found', '', '', '', '', '', '']],
        ...this.getTableStyles()
      });
    } else if (this.reportType === 'claims') {
      const rows = this.filteredClaims.map(c => [
        c.claimNumber, c.description.slice(0, 40) + (c.description.length > 40 ? '...' : ''),
        c.status, `₹${c.claimAmount.toLocaleString('en-IN')}`,
        new Date(c.createdDate).toLocaleDateString()
      ]);
      autoTable(doc, {
        startY: yPos,
        head: [['Claim #', 'Description', 'Status', 'Amount', 'Filed on']],
        body: rows.length ? rows : [['No claims found', '', '', '', '']],
        ...this.getTableStyles()
      });
    } else {
      const rows = this.filteredPolicies.map(p => [
        p.policyNumber, p.typeName, p.status,
        `₹${p.coverageAmount.toLocaleString('en-IN')}`,
        `₹${p.monthlyPremium.toLocaleString('en-IN')}`,
        `₹${this.estimateRevenue(p).toLocaleString('en-IN')}`
      ]);
      if (rows.length) {
        rows.push(['', '', '', '', 'Total revenue', `₹${this.totalRevenue.toLocaleString('en-IN')}`]);
      }
      autoTable(doc, {
        startY: yPos,
        head: [['Policy #', 'Type', 'Status', 'Coverage', 'Premium/mo', 'Revenue collected']],
        body: rows.length ? rows : [['No revenue data', '', '', '', '', '']],
        ...this.getTableStyles()
      });
    }

    // ── Footer ──
    const pageCount = doc.getNumberOfPages();
    for (let i = 1; i <= pageCount; i++) {
      doc.setPage(i);
      doc.setFontSize(7);
      doc.setTextColor(160, 160, 160);
      const pageH = doc.internal.pageSize.getHeight();
      doc.text(
        `SmartSure Insurance  •  ${reportTitle}  •  Page ${i} of ${pageCount}`,
        pageWidth / 2, pageH - 8,
        { align: 'center' }
      );
    }

    // ── Save with native dialog ──
    const defaultName = `SmartSure_${this.reportType}_report_${dateStr}.pdf`;
    const pdfBlob = doc.output('blob');

    try {
      if ('showSaveFilePicker' in window) {
        const handle = await (window as any).showSaveFilePicker({
          suggestedName: defaultName,
          types: [{ description: 'PDF Document', accept: { 'application/pdf': ['.pdf'] } }]
        });
        const writable = await handle.createWritable();
        await writable.write(pdfBlob);
        await writable.close();

        const savedFile = await handle.getFile();
        const viewUrl = URL.createObjectURL(savedFile);
        window.open(viewUrl, '_blank');
        setTimeout(() => URL.revokeObjectURL(viewUrl), 5000);
        return;
      }
    } catch (err: any) {
      if (err?.name === 'AbortError') { return; }
    }

    // Fallback
    const url = URL.createObjectURL(pdfBlob);
    window.open(url, '_blank');
    setTimeout(() => URL.revokeObjectURL(url), 5000);
  }

  private getTableStyles() {
    return {
      theme: 'grid' as const,
      headStyles: {
        fillColor: [84, 101, 255] as [number, number, number],
        textColor: [255, 255, 255] as [number, number, number],
        fontStyle: 'bold' as const,
        fontSize: 9
      },
      bodyStyles: {
        fontSize: 8.5,
        textColor: [50, 50, 50] as [number, number, number]
      },
      alternateRowStyles: {
        fillColor: [245, 247, 255] as [number, number, number]
      },
      styles: {
        cellPadding: 5,
        lineColor: [220, 225, 240] as [number, number, number],
        lineWidth: 0.2
      },
      margin: { left: 14, right: 14 }
    };
  }
}
