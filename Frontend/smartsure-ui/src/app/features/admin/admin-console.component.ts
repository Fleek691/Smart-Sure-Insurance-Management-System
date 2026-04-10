import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import {
  AuditLogDto,
  ClaimDto,
  DashboardStatsDto,
  PolicyDto,
  ProfileDto
} from '../../models/api-models';
import { AdminService } from '../../core/services/admin.service';
import { AuthService } from '../../core/services/auth.service';
import { ClaimsService } from '../../core/services/claims.service';
import { PolicyService } from '../../core/services/policy.service';
import { SharedModule } from '../../shared/shared.module';

@Component({
  selector: 'app-admin-console',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, SharedModule],
  template: `
    <section class="console-grid">
      <aside class="pane">
        <p class="eyebrow">Admin console</p>
        <h2>Operations, approvals, and audit visibility</h2>
        <p class="muted">The admin surface combines dashboard metrics, claim review, policy status management, user controls, and audit logs.</p>

        <div class="mini-tabs">
          <button type="button" [class.active]="view === 'dashboard'" (click)="go('/admin/dashboard')">Dashboard</button>
          <button type="button" [class.active]="view === 'claims'" (click)="go('/admin/claims')">Claims</button>
          <button type="button" [class.active]="view === 'policies'" (click)="go('/admin/policies')">Policies</button>
          <button type="button" [class.active]="view === 'users'" (click)="go('/admin/users')">Users</button>
          <button type="button" [class.active]="view === 'audit'" (click)="go('/admin/audit-logs')">Audit logs</button>
          <button type="button" [class.active]="view === 'reports'" (click)="go('/admin/reports')">Reports</button>
        </div>

        <div class="summary-grid">
          <div class="summary-card">
            <span>Policies</span>
            <strong>{{ stats?.totalPolicies || 0 }}</strong>
          </div>
          <div class="summary-card">
            <span>Claims</span>
            <strong>{{ stats?.totalClaims || 0 }}</strong>
          </div>
          <div class="summary-card">
            <span>Revenue</span>
            <strong>{{ stats?.totalRevenue | formatCurrency }}</strong>
          </div>
          <div class="summary-card">
            <span>Users</span>
            <strong>{{ users.length }}</strong>
          </div>
        </div>
      </aside>

      <div class="pane wide" *ngIf="view === 'dashboard'">
        <div class="surface-card">
          <div class="section-head">
            <div>
              <p class="eyebrow">Dashboard</p>
              <h3>Current operations snapshot</h3>
            </div>
            <app-status-badge value="ADMIN"></app-status-badge>
          </div>

          <div class="detail-grid">
            <div><span>Total policies</span><strong>{{ stats?.totalPolicies || 0 }}</strong></div>
            <div><span>Total claims</span><strong>{{ stats?.totalClaims || 0 }}</strong></div>
            <div><span>Total revenue</span><strong>{{ stats?.totalRevenue | formatCurrency }}</strong></div>
            <div><span>Live signals</span><strong>Gateway + RabbitMQ</strong></div>
          </div>
        </div>
      </div>

      <div class="pane wide" *ngIf="view === 'claims'">
        <div class="surface-card">
          <div class="section-head">
            <div>
              <p class="eyebrow">Claims review</p>
              <h3>Admin claim queue</h3>
            </div>
            <app-stepper [steps]="['Review', 'Approve', 'Reject']" [activeIndex]="1"></app-stepper>
          </div>

          <div class="claim-grid">
            <article class="claim-card" *ngFor="let claim of claims" (click)="selectedClaim = claim">
              <div class="policy-top">
                <div>
                  <strong>{{ claim.claimNumber }}</strong>
                  <p>{{ claim.description }}</p>
                </div>
                <app-status-badge [value]="claim.status"></app-status-badge>
              </div>
              <div class="policy-meta">
                <span>{{ claim.claimAmount | formatCurrency }}</span>
                <span>{{ claim.createdDate | date:'mediumDate' }}</span>
              </div>
            </article>
          </div>

          <div class="action-card" *ngIf="selectedClaim">
            <div class="section-head">
              <div>
                <p class="eyebrow">Selected claim</p>
                <h3>{{ selectedClaim.claimNumber }}</h3>
              </div>
              <app-status-badge [value]="selectedClaim.status"></app-status-badge>
            </div>

            <label>
              <span>Review note</span>
              <textarea name="reviewNote" [(ngModel)]="reviewNote" rows="3"></textarea>
            </label>
            <label>
              <span>Approved amount</span>
              <input name="approvedAmount" [(ngModel)]="approvedAmount" type="number" min="1" />
            </label>
            <label>
              <span>Rejection reason</span>
              <input name="rejectReason" [(ngModel)]="rejectReason" />
            </label>

            <div class="button-row">
              <button type="button" class="secondary-button" (click)="markUnderReview()">Mark under review</button>
              <button type="button" class="primary-button" (click)="approveClaim()">Approve</button>
              <button type="button" class="danger-button" (click)="rejectClaim()">Reject</button>
            </div>
          </div>
        </div>
      </div>

      <div class="pane wide" *ngIf="view === 'policies'">
        <div class="surface-card">
          <div class="section-head">
            <div>
              <p class="eyebrow">Policy management</p>
              <h3>Service-owned products and policies</h3>
            </div>
          </div>

          <div class="policy-grid">
            <article class="policy-card" *ngFor="let policy of policies" (click)="selectedPolicy = policy">
              <div class="policy-top">
                <div>
                  <strong>{{ policy.policyNumber }}</strong>
                  <p>{{ policy.typeName }} / {{ policy.subTypeName }}</p>
                </div>
                <app-status-badge [value]="policy.status"></app-status-badge>
              </div>
              <div class="policy-meta">
                <span>{{ policy.coverageAmount | formatCurrency }}</span>
                <span>{{ policy.monthlyPremium | formatCurrency }}/mo</span>
              </div>
            </article>
          </div>

          <div class="action-card" *ngIf="selectedPolicy">
            <div class="section-head">
              <div>
                <p class="eyebrow">Status update</p>
                <h3>{{ selectedPolicy.policyNumber }}</h3>
              </div>
              <app-status-badge [value]="selectedPolicy.status"></app-status-badge>
            </div>
            <label>
              <span>New status</span>
              <select name="policyStatus" [(ngModel)]="policyStatus">
                <option value="ACTIVE">ACTIVE</option>
                <option value="DRAFT">DRAFT</option>
                <option value="EXPIRED">EXPIRED</option>
                <option value="CANCELLED">CANCELLED</option>
              </select>
            </label>
            <button type="button" class="primary-button" (click)="updatePolicyStatus()">Update status</button>
          </div>
        </div>
      </div>

      <div class="pane wide" *ngIf="view === 'users'">
        <div class="surface-card">
          <div class="section-head">
            <div>
              <p class="eyebrow">User management</p>
              <h3>Identity users</h3>
            </div>
          </div>

          <div class="user-grid">
            <article class="user-card" *ngFor="let user of users">
              <div>
                <strong>{{ user.fullName }}</strong>
                <p>{{ user.email }}</p>
              </div>
              <div class="button-row">
                <app-status-badge [value]="user.isActive === false ? 'REJECTED' : 'ACTIVE'"></app-status-badge>
                <button type="button" class="secondary-button" (click)="toggleUser(user)">{{ user.isActive === false ? 'Activate' : 'Deactivate' }}</button>
              </div>
            </article>
          </div>
        </div>
      </div>

      <div class="pane wide" *ngIf="view === 'audit'">
        <div class="surface-card">
          <div class="section-head">
            <div>
              <p class="eyebrow">Audit trail</p>
              <h3>Recent events</h3>
            </div>
          </div>

          <div class="audit-list">
            <article class="audit-card" *ngFor="let log of auditLogs">
              <div>
                <strong>{{ log.action }}</strong>
                <p>{{ log.entityType }} · {{ log.entityId }}</p>
              </div>
              <span>{{ log.timeStamp | date:'medium' }}</span>
            </article>
          </div>
        </div>
      </div>

      <div class="pane wide" *ngIf="view === 'reports'">
        <div class="surface-card">
          <div class="section-head">
            <div>
              <p class="eyebrow">Reports</p>
              <h3>Open the report viewer</h3>
            </div>
          </div>
          <p class="muted">Use the dedicated reports route for policy, claims, and revenue analysis.</p>
          <div class="button-row">
            <button type="button" class="primary-button" (click)="go('/admin/reports')">Go to reports</button>
          </div>
        </div>
      </div>
    </section>
  `,
  styles: [`
    :host { display: block; animation: fadeInUp 0.4s ease; }
    .console-grid { display: grid; grid-template-columns: 340px minmax(0, 1fr); gap: 1.25rem; }
    .pane { display: grid; gap: 1rem; align-content: start; }
    .surface-card {
      border-radius: var(--radius-lg); padding: 1.5rem;
      border: 1px solid rgba(84, 101, 255, 0.1); background: rgba(255, 255, 255, 0.88);
      backdrop-filter: blur(12px); box-shadow: var(--shadow);
      transition: box-shadow var(--transition);
    }
    .surface-card:hover { box-shadow: var(--shadow-lg); }

    h2, h3 { margin: 0; font-family: 'Space Grotesk', sans-serif; color: var(--ink); letter-spacing: -0.02em; }
    h2 { font-size: 1.75rem; font-weight: 700; line-height: 1.3; }
    h3 { font-size: 1.15rem; font-weight: 600; }
    .muted { color: var(--muted); line-height: 1.65; font-size: 0.9rem; }
    .eyebrow { text-transform: uppercase; letter-spacing: 0.14em; font-size: 0.7rem; font-weight: 700; color: var(--primary-2); margin-bottom: 0.3rem; }

    .mini-tabs { display: flex; flex-wrap: wrap; gap: 0.4rem; background: rgba(84, 101, 255, 0.06); padding: 4px; border-radius: var(--radius-full); }
    .mini-tabs button { border: 0; border-radius: var(--radius-full); padding: 0.6rem 1rem; background: transparent; color: var(--muted); font-weight: 600; font-size: 0.85rem; cursor: pointer; transition: all var(--transition); }
    .mini-tabs button:hover { color: var(--primary); background: rgba(255,255,255,0.6); }
    .mini-tabs button.active { background: linear-gradient(135deg, #5465ff 0%, #788bff 100%); color: white; box-shadow: 0 4px 14px rgba(84, 101, 255, 0.35); }

    .summary-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 0.75rem; }
    .summary-card {
      border-radius: var(--radius); padding: 1.1rem;
      background: linear-gradient(135deg, rgba(84, 101, 255, 0.06) 0%, rgba(191, 215, 255, 0.15) 100%);
      border: 1px solid rgba(84, 101, 255, 0.1); display: grid; gap: 0.4rem;
      transition: transform var(--transition);
    }
    .summary-card:hover { transform: translateY(-2px); }
    .summary-card span { color: var(--muted); font-size: 0.8rem; font-weight: 600; text-transform: uppercase; letter-spacing: 0.06em; }
    .summary-card strong { color: var(--ink); font-size: 1.5rem; font-family: 'Space Grotesk', sans-serif; font-weight: 700; }

    .detail-grid span { color: var(--muted); font-size: 0.85rem; }
    .detail-grid strong { color: var(--ink); font-size: 1rem; }

    .section-head { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; margin-bottom: 1rem; }

    .claim-grid, .policy-grid, .user-grid, .audit-list { display: grid; gap: 0.75rem; }
    .claim-card, .policy-card, .user-card, .audit-card {
      border: 1px solid rgba(84, 101, 255, 0.1); border-radius: var(--radius);
      background: rgba(255, 255, 255, 0.8); padding: 1.1rem;
      transition: all var(--transition); position: relative; overflow: hidden;
    }
    .claim-card::before, .policy-card::before {
      content: ''; position: absolute; left: 0; top: 0; bottom: 0; width: 3px;
      background: linear-gradient(180deg, #5465ff, #9bb1ff); border-radius: 0 3px 3px 0;
      opacity: 0; transition: opacity var(--transition);
    }
    .claim-card, .policy-card { cursor: pointer; }
    .claim-card:hover, .policy-card:hover { border-color: rgba(84, 101, 255, 0.2); box-shadow: var(--shadow-sm); transform: translateY(-2px); }
    .claim-card:hover::before, .policy-card:hover::before { opacity: 1; }
    .user-card:hover, .audit-card:hover { border-color: rgba(84, 101, 255, 0.2); box-shadow: var(--shadow-sm); }
    .claim-card p, .policy-card p, .user-card p, .audit-card p { color: var(--muted); margin-top: 0.2rem; font-size: 0.85rem; }
    .audit-card span { color: var(--muted); font-size: 0.82rem; }

    .policy-top { display: flex; justify-content: space-between; gap: 1rem; margin-bottom: 0.6rem; }
    .policy-meta { display: flex; flex-wrap: wrap; gap: 0.5rem; }
    .policy-meta span { background: rgba(84, 101, 255, 0.07); padding: 0.3rem 0.6rem; border-radius: var(--radius-full); font-size: 0.8rem; font-weight: 500; color: var(--text); }

    .action-card {
      margin-top: 1rem; border-radius: var(--radius); padding: 1.25rem;
      background: linear-gradient(135deg, rgba(84, 101, 255, 0.06) 0%, rgba(226, 253, 255, 0.4) 100%);
      border: 1px solid rgba(84, 101, 255, 0.12); display: grid; gap: 0.85rem;
    }

    .detail-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 0.75rem; margin: 1rem 0; }
    .detail-grid div {
      border-radius: var(--radius-sm); background: rgba(255, 255, 255, 0.75); padding: 0.9rem;
      display: grid; gap: 0.3rem; border: 1px solid rgba(84, 101, 255, 0.08);
      transition: transform var(--transition);
    }
    .detail-grid div:hover { transform: translateY(-1px); }

    .button-row { display: flex; flex-wrap: wrap; gap: 0.6rem; align-items: center; }
    label { display: grid; gap: 0.35rem; }
    label span { font-weight: 600; color: var(--ink); font-size: 0.88rem; }
    textarea, input, select { border: 1.5px solid rgba(84, 101, 255, 0.14); background: rgba(255, 255, 255, 0.8); border-radius: var(--radius-sm); padding: 0.82rem 1rem; color: var(--ink); font-size: 0.92rem; }
    textarea:focus, input:focus, select:focus { border-color: var(--primary-2); background: white; box-shadow: 0 0 0 4px rgba(84, 101, 255, 0.1); outline: none; }
    textarea { resize: vertical; }

    .primary-button, .secondary-button, .danger-button {
      border: 0; border-radius: var(--radius-sm); padding: 0.82rem 1.25rem;
      font-weight: 700; font-size: 0.9rem; cursor: pointer; transition: all var(--transition);
    }
    .primary-button { color: white; background: linear-gradient(135deg, #5465ff 0%, #788bff 100%); box-shadow: 0 4px 14px rgba(84, 101, 255, 0.3); }
    .primary-button:hover:not(:disabled) { transform: translateY(-1px); box-shadow: 0 6px 20px rgba(84, 101, 255, 0.4); }
    .secondary-button { background: rgba(84, 101, 255, 0.08); color: var(--primary); }
    .secondary-button:hover:not(:disabled) { background: rgba(84, 101, 255, 0.14); }
    .danger-button { background: rgba(239, 68, 68, 0.08); color: #ef4444; border: 1px solid rgba(239, 68, 68, 0.15); }
    .danger-button:hover { background: rgba(239, 68, 68, 0.14); border-color: rgba(239, 68, 68, 0.3); }
    .ghost-link { background: transparent; border: 0; color: var(--primary); cursor: pointer; font-weight: 700; font-size: 0.85rem; padding: 0.2rem 0; text-decoration: none; }
    .muted { color: var(--muted); }
    @media (max-width: 1200px) { .console-grid { grid-template-columns: 1fr; } }
    @media (max-width: 720px) { .detail-grid, .summary-grid { grid-template-columns: 1fr; } }
  `]
})
export class AdminConsoleComponent implements OnInit {
  view: 'dashboard' | 'claims' | 'policies' | 'users' | 'audit' | 'reports' = 'dashboard';
  stats: DashboardStatsDto | null = null;
  claims: ClaimDto[] = [];
  policies: PolicyDto[] = [];
  users: ProfileDto[] = [];
  auditLogs: AuditLogDto[] = [];
  selectedClaim: ClaimDto | null = null;
  selectedPolicy: PolicyDto | null = null;
  reviewNote = '';
  approvedAmount = 0;
  rejectReason = '';
  policyStatus = 'ACTIVE';

  constructor(
    private readonly adminService: AdminService,
    private readonly authService: AuthService,
    private readonly claimsService: ClaimsService,
    private readonly policyService: PolicyService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.syncRoute();
    this.loadData();
    this.route.url.subscribe(() => this.syncRoute());
  }

  go(path: string): void {
    void this.router.navigateByUrl(path);
  }

  private loadData(): void {
    forkJoin({
      stats: this.adminService.getDashboardStats(),
      claims: this.claimsService.getAllClaimsForAdmin(),
      policies: this.policyService.getAllPoliciesForAdmin(),
      users: this.authService.getUsers(),
      auditLogs: this.adminService.getAuditLogs(undefined, undefined, undefined, undefined, 1, 10)
    }).subscribe({
      next: ({ stats, claims, policies, users, auditLogs }) => {
        this.stats = stats;
        this.claims = claims;
        this.policies = policies;
        this.users = users;
        this.auditLogs = auditLogs.items;
      }
    });
  }

  markUnderReview(): void {
    if (!this.selectedClaim) {
      return;
    }

    this.claimsService.markUnderReview(this.selectedClaim.claimId, { note: this.reviewNote }).subscribe({
      next: (claim) => {
        this.selectedClaim = claim;
        this.loadData();
      }
    });
  }

  approveClaim(): void {
    if (!this.selectedClaim) {
      return;
    }

    this.claimsService.approveClaim(this.selectedClaim.claimId, { approvedAmount: this.approvedAmount || undefined, note: this.reviewNote }).subscribe({
      next: (claim) => {
        this.selectedClaim = claim;
        this.loadData();
      }
    });
  }

  rejectClaim(): void {
    if (!this.selectedClaim) {
      return;
    }

    this.claimsService.rejectClaim(this.selectedClaim.claimId, { reason: this.rejectReason || this.reviewNote || 'Rejected by admin' }).subscribe({
      next: (claim) => {
        this.selectedClaim = claim;
        this.loadData();
      }
    });
  }

  updatePolicyStatus(): void {
    if (!this.selectedPolicy) {
      return;
    }

    this.policyService.updatePolicyStatus(this.selectedPolicy.policyId, this.policyStatus).subscribe({
      next: (policy) => {
        this.selectedPolicy = policy;
        this.loadData();
      }
    });
  }

  toggleUser(user: ProfileDto): void {
    this.authService.updateUserStatus(user.userId, user.isActive === false).subscribe({
      next: () => this.loadData()
    });
  }

  private syncRoute(): void {
    const url = this.router.url;
    if (url.includes('/claims')) {
      this.view = 'claims';
      return;
    }

    if (url.includes('/policies')) {
      this.view = 'policies';
      return;
    }

    if (url.includes('/users')) {
      this.view = 'users';
      return;
    }

    if (url.includes('/audit-logs')) {
      this.view = 'audit';
      return;
    }

    if (url.includes('/reports')) {
      this.view = 'reports';
      return;
    }

    this.view = 'dashboard';
  }
}
