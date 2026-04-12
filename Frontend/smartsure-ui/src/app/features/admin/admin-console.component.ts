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
    <section class="console-shell">
      <!-- ── TAB BAR ── -->
      <div class="tab-bar">
        <button type="button" [class.active]="view === 'dashboard'" (click)="go('/admin/dashboard')">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/></svg>
          Dashboard
        </button>
        <button type="button" [class.active]="view === 'claims'" (click)="go('/admin/claims')">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/></svg>
          Claims
        </button>
        <button type="button" [class.active]="view === 'policies'" (click)="go('/admin/policies')">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
          Policies
        </button>
        <button type="button" [class.active]="view === 'users'" (click)="go('/admin/users')">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 00-3-3.87"/><path d="M16 3.13a4 4 0 010 7.75"/></svg>
          Users
        </button>
        <button type="button" [class.active]="view === 'audit'" (click)="go('/admin/audit-logs')">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
          Audit logs
        </button>
        <button type="button" [class.active]="view === 'reports'" (click)="go('/admin/reports')">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="20" x2="18" y2="10"/><line x1="12" y1="20" x2="12" y2="4"/><line x1="6" y1="20" x2="6" y2="14"/></svg>
          Reports
        </button>
      </div>

      <!-- ══════════════ DASHBOARD TAB ══════════════ -->
      <div class="tab-content" *ngIf="view === 'dashboard'">
        <div class="kpi-strip">
          <div class="kpi-card kpi-policies">
            <div class="kpi-icon">
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
            </div>
            <div>
              <span class="kpi-label">Total Policies</span>
              <strong class="kpi-value">{{ stats?.totalPolicies || 0 }}</strong>
            </div>
          </div>
          <div class="kpi-card kpi-claims">
            <div class="kpi-icon">
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
            </div>
            <div>
              <span class="kpi-label">Total Claims</span>
              <strong class="kpi-value">{{ stats?.totalClaims || 0 }}</strong>
            </div>
          </div>
          <div class="kpi-card kpi-revenue">
            <div class="kpi-icon">
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 000 7h5a3.5 3.5 0 010 7H6"/></svg>
            </div>
            <div>
              <span class="kpi-label">Revenue</span>
              <strong class="kpi-value">{{ stats?.totalRevenue | formatCurrency }}</strong>
            </div>
          </div>
          <div class="kpi-card kpi-users">
            <div class="kpi-icon">
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2"/><circle cx="9" cy="7" r="4"/></svg>
            </div>
            <div>
              <span class="kpi-label">Users</span>
              <strong class="kpi-value">{{ users.length }}</strong>
            </div>
          </div>
        </div>

        <!-- Recent activity -->
        <div class="dashboard-grid">
          <div class="surface-card">
            <div class="section-head">
              <div>
                <p class="eyebrow">Recent claims</p>
                <h3>Pending review</h3>
              </div>
              <button type="button" class="ghost-link" (click)="go('/admin/claims')">View all &rarr;</button>
            </div>
            <div class="compact-list" *ngIf="pendingClaims.length; else noPending">
              <article class="compact-item" *ngFor="let claim of pendingClaims">
                <div>
                  <strong>{{ claim.claimNumber }}</strong>
                  <p>{{ claim.description | slice:0:60 }}{{ claim.description.length > 60 ? '...' : '' }}</p>
                </div>
                <div class="compact-right">
                  <app-status-badge [value]="claim.status"></app-status-badge>
                  <span class="meta-text">{{ claim.claimAmount | formatCurrency }}</span>
                </div>
              </article>
            </div>
            <ng-template #noPending><p class="empty-state">No pending claims</p></ng-template>
          </div>

          <div class="surface-card">
            <div class="section-head">
              <div>
                <p class="eyebrow">Audit trail</p>
                <h3>Latest events</h3>
              </div>
              <button type="button" class="ghost-link" (click)="go('/admin/audit-logs')">View all &rarr;</button>
            </div>
            <div class="compact-list" *ngIf="auditLogs.length; else noLogs">
              <article class="compact-item" *ngFor="let log of auditLogs | slice:0:5">
                <div>
                  <strong>{{ log.action }}</strong>
                  <p>{{ log.entityType }} &middot; {{ log.entityId | slice:0:12 }}...</p>
                </div>
                <span class="meta-text">{{ log.timeStamp | date:'short' }}</span>
              </article>
            </div>
            <ng-template #noLogs><p class="empty-state">No audit events yet</p></ng-template>
          </div>
        </div>
      </div>

      <!-- ══════════════ CLAIMS TAB ══════════════ -->
      <div class="tab-content" *ngIf="view === 'claims'">
        <div class="surface-card">
          <div class="section-head">
            <div>
              <p class="eyebrow">Claims review</p>
              <h3>Admin claim queue</h3>
            </div>
            <app-stepper [steps]="['Review', 'Approve', 'Reject']" [activeIndex]="1"></app-stepper>
          </div>

          <div class="claim-grid">
            <article class="claim-card" *ngFor="let claim of claims" (click)="selectedClaim = claim" [class.selected]="selectedClaim?.claimId === claim.claimId">
              <div class="policy-top">
                <div>
                  <strong>{{ claim.claimNumber }}</strong>
                  <p>{{ claim.description | slice:0:80 }}{{ claim.description.length > 80 ? '...' : '' }}</p>
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

            <!-- Terminal state — hide all action controls -->
            <div class="terminal-state" *ngIf="selectedClaim.status === 'APPROVED' || selectedClaim.status === 'REJECTED' || selectedClaim.status === 'CLOSED'">
              <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.5"><circle cx="10" cy="10" r="8"/><path d="M10 6v4"/><circle cx="10" cy="14" r="0.5" fill="currentColor"/></svg>
              This claim is <strong>{{ selectedClaim.status | lowercase }}</strong> and cannot be modified further.
              <span *ngIf="selectedClaim.adminNote">&nbsp;Note: "{{ selectedClaim.adminNote }}"</span>
            </div>

            <!-- Active review controls -->
            <ng-container *ngIf="selectedClaim.status !== 'APPROVED' && selectedClaim.status !== 'REJECTED' && selectedClaim.status !== 'CLOSED'">
              <div class="form-row">
                <label>
                  <span>Review note</span>
                  <textarea name="reviewNote" [(ngModel)]="reviewNote" rows="3" placeholder="Add review notes..."></textarea>
                </label>
              </div>
              <div class="form-row two-col">
                <label>
                  <span>Approved amount</span>
                  <input name="approvedAmount" [(ngModel)]="approvedAmount" type="number" min="1" placeholder="0.00" />
                </label>
                <label>
                  <span>Rejection reason</span>
                  <input name="rejectReason" [(ngModel)]="rejectReason" placeholder="Reason for rejection..." />
                </label>
              </div>
              <div class="button-row">
                <button type="button" class="secondary-button"
                  (click)="markUnderReview()"
                  [disabled]="actionLoading || selectedClaim.status === 'UNDER_REVIEW' || selectedClaim.status === 'DRAFT'">
                  Mark under review
                </button>
                <button type="button" class="primary-button"
                  (click)="approveClaim()"
                  [disabled]="actionLoading || selectedClaim.status !== 'UNDER_REVIEW'">
                  Approve
                </button>
                <button type="button" class="danger-button"
                  (click)="rejectClaim()"
                  [disabled]="actionLoading || selectedClaim.status !== 'UNDER_REVIEW'">
                  Reject
                </button>
              </div>
              <p class="action-error" *ngIf="actionError">{{ actionError }}</p>
              <p class="action-hint" *ngIf="selectedClaim.status === 'SUBMITTED'">
                ⚠ First click "Mark under review", then approve or reject.
              </p>
            </ng-container>
          </div>
        </div>
      </div>

      <!-- ══════════════ POLICIES TAB ══════════════ -->
      <div class="tab-content" *ngIf="view === 'policies'">
        <div class="surface-card">
          <div class="section-head">
            <div>
              <p class="eyebrow">Policy management</p>
              <h3>All policies</h3>
            </div>
          </div>

          <div class="policy-grid">
            <article class="policy-card" *ngFor="let policy of policies" (click)="selectedPolicy = policy" [class.selected]="selectedPolicy?.policyId === policy.policyId">
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
            <div class="form-row two-col">
              <label>
                <span>New status</span>
                <select name="policyStatus" [(ngModel)]="policyStatus">
                  <option value="ACTIVE">ACTIVE</option>
                  <option value="DRAFT">DRAFT</option>
                  <option value="EXPIRED">EXPIRED</option>
                  <option value="CANCELLED">CANCELLED</option>
                </select>
              </label>
              <div class="form-action">
                <button type="button" class="primary-button" (click)="updatePolicyStatus()">Update status</button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- ══════════════ USERS TAB ══════════════ -->
      <div class="tab-content" *ngIf="view === 'users'">
        <div class="surface-card">
          <div class="section-head">
            <div>
              <p class="eyebrow">People</p>
              <h3>All users ({{ users.length }})</h3>
            </div>
          </div>

          <div class="user-grid">
            <article class="user-card" *ngFor="let user of users">
              <div class="user-info">
                <div class="avatar">{{ user.fullName.charAt(0).toUpperCase() }}</div>
                <div>
                  <strong>{{ user.fullName }}</strong>
                  <p>{{ user.email }}</p>
                  <p class="user-role-tag" *ngIf="user.roles?.length">{{ user.roles!.join(', ') }}</p>
                </div>
              </div>
              <div class="user-actions">
                <div class="role-select">
                  <label><span>Role</span>
                    <select [ngModel]="user.roles?.[0] || 'CUSTOMER'" (ngModelChange)="changeRole(user, $event)">
                      <option value="CUSTOMER">Customer</option>
                      <option value="ADMIN">Admin</option>
                    </select>
                  </label>
                </div>
                <div class="button-row">
                  <app-status-badge [value]="user.isActive === false ? 'REJECTED' : 'ACTIVE'"></app-status-badge>
                  <button type="button" class="secondary-button small" (click)="toggleUser(user)">{{ user.isActive === false ? 'Activate' : 'Deactivate' }}</button>
                </div>
              </div>
            </article>
          </div>
        </div>
      </div>

      <!-- ══════════════ AUDIT LOGS TAB ══════════════ -->
      <div class="tab-content" *ngIf="view === 'audit'">
        <div class="surface-card">
          <div class="section-head">
            <div>
              <p class="eyebrow">Audit trail</p>
              <h3>Event history</h3>
            </div>
          </div>

          <div class="audit-list" *ngIf="auditLogs.length; else noAudit">
            <article class="audit-card" *ngFor="let log of auditLogs">
              <div class="audit-icon">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
              </div>
              <div class="audit-body">
                <strong>{{ log.action }}</strong>
                <p>{{ log.entityType }} &middot; {{ log.entityId }}</p>
              </div>
              <span class="meta-text">{{ log.timeStamp | date:'medium' }}</span>
            </article>
          </div>
          <ng-template #noAudit><p class="empty-state">No audit events recorded yet</p></ng-template>
        </div>
      </div>

      <!-- ══════════════ REPORTS TAB ══════════════ -->
      <div class="tab-content" *ngIf="view === 'reports'">
        <div class="surface-card">
          <div class="section-head">
            <div>
              <p class="eyebrow">Reports</p>
              <h3>Generate &amp; export reports</h3>
            </div>
          </div>
          <p class="muted">Use the dedicated reports route for policy, claims, and revenue analysis.</p>
          <div class="button-row">
            <button type="button" class="primary-button" (click)="go('/admin/reports')">Open report viewer</button>
          </div>
        </div>
      </div>
    </section>
  `,
  styles: [`
    :host { display: block; animation: fadeInUp 0.4s ease; }

    /* ── Tab bar ── */
    .tab-bar {
      display: flex; flex-wrap: wrap; gap: 0.35rem;
      background: rgba(255, 255, 255, 0.7); backdrop-filter: blur(12px);
      padding: 5px; border-radius: var(--radius-lg);
      border: 1px solid rgba(84, 101, 255, 0.08);
      box-shadow: 0 1px 3px rgba(0,0,0,0.04);
      margin-bottom: 1.25rem;
    }
    .tab-bar button {
      display: flex; align-items: center; gap: 0.4rem;
      border: 0; border-radius: var(--radius); padding: 0.65rem 1.1rem;
      background: transparent; color: var(--muted); font-weight: 600;
      font-size: 0.84rem; cursor: pointer; transition: all var(--transition);
      white-space: nowrap;
    }
    .tab-bar button:hover { color: var(--primary); background: rgba(84,101,255,0.05); }
    .tab-bar button.active {
      background: linear-gradient(135deg, #5465ff 0%, #788bff 100%);
      color: white; box-shadow: 0 4px 14px rgba(84, 101, 255, 0.35);
    }
    .tab-bar button.active svg { stroke: white; }

    /* ── KPI strip ── */
    .kpi-strip { display: grid; grid-template-columns: repeat(4, 1fr); gap: 0.85rem; margin-bottom: 1.25rem; }
    .kpi-card {
      display: flex; align-items: center; gap: 1rem;
      padding: 1.2rem 1.4rem; border-radius: var(--radius-lg);
      border: 1px solid rgba(84, 101, 255, 0.08);
      background: rgba(255, 255, 255, 0.85); backdrop-filter: blur(10px);
      box-shadow: 0 1px 3px rgba(0,0,0,0.04);
      transition: all var(--transition);
    }
    .kpi-card:hover { transform: translateY(-3px); box-shadow: var(--shadow); }
    .kpi-icon {
      width: 44px; height: 44px; border-radius: var(--radius);
      display: flex; align-items: center; justify-content: center;
      flex-shrink: 0;
    }
    .kpi-policies .kpi-icon { background: rgba(84, 101, 255, 0.1); color: #5465ff; }
    .kpi-claims .kpi-icon { background: rgba(255, 159, 67, 0.12); color: #ff9f43; }
    .kpi-revenue .kpi-icon { background: rgba(40, 199, 111, 0.12); color: #28c76f; }
    .kpi-users .kpi-icon { background: rgba(115, 103, 240, 0.12); color: #7367f0; }
    .kpi-icon svg { stroke: currentColor; }
    .kpi-label { font-size: 0.78rem; color: var(--muted); font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; }
    .kpi-value { display: block; font-size: 1.6rem; font-weight: 700; font-family: 'Space Grotesk', sans-serif; color: var(--ink); }

    /* ── Dashboard grid ── */
    .dashboard-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 1rem; }

    /* ── Surface card ── */
    .surface-card {
      border-radius: var(--radius-lg); padding: 1.5rem;
      border: 1px solid rgba(84, 101, 255, 0.08); background: rgba(255, 255, 255, 0.88);
      backdrop-filter: blur(12px); box-shadow: 0 1px 3px rgba(0,0,0,0.04);
      transition: box-shadow var(--transition);
    }
    .surface-card:hover { box-shadow: var(--shadow); }

    /* ── Typography ── */
    h3 { margin: 0; font-family: 'Space Grotesk', sans-serif; color: var(--ink); font-size: 1.15rem; font-weight: 600; letter-spacing: -0.02em; }
    .muted { color: var(--muted); line-height: 1.65; font-size: 0.9rem; }
    .eyebrow { text-transform: uppercase; letter-spacing: 0.14em; font-size: 0.68rem; font-weight: 700; color: var(--primary-2); margin-bottom: 0.2rem; }
    .section-head { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; margin-bottom: 1rem; }

    /* ── Compact list (dashboard) ── */
    .compact-list { display: grid; gap: 0.5rem; }
    .compact-item {
      display: flex; justify-content: space-between; align-items: center; gap: 1rem;
      padding: 0.85rem 1rem; border-radius: var(--radius);
      border: 1px solid rgba(84, 101, 255, 0.06); background: rgba(248, 249, 255, 0.6);
      transition: all var(--transition);
    }
    .compact-item:hover { border-color: rgba(84, 101, 255, 0.15); background: white; }
    .compact-item strong { font-size: 0.88rem; color: var(--ink); }
    .compact-item p { color: var(--muted); font-size: 0.8rem; margin: 0.15rem 0 0; }
    .compact-right { display: flex; align-items: center; gap: 0.6rem; flex-shrink: 0; }
    .meta-text { color: var(--muted); font-size: 0.8rem; white-space: nowrap; }
    .empty-state { color: var(--muted); font-size: 0.88rem; text-align: center; padding: 1.5rem 0; }

    /* ── Card grids ── */
    .claim-grid, .policy-grid, .user-grid, .audit-list { display: grid; gap: 0.65rem; }
    .claim-card, .policy-card, .user-card, .audit-card {
      border: 1px solid rgba(84, 101, 255, 0.08); border-radius: var(--radius);
      background: rgba(255, 255, 255, 0.8); padding: 1.1rem;
      transition: all var(--transition); position: relative; overflow: hidden;
    }
    .claim-card::before, .policy-card::before {
      content: ''; position: absolute; left: 0; top: 0; bottom: 0; width: 3px;
      background: linear-gradient(180deg, #5465ff, #9bb1ff); border-radius: 0 3px 3px 0;
      opacity: 0; transition: opacity var(--transition);
    }
    .claim-card, .policy-card { cursor: pointer; }
    .claim-card:hover, .policy-card:hover { border-color: rgba(84, 101, 255, 0.18); box-shadow: var(--shadow-sm); transform: translateY(-1px); }
    .claim-card:hover::before, .policy-card:hover::before { opacity: 1; }
    .claim-card.selected, .policy-card.selected { border-color: rgba(84, 101, 255, 0.3); box-shadow: 0 0 0 3px rgba(84, 101, 255, 0.08); }
    .claim-card.selected::before, .policy-card.selected::before { opacity: 1; }
    .user-card:hover, .audit-card:hover { border-color: rgba(84, 101, 255, 0.15); box-shadow: var(--shadow-sm); }
    .claim-card p, .policy-card p, .user-card p, .audit-card p { color: var(--muted); margin-top: 0.2rem; font-size: 0.84rem; }

    .policy-top { display: flex; justify-content: space-between; gap: 1rem; margin-bottom: 0.6rem; }
    .policy-meta { display: flex; flex-wrap: wrap; gap: 0.5rem; }
    .policy-meta span { background: rgba(84, 101, 255, 0.06); padding: 0.25rem 0.55rem; border-radius: var(--radius-full); font-size: 0.78rem; font-weight: 500; color: var(--text); }

    /* ── User card ── */
    .user-card { display: flex; justify-content: space-between; align-items: center; gap: 1rem; }
    .user-info { display: flex; align-items: center; gap: 0.85rem; }
    .avatar {
      width: 38px; height: 38px; border-radius: 50%;
      background: linear-gradient(135deg, #5465ff, #788bff );
      color: white; display: flex; align-items: center; justify-content: center;
      font-weight: 700; font-size: 0.9rem; flex-shrink: 0;
    }

    /* ── Audit card ── */
    .audit-card { display: flex; align-items: center; gap: 0.85rem; }
    .audit-icon {
      width: 32px; height: 32px; border-radius: var(--radius-sm);
      background: rgba(84, 101, 255, 0.08); flex-shrink: 0;
      display: flex; align-items: center; justify-content: center; color: var(--primary);
    }
    .audit-body { flex: 1; min-width: 0; }
    .audit-body strong { font-size: 0.86rem; }

    /* ── Action card ── */
    .action-card {
      margin-top: 1rem; border-radius: var(--radius); padding: 1.25rem;
      background: linear-gradient(135deg, rgba(84, 101, 255, 0.04) 0%, rgba(226, 253, 255, 0.25) 100%);
      border: 1px solid rgba(84, 101, 255, 0.1); display: grid; gap: 0.85rem;
    }

    /* ── Forms ── */
    .form-row { display: grid; gap: 0.35rem; }
    .form-row.two-col { grid-template-columns: 1fr 1fr; gap: 0.85rem; }
    .form-action { display: flex; align-items: flex-end; }
    label { display: grid; gap: 0.35rem; }
    label span { font-weight: 600; color: var(--ink); font-size: 0.86rem; }
    textarea, input, select {
      border: 1.5px solid rgba(84, 101, 255, 0.12); background: rgba(255, 255, 255, 0.85);
      border-radius: var(--radius-sm); padding: 0.78rem 1rem; color: var(--ink); font-size: 0.9rem;
      transition: all var(--transition);
    }
    textarea:focus, input:focus, select:focus { border-color: var(--primary-2); background: white; box-shadow: 0 0 0 4px rgba(84, 101, 255, 0.08); outline: none; }
    textarea { resize: vertical; }

    /* ── Buttons ── */
    .button-row { display: flex; flex-wrap: wrap; gap: 0.5rem; align-items: center; }
    .primary-button, .secondary-button, .danger-button {
      border: 0; border-radius: var(--radius-sm); padding: 0.78rem 1.2rem;
      font-weight: 700; font-size: 0.88rem; cursor: pointer; transition: all var(--transition);
    }
    .primary-button { color: white; background: linear-gradient(135deg, #5465ff 0%, #788bff 100%); box-shadow: 0 4px 14px rgba(84, 101, 255, 0.25); }
    .primary-button:hover:not(:disabled) { transform: translateY(-1px); box-shadow: 0 6px 20px rgba(84, 101, 255, 0.35); }
    .secondary-button { background: rgba(84, 101, 255, 0.07); color: var(--primary); }
    .secondary-button:hover:not(:disabled) { background: rgba(84, 101, 255, 0.13); }
    .secondary-button.small { padding: 0.5rem 0.85rem; font-size: 0.8rem; }
    .danger-button { background: rgba(239, 68, 68, 0.07); color: #ef4444; border: 1px solid rgba(239, 68, 68, 0.12); }
    .danger-button:hover { background: rgba(239, 68, 68, 0.13); border-color: rgba(239, 68, 68, 0.25); }
    .ghost-link { background: transparent; border: 0; color: var(--primary); cursor: pointer; font-weight: 700; font-size: 0.82rem; padding: 0.2rem 0; white-space: nowrap; }
    .ghost-link:hover { text-decoration: underline; }
    .action-error { color: #ef4444; font-size: 0.88rem; margin: 0.5rem 0 0; font-weight: 500; }
    .action-hint { color: #d97706; font-size: 0.85rem; margin: 0.5rem 0 0; }
    .terminal-state {
      display: flex; align-items: center; gap: 0.6rem;
      padding: 0.9rem 1rem; border-radius: var(--radius-sm);
      background: rgba(84, 101, 255, 0.04); border: 1px solid rgba(84, 101, 255, 0.1);
      color: var(--muted); font-size: 0.9rem; margin-top: 0.5rem;
    }
    .terminal-state svg { width: 1.1rem; height: 1.1rem; flex-shrink: 0; color: var(--primary-2); }
    .terminal-state strong { color: var(--ink); }
    .primary-button:disabled, .secondary-button:disabled, .danger-button:disabled {
      opacity: 0.38; cursor: not-allowed; pointer-events: none;
    }

    /* ── User tab extras ── */
    .user-role-tag { font-size: 0.72rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.06em; color: var(--primary); background: rgba(84,101,255,0.08); padding: 0.15rem 0.5rem; border-radius: var(--radius-full); display: inline-block; margin-top: 0.25rem; }
    .user-actions { display: flex; align-items: center; gap: 1rem; flex-shrink: 0; }
    .role-select label { gap: 0.2rem; }
    .role-select select { padding: 0.45rem 0.7rem; font-size: 0.82rem; min-width: 110px; }

    @media (max-width: 900px) {
      .kpi-strip { grid-template-columns: repeat(2, 1fr); }
      .dashboard-grid { grid-template-columns: 1fr; }
      .form-row.two-col { grid-template-columns: 1fr; }
    }
    @media (max-width: 600px) { .kpi-strip { grid-template-columns: 1fr; } .tab-bar { gap: 0.2rem; } .tab-bar button { padding: 0.5rem 0.7rem; font-size: 0.78rem; } }
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
  actionError = '';
  actionLoading = false;

  get pendingClaims(): ClaimDto[] {
    return this.claims.filter(c => c.status === 'SUBMITTED' || c.status === 'UNDER_REVIEW').slice(0, 5);
  }

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
      },
      error: (err) => {
        console.error('Admin dashboard load failed:', err);
      }
    });
  }

  markUnderReview(): void {
    if (!this.selectedClaim) return;
    this.actionError = '';
    this.actionLoading = true;
    this.claimsService.markUnderReview(this.selectedClaim.claimId, { note: this.reviewNote }).subscribe({
      next: (claim) => { this.selectedClaim = claim; this.loadData(); this.actionLoading = false; },
      error: (err) => { this.actionError = this.resolveError(err); this.actionLoading = false; }
    });
  }

  approveClaim(): void {
    if (!this.selectedClaim) return;
    if (this.selectedClaim.status !== 'UNDER_REVIEW') {
      this.actionError = 'Claim must be marked "Under Review" before it can be approved.';
      return;
    }
    this.actionError = '';
    this.actionLoading = true;
    this.claimsService.approveClaim(this.selectedClaim.claimId, { approvedAmount: this.approvedAmount || undefined, note: this.reviewNote }).subscribe({
      next: (claim) => { this.selectedClaim = claim; this.loadData(); this.actionLoading = false; },
      error: (err) => { this.actionError = this.resolveError(err); this.actionLoading = false; }
    });
  }

  rejectClaim(): void {
    if (!this.selectedClaim) return;
    if (this.selectedClaim.status !== 'UNDER_REVIEW') {
      this.actionError = 'Claim must be marked "Under Review" before it can be rejected.';
      return;
    }
    if (!this.rejectReason && !this.reviewNote) {
      this.actionError = 'Please provide a rejection reason.';
      return;
    }
    this.actionError = '';
    this.actionLoading = true;
    this.claimsService.rejectClaim(this.selectedClaim.claimId, { reason: this.rejectReason || this.reviewNote }).subscribe({
      next: (claim) => { this.selectedClaim = claim; this.loadData(); this.actionLoading = false; },
      error: (err) => { this.actionError = this.resolveError(err); this.actionLoading = false; }
    });
  }

  private resolveError(error: unknown): string {
    const r = error as { error?: { detail?: string; title?: string }; message?: string };
    return r?.error?.detail ?? r?.error?.title ?? r?.message ?? 'Something went wrong.';
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

  changeRole(user: ProfileDto, newRole: string): void {
    this.authService.updateUserRole(user.userId, newRole).subscribe({
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
