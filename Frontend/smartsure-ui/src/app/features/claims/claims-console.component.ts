import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import {
  ClaimDocumentDto,
  ClaimDto,
  CreateClaimDto,
  PolicyDto,
  UploadClaimDocumentDto
} from '../../models/api-models';
import { AuthStateService } from '../../core/services/auth-state.service';
import { ClaimsService } from '../../core/services/claims.service';
import { PolicyService } from '../../core/services/policy.service';
import { SharedModule } from '../../shared/shared.module';

@Component({
  selector: 'app-claims-console',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, SharedModule],
  template: `
    <section class="page-shell">

      <!-- ═══════════ PAGE HEADER ═══════════ -->
      <header class="page-header">
        <div class="header-left">
          <div class="page-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2"/><rect x="9" y="3" width="6" height="4" rx="1"/><path d="M9 14l2 2 4-4"/></svg>
          </div>
          <div>
            <p class="eyebrow">Claims</p>
            <h1 class="page-title" *ngIf="view === 'list'">My Claims</h1>
            <h1 class="page-title" *ngIf="view === 'new'">File a New Claim</h1>
            <h1 class="page-title" *ngIf="view === 'detail'">Claim Details</h1>
          </div>
        </div>
        <nav class="page-tabs">
          <button type="button" [class.active]="view === 'list'" (click)="go('/claims/my-claims')">
            <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M2 4h12"/><path d="M2 8h12"/><path d="M2 12h8"/></svg>
            My Claims
          </button>
          <button type="button" [class.active]="view === 'new'" (click)="go('/claims/new')">
            <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5"><circle cx="8" cy="8" r="6"/><path d="M8 5v6"/><path d="M5 8h6"/></svg>
            New Claim
          </button>
          <button type="button" [class.active]="view === 'detail'" [class.disabled]="!selectedClaim" (click)="selectedClaim && go('/claims/' + selectedClaim.claimId)">
            <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M10 2H4a1 1 0 00-1 1v10a1 1 0 001 1h8a1 1 0 001-1V5z"/><path d="M10 2v3h3"/></svg>
            Claim Detail
          </button>
        </nav>
      </header>

      <!-- ═══════════ MY CLAIMS LIST ═══════════ -->
      <div class="view-container" *ngIf="view === 'list'">
        <div class="stats-row">
          <div class="stat-card">
            <div class="stat-icon claims-icon">
              <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M9 5H7a2 2 0 00-2 2v8a2 2 0 002 2h6a2 2 0 002-2V7a2 2 0 00-2-2h-2"/><rect x="7" y="3" width="6" height="3" rx="1"/></svg>
            </div>
            <div><span class="stat-label">Total Claims</span><strong class="stat-value">{{ claims.length }}</strong></div>
          </div>
          <div class="stat-card">
            <div class="stat-icon policies-icon">
              <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M9 2H4a2 2 0 00-2 2v12a2 2 0 002 2h8a2 2 0 002-2V7z"/><path d="M9 2v5h5"/></svg>
            </div>
            <div><span class="stat-label">Eligible Policies</span><strong class="stat-value">{{ claimablePolicies.length }}</strong></div>
          </div>
          <div class="stat-card action-stat" (click)="go('/claims/new')">
            <div class="stat-icon new-icon">
              <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.5"><circle cx="10" cy="10" r="8"/><path d="M10 6v8"/><path d="M6 10h8"/></svg>
            </div>
            <div><span class="stat-label">Quick Action</span><strong class="stat-value cta-text">New Claim →</strong></div>
          </div>
        </div>

        <div class="content-card" *ngIf="claims.length > 0">
          <div class="card-header">
            <h2 class="card-title">
              <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M2 4h16"/><path d="M2 8h16"/><path d="M2 12h10"/></svg>
              All Claims
            </h2>
          </div>
          <div class="claim-list">
            <article class="claim-row" *ngFor="let claim of claims" (click)="openClaim(claim)">
              <div class="claim-info">
                <strong>{{ claim.claimNumber }}</strong>
                <span class="claim-desc">{{ claim.description | slice:0:60 }}{{ claim.description.length > 60 ? '...' : '' }}</span>
              </div>
              <div class="claim-amount">{{ claim.claimAmount | formatCurrency }}</div>
              <app-status-badge [value]="claim.status"></app-status-badge>
              <span class="claim-date">{{ claim.createdDate | date:'mediumDate' }}</span>
              <svg class="row-arrow" viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="2"><path d="M6 4l4 4-4 4"/></svg>
            </article>
          </div>
        </div>

        <div class="content-card empty-state" *ngIf="claims.length === 0">
          <div class="empty-icon"><svg viewBox="0 0 48 48" fill="none" stroke="#788bff" stroke-width="1.5"><rect x="10" y="6" width="28" height="36" rx="3" fill="rgba(84,101,255,0.06)"/><path d="M18 18h12"/><path d="M18 24h12"/><path d="M18 30h8"/></svg></div>
          <h3>No claims filed yet</h3>
          <p>You can file a claim against any of your active policies. Claims go through a review process before approval.</p>
          <button type="button" class="primary-button" (click)="go('/claims/new')">
            <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="2"><circle cx="8" cy="8" r="6"/><path d="M8 5v6"/><path d="M5 8h6"/></svg>
            File Your First Claim
          </button>
        </div>
      </div>

      <!-- ═══════════ NEW CLAIM FORM ═══════════ -->
      <div class="view-container" *ngIf="view === 'new'">
        <app-stepper
          [steps]="['Select Policy', 'Claim Details', 'Review']"
          [activeIndex]="claimStep"
          (stepClick)="goToClaimStep($event)">
        </app-stepper>

        <!-- Step 0: Select policy -->
        <div class="content-card" *ngIf="claimStep === 0">
          <div class="card-header">
            <h2 class="card-title">Select a policy to file against</h2>
          </div>
          <div class="policy-select-list" *ngIf="claimablePolicies.length > 0">
            <button type="button" class="policy-select-row"
              *ngFor="let policy of claimablePolicies"
              [class.selected]="createModel.policyId === policy.policyId"
              (click)="createModel.policyId = policy.policyId">
              <div class="policy-select-info">
                <strong>{{ policy.policyNumber }}</strong>
                <span>{{ policy.typeName }} · {{ policy.subTypeName }}</span>
              </div>
              <div class="policy-select-coverage">{{ policy.coverageAmount | formatCurrency }}</div>
              <app-status-badge [value]="policy.status"></app-status-badge>
              <div class="selection-check" *ngIf="createModel.policyId === policy.policyId">
                <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M3 8.5l3 3 7-7"/></svg>
              </div>
            </button>
          </div>
          <div class="empty-state" *ngIf="claimablePolicies.length === 0">
            <div class="empty-icon"><svg viewBox="0 0 48 48" fill="none" stroke="#788bff" stroke-width="1.5"><path d="M24 4L10 10v10c0 10 6 16 14 18 8-2 14-8 14-18V10L24 4z" fill="rgba(84,101,255,0.06)"/><path d="M18 24l4 4 8-8" stroke-linecap="round" stroke-linejoin="round"/></svg></div>
            <h3>No eligible policies</h3>
            <p>All your active policies already have an open or approved claim, or you have no active policies. Purchase a new policy to file another claim.</p>
          </div>
          <div class="step-actions">
            <div></div>
            <button type="button" class="primary-button" [disabled]="!createModel.policyId" (click)="claimStep = 1">
              Next: Enter Details →
            </button>
          </div>
        </div>

        <!-- Step 1: Claim details -->
        <div class="content-card" *ngIf="claimStep === 1">
          <div class="card-header">
            <h2 class="card-title">Describe your claim</h2>
            <span class="selected-product-tag" *ngIf="getSelectedPolicy() as p">{{ p.policyNumber }}</span>
          </div>
          <div class="form-stack">
            <label class="form-field">
              <span class="field-label">What happened?</span>
              <textarea name="description" [(ngModel)]="createModel.description" rows="5" placeholder="Describe the incident and reason for your claim..." required></textarea>
              <span class="field-hint">Provide as much detail as possible to help speed up review</span>
            </label>
            <label class="form-field">
              <span class="field-label">Claim Amount (₹)</span>
              <input name="claimAmount" [(ngModel)]="createModel.claimAmount" type="number" min="1" placeholder="e.g. 50000" required />
              <span class="field-hint">The amount you're requesting. Cannot exceed your coverage amount.</span>
            </label>
          </div>
          <div class="step-actions">
            <button type="button" class="secondary-button" (click)="claimStep = 0">← Back</button>
            <button type="button" class="primary-button"
              [disabled]="!createModel.description || createModel.description.trim().length < 10 || !createModel.claimAmount || createModel.claimAmount <= 0"
              (click)="claimStep = 2">
              Next: Review →
            </button>
          </div>
        </div>

        <!-- Step 2: Review -->
        <div class="content-card" *ngIf="claimStep === 2">
          <div class="card-header">
            <h2 class="card-title">Review & Submit Claim</h2>
          </div>
          <div class="review-grid">
            <div class="review-item"><span>Policy</span><strong>{{ getSelectedPolicy()?.policyNumber || 'N/A' }}</strong></div>
            <div class="review-item"><span>Product</span><strong>{{ getSelectedPolicy()?.typeName }} · {{ getSelectedPolicy()?.subTypeName }}</strong></div>
            <div class="review-item highlight" style="grid-column: 1 / -1;"><span>Claim Amount</span><strong class="premium-amount">{{ createModel.claimAmount | formatCurrency }}</strong></div>
          </div>
          <div class="description-preview">
            <span class="field-label">Description</span>
            <p>{{ createModel.description }}</p>
          </div>
          <p class="error-msg" *ngIf="errorMessage">{{ errorMessage }}</p>
          <div class="step-actions">
            <button type="button" class="secondary-button" (click)="claimStep = 1">← Back</button>
            <button type="button" class="confirm-button" (click)="createClaim()" [disabled]="loading">
              <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 8.5l3 3 7-7"/></svg>
              {{ loading ? 'Creating...' : 'Create Claim Draft' }}
            </button>
          </div>
        </div>
      </div>

      <!-- ═══════════ CLAIM DETAIL ═══════════ -->
      <div class="view-container" *ngIf="view === 'detail'">
        <div class="content-card" *ngIf="selectedClaim; else noClaim">
          <div class="card-header">
            <h2 class="card-title">{{ selectedClaim.claimNumber }}</h2>
            <app-status-badge [value]="selectedClaim.status"></app-status-badge>
          </div>

          <div class="review-grid">
            <div class="review-item"><span>Policy ID</span><strong>{{ selectedClaim.policyId }}</strong></div>
            <div class="review-item"><span>Claim Amount</span><strong>{{ selectedClaim.claimAmount | formatCurrency }}</strong></div>
            <div class="review-item"><span>Created</span><strong>{{ selectedClaim.createdDate | date:'medium' }}</strong></div>
            <div class="review-item"><span>Submitted</span><strong>{{ selectedClaim.submittedAt ? (selectedClaim.submittedAt | date:'medium') : '— Not submitted yet' }}</strong></div>
            <div class="review-item"><span>Reviewed</span><strong>{{ selectedClaim.reviewedAt ? (selectedClaim.reviewedAt | date:'medium') : '— Pending review' }}</strong></div>
            <div class="review-item"><span>Admin Note</span><strong>{{ selectedClaim.adminNote || '— No notes yet' }}</strong></div>
          </div>

          <div class="description-preview">
            <span class="field-label">Description</span>
            <p>{{ selectedClaim.description }}</p>
          </div>

          <div class="step-actions">
            <button type="button" class="secondary-button" (click)="go('/claims/my-claims')">← Back to Claims</button>
            <button type="button" class="primary-button"
              *ngIf="selectedClaim.status === 'DRAFT'"
              (click)="submitClaim()" [disabled]="loading">
              {{ loading ? 'Submitting...' : 'Submit for Review' }}
            </button>
          </div>

          <!-- Terminal state banner -->
          <div class="claim-terminal-banner" *ngIf="selectedClaim.status !== 'DRAFT'">
            <ng-container [ngSwitch]="selectedClaim.status">
              <span *ngSwitchCase="'SUBMITTED'">
                <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5"><circle cx="8" cy="8" r="6"/><path d="M8 5v3l2 2"/></svg>
                Your claim has been submitted and is awaiting admin review.
              </span>
              <span *ngSwitchCase="'UNDER_REVIEW'">
                <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5"><circle cx="8" cy="8" r="6"/><path d="M8 5v3l2 2"/></svg>
                Your claim is currently under review by our team.
              </span>
              <span *ngSwitchCase="'APPROVED'">
                <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5"><circle cx="8" cy="8" r="6"/><path d="M5 8l2 2 4-4"/></svg>
                Your claim has been <strong>approved</strong>.{{ selectedClaim.adminNote ? ' Note: "' + selectedClaim.adminNote + '"' : '' }}
              </span>
              <span *ngSwitchCase="'REJECTED'">
                <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5"><circle cx="8" cy="8" r="6"/><path d="M6 6l4 4M10 6l-4 4"/></svg>
                Your claim has been <strong>rejected</strong>.{{ selectedClaim.adminNote ? ' Reason: "' + selectedClaim.adminNote + '"' : '' }}
              </span>
              <span *ngSwitchDefault>
                <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5"><circle cx="8" cy="8" r="6"/><path d="M8 5v3"/><circle cx="8" cy="11" r="0.5" fill="currentColor"/></svg>
                This claim is {{ selectedClaim.status | lowercase }} and no further action is required.
              </span>
            </ng-container>
          </div>
          <p class="error-msg" *ngIf="errorMessage">{{ errorMessage }}</p>
        </div>

        <!-- Documents section -->
        <div class="content-card" *ngIf="selectedClaim">
          <div class="card-header">
            <h2 class="card-title">
              <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M14 2H6a2 2 0 00-2 2v12a2 2 0 002 2h8a2 2 0 002-2V4a2 2 0 00-2-2z"/><path d="M10 10V6"/><path d="M8 8l2-2 2 2"/></svg>
              {{ selectedClaim.status === 'DRAFT' ? 'Upload Documents' : 'Submitted Documents' }}
            </h2>
            <button type="button" class="ghost-link" (click)="loadDocuments()">↻ Refresh</button>
          </div>

          <!-- Upload zone — only for DRAFT claims -->
          <ng-container *ngIf="selectedClaim.status === 'DRAFT'">
            <div class="upload-zone">
              <input type="file" id="fileInput" (change)="onFileSelected($event)" class="file-input" />
              <label for="fileInput" class="file-label">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4"/><path d="M17 8l-5-5-5 5"/><path d="M12 3v12"/></svg>
                <span *ngIf="!uploadModel.fileName">Choose a file to upload</span>
                <span *ngIf="uploadModel.fileName">{{ uploadModel.fileName }} ({{ uploadModel.fileSizeKb }} KB)</span>
              </label>
            </div>
            <div class="step-actions" *ngIf="uploadModel.contentBase64">
              <div></div>
              <button type="button" class="primary-button" (click)="uploadDocument()" [disabled]="loading">
                {{ loading ? 'Uploading...' : 'Upload Document' }}
              </button>
            </div>
          </ng-container>

          <div class="doc-list" *ngIf="documents.length > 0">
            <article class="doc-row" *ngFor="let doc of documents">
              <div class="doc-info">
                <div class="doc-icon">
                  <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" width="16" height="16"><path d="M10 2H4a1 1 0 00-1 1v10a1 1 0 001 1h8a1 1 0 001-1V5z"/><path d="M10 2v3h3"/></svg>
                </div>
                <div>
                  <strong>{{ doc.fileName }}</strong>
                  <span>{{ doc.fileType | uppercase }} · {{ doc.fileSizeKb }} KB · {{ doc.uploadedAt | date:'mediumDate' }}</span>
                </div>
              </div>
              <div class="doc-actions">
                <a [href]="doc.fileUrl" target="_blank" rel="noopener noreferrer" class="doc-link">Open ↗</a>
                <!-- Delete only allowed on DRAFT claims -->
                <button type="button" class="doc-delete"
                  *ngIf="selectedClaim.status === 'DRAFT'"
                  (click)="deleteDocument(doc.docId)" [disabled]="loading"
                  title="Delete document">
                  <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" width="14" height="14"><path d="M2 4h12"/><path d="M5 4V2h6v2"/><path d="M3 4l1 10h8l1-10"/></svg>
                </button>
              </div>
            </article>
          </div>
          <p class="hint" *ngIf="documents.length === 0" style="text-align:center; padding: 1rem 0;">
            {{ selectedClaim.status === 'DRAFT' ? 'No documents uploaded yet. Attach supporting evidence to strengthen your claim.' : 'No documents were submitted with this claim.' }}
          </p>
        </div>

        <ng-template #noClaim>
          <div class="content-card empty-state">
            <div class="empty-icon"><svg viewBox="0 0 48 48" fill="none" stroke="#788bff" stroke-width="1.5"><path d="M28 6H14a4 4 0 00-4 4v28a4 4 0 004 4h20a4 4 0 004-4V16L28 6z" fill="rgba(84,101,255,0.06)"/><path d="M28 6v10h10"/><path d="M20 26h8"/><path d="M20 32h4"/></svg></div>
            <h3>No claim selected</h3>
            <p>Select a claim from the list or file a new one.</p>
            <button type="button" class="secondary-button" (click)="go('/claims/my-claims')">View My Claims</button>
          </div>
        </ng-template>
      </div>
    </section>
  `,
  styles: [`
    :host { display: block; animation: fadeInUp 0.4s ease; }
    .page-shell { display: grid; gap: 1.25rem; }

    /* ── Page Header ── */
    .page-header {
      display: flex; justify-content: space-between; align-items: center;
      flex-wrap: wrap; gap: 1rem;
    }
    .header-left { display: flex; align-items: center; gap: 0.85rem; }
    .page-icon {
      width: 2.6rem; height: 2.6rem; border-radius: var(--radius, 14px);
      background: linear-gradient(135deg, #5465ff 0%, #788bff 100%);
      display: grid; place-items: center; flex-shrink: 0;
    }
    .page-icon svg { width: 1.3rem; height: 1.3rem; color: white; }
    .eyebrow { text-transform: uppercase; letter-spacing: 0.14em; font-size: 0.65rem; font-weight: 700; color: var(--primary-2, #788bff); margin: 0; }
    .page-title { margin: 0; font-family: 'Space Grotesk', sans-serif; font-size: 1.4rem; font-weight: 700; color: var(--ink, #14213d); letter-spacing: -0.02em; line-height: 1.2; }

    .page-tabs { display: flex; gap: 0.3rem; background: rgba(84, 101, 255, 0.05); padding: 4px; border-radius: var(--radius-full, 999px); }
    .page-tabs button {
      display: flex; align-items: center; gap: 0.4rem;
      border: 0; border-radius: var(--radius-full, 999px); padding: 0.55rem 1rem;
      background: transparent; color: var(--muted, #6b7a99); font-weight: 600; font-size: 0.82rem;
      cursor: pointer; transition: all 0.2s ease; white-space: nowrap;
    }
    .page-tabs button svg { width: 14px; height: 14px; }
    .page-tabs button:hover:not(.disabled) { color: var(--primary, #5465ff); background: rgba(255,255,255,0.6); }
    .page-tabs button.active { background: linear-gradient(135deg, #5465ff 0%, #788bff 100%); color: white; box-shadow: 0 4px 14px rgba(84, 101, 255, 0.3); }
    .page-tabs button.disabled { opacity: 0.4; cursor: not-allowed; }

    .view-container { display: grid; gap: 1.25rem; }

    /* ── Stats Row ── */
    .stats-row { display: grid; grid-template-columns: repeat(3, 1fr); gap: 1rem; }
    .stat-card {
      display: flex; align-items: center; gap: 1rem;
      border-radius: var(--radius, 14px); padding: 1.15rem 1.25rem;
      background: rgba(255, 255, 255, 0.88); backdrop-filter: blur(8px);
      border: 1px solid rgba(84, 101, 255, 0.08); transition: all 0.2s ease;
    }
    .stat-card:hover { box-shadow: var(--shadow, 0 2px 8px rgba(0,0,0,0.06)); }
    .action-stat { cursor: pointer; }
    .action-stat:hover { border-color: rgba(84, 101, 255, 0.2); transform: translateY(-1px); }
    .stat-icon { width: 2.6rem; height: 2.6rem; border-radius: var(--radius-sm, 10px); display: grid; place-items: center; flex-shrink: 0; }
    .stat-icon svg { width: 1.2rem; height: 1.2rem; }
    .claims-icon { background: rgba(84, 101, 255, 0.08); color: #5465ff; }
    .policies-icon { background: rgba(16, 185, 129, 0.08); color: #059669; }
    .new-icon { background: rgba(245, 158, 11, 0.08); color: #d97706; }
    .stat-label { display: block; font-size: 0.75rem; font-weight: 600; color: var(--muted); text-transform: uppercase; letter-spacing: 0.04em; }
    .stat-value { display: block; font-size: 1.35rem; font-weight: 700; font-family: 'Space Grotesk', sans-serif; color: var(--ink); }
    .cta-text { font-size: 1rem; color: #d97706; }

    /* ── Content Cards ── */
    .content-card {
      border-radius: var(--radius-lg, 20px); padding: 1.5rem 1.75rem;
      background: rgba(255, 255, 255, 0.9); backdrop-filter: blur(12px);
      border: 1px solid rgba(84, 101, 255, 0.08);
      box-shadow: var(--shadow, 0 2px 8px rgba(0,0,0,0.04));
    }
    .card-header { display: flex; justify-content: space-between; align-items: center; gap: 1rem; margin-bottom: 1.25rem; flex-wrap: wrap; }
    .card-title { margin: 0; font-size: 1.15rem; font-weight: 700; font-family: 'Space Grotesk', sans-serif; color: var(--ink); display: flex; align-items: center; gap: 0.5rem; }
    .card-title svg { width: 1.1rem; height: 1.1rem; color: var(--primary-2, #788bff); }

    /* ── Claim List ── */
    .claim-list { display: grid; gap: 0.6rem; }
    .claim-row {
      display: grid; grid-template-columns: 1fr auto auto auto 24px; align-items: center; gap: 1rem;
      border-radius: var(--radius, 14px); padding: 1rem 1.15rem;
      border: 1px solid rgba(84, 101, 255, 0.08); background: rgba(255, 255, 255, 0.7);
      cursor: pointer; transition: all 0.2s ease;
    }
    .claim-row:hover { border-color: rgba(84, 101, 255, 0.2); box-shadow: var(--shadow-sm, 0 2px 6px rgba(0,0,0,0.04)); transform: translateX(4px); }
    .claim-info strong { display: block; font-size: 0.9rem; color: var(--ink); }
    .claim-desc { display: block; font-size: 0.78rem; color: var(--muted); margin-top: 2px; }
    .claim-amount { font-weight: 700; color: var(--primary, #5465ff); font-size: 0.9rem; white-space: nowrap; }
    .claim-date { font-size: 0.78rem; color: var(--muted); white-space: nowrap; }
    .row-arrow { width: 16px; height: 16px; color: var(--muted); transition: transform 0.2s; }
    .claim-row:hover .row-arrow { transform: translateX(3px); color: var(--primary); }

    /* ── Policy Select List (for new claim) ── */
    .policy-select-list { display: grid; gap: 0.6rem; }
    .policy-select-row {
      all: unset; cursor: pointer; display: grid; grid-template-columns: 1fr auto auto; align-items: center; gap: 1rem;
      border-radius: var(--radius, 14px); padding: 1rem 1.15rem;
      border: 1.5px solid rgba(84, 101, 255, 0.1); background: rgba(255, 255, 255, 0.75);
      transition: all 0.2s ease; position: relative;
    }
    .policy-select-row:hover { border-color: rgba(84, 101, 255, 0.25); box-shadow: var(--shadow-sm); }
    .policy-select-row.selected { border-color: var(--primary, #5465ff); background: rgba(84, 101, 255, 0.03); }
    .policy-select-info strong { display: block; font-size: 0.9rem; color: var(--ink); }
    .policy-select-info span { display: block; font-size: 0.78rem; color: var(--muted); margin-top: 2px; }
    .policy-select-coverage { font-weight: 700; color: var(--primary); font-size: 0.9rem; }
    .selection-check { position: absolute; top: 0.75rem; right: 0.75rem; width: 1.5rem; height: 1.5rem; background: #059669; border-radius: 999px; display: grid; place-items: center; }
    .selection-check svg { width: 0.9rem; height: 0.9rem; color: white; }
    .selected-product-tag { background: rgba(84, 101, 255, 0.08); color: #5465ff; font-size: 0.78rem; font-weight: 600; padding: 0.35rem 0.75rem; border-radius: var(--radius-full, 999px); }

    /* ── Forms ── */
    .form-stack { display: grid; gap: 1rem; }
    .form-field { display: grid; gap: 0.3rem; }
    .field-label { font-weight: 600; color: var(--ink); font-size: 0.88rem; }
    .field-hint { font-size: 0.75rem; color: var(--muted); }
    input, select, textarea {
      border: 1.5px solid rgba(84, 101, 255, 0.14); background: rgba(255, 255, 255, 0.8);
      border-radius: var(--radius-sm, 10px); padding: 0.82rem 1rem; color: var(--ink); font-size: 0.92rem;
      transition: all 0.2s ease; font-family: inherit;
    }
    input:focus, select:focus, textarea:focus { border-color: var(--primary-2, #788bff); background: white; box-shadow: 0 0 0 4px rgba(84, 101, 255, 0.1); outline: none; }
    textarea { resize: vertical; min-height: 6rem; }

    /* ── Review Grid ── */
    .review-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 0.75rem; margin-bottom: 1.25rem; }
    .review-item { display: grid; gap: 0.3rem; padding: 1rem; border-radius: var(--radius-sm, 10px); background: rgba(255, 255, 255, 0.75); border: 1px solid rgba(84, 101, 255, 0.06); }
    .review-item span { font-size: 0.78rem; color: var(--muted); font-weight: 500; text-transform: uppercase; letter-spacing: 0.04em; }
    .review-item strong { font-size: 0.95rem; color: var(--ink); }
    .review-item.highlight { background: linear-gradient(135deg, rgba(84, 101, 255, 0.06) 0%, rgba(226, 253, 255, 0.25) 100%); border-color: rgba(84, 101, 255, 0.12); }
    .premium-amount { font-size: 1.4rem !important; font-family: 'Space Grotesk', sans-serif; color: var(--primary, #5465ff) !important; }
    .description-preview { padding: 1rem; border-radius: var(--radius-sm, 10px); background: rgba(255, 255, 255, 0.6); border: 1px solid rgba(84, 101, 255, 0.06); margin-bottom: 0.75rem; }
    .description-preview p { color: var(--ink); font-size: 0.92rem; line-height: 1.65; margin: 0.3rem 0 0; }

    /* ── Upload Zone ── */
    .upload-zone { margin-bottom: 1rem; }
    .file-input { display: none; }
    .file-label {
      display: flex; align-items: center; justify-content: center; gap: 0.75rem;
      border: 2px dashed rgba(84, 101, 255, 0.15); border-radius: var(--radius, 14px);
      padding: 1.5rem; cursor: pointer; text-align: center; color: var(--muted);
      font-weight: 500; font-size: 0.9rem; transition: all 0.2s ease;
    }
    .file-label:hover { border-color: rgba(84, 101, 255, 0.3); background: rgba(84, 101, 255, 0.02); }
    .file-label svg { width: 1.5rem; height: 1.5rem; color: var(--primary-2); }

    /* ── Doc List ── */
    .doc-list { display: grid; gap: 0.5rem; margin-top: 1rem; }
    .doc-row {
      display: flex; justify-content: space-between; align-items: center; gap: 1rem;
      padding: 0.75rem 1rem; border-radius: var(--radius-sm, 10px);
      border: 1px solid rgba(84, 101, 255, 0.08); background: rgba(255, 255, 255, 0.7);
      transition: all 0.2s ease;
    }
    .doc-row:hover { border-color: rgba(84, 101, 255, 0.2); }
    .doc-info { display: flex; align-items: center; gap: 0.65rem; min-width: 0; }
    .doc-icon {
      width: 2rem; height: 2rem; border-radius: 8px; flex-shrink: 0;
      background: rgba(84,101,255,0.08); display: flex; align-items: center; justify-content: center;
      color: var(--primary-2);
    }
    .doc-info strong { display: block; font-size: 0.88rem; color: var(--ink); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 280px; }
    .doc-info span { display: block; font-size: 0.75rem; color: var(--muted); }
    .doc-delete {
      background: none; border: none; cursor: pointer; padding: 0.3rem;
      color: var(--muted); border-radius: 6px; display: flex; align-items: center;
      transition: color 0.15s, background 0.15s;
    }
    .doc-delete svg { width: 1rem; height: 1rem; }
    .doc-delete:hover:not(:disabled) { color: #ef4444; background: rgba(239,68,68,0.08); }
    .doc-delete:disabled { opacity: 0.35; cursor: not-allowed; }

    /* ── Step Actions ── */
    .step-actions { display: flex; justify-content: space-between; align-items: center; gap: 1rem; margin-top: 1.25rem; padding-top: 1.25rem; border-top: 1px solid rgba(84, 101, 255, 0.06); }

    /* ── Buttons ── */
    .primary-button, .secondary-button, .confirm-button {
      border: 0; border-radius: var(--radius-sm, 10px); padding: 0.82rem 1.5rem;
      font-weight: 700; font-size: 0.88rem; cursor: pointer; transition: all 0.2s ease;
      display: inline-flex; align-items: center; gap: 0.4rem;
    }
    .primary-button { color: white; background: linear-gradient(135deg, #5465ff 0%, #788bff 100%); box-shadow: 0 4px 14px rgba(84, 101, 255, 0.25); }
    .primary-button:hover:not(:disabled) { transform: translateY(-1px); box-shadow: 0 6px 20px rgba(84, 101, 255, 0.35); }
    .primary-button:disabled, .confirm-button:disabled { opacity: 0.55; cursor: not-allowed; }
    .primary-button svg { width: 16px; height: 16px; }
    .secondary-button { background: rgba(84, 101, 255, 0.07); color: var(--primary, #5465ff); }
    .secondary-button:hover { background: rgba(84, 101, 255, 0.12); }
    .confirm-button { color: white; background: linear-gradient(135deg, #059669, #10b981); box-shadow: 0 4px 14px rgba(5, 150, 105, 0.25); }
    .confirm-button:hover:not(:disabled) { transform: translateY(-1px); box-shadow: 0 6px 20px rgba(5, 150, 105, 0.35); }
    .confirm-button svg { width: 16px; height: 16px; }
    .ghost-link { background: transparent; border: 0; color: var(--primary, #5465ff); cursor: pointer; font-weight: 700; font-size: 0.82rem; padding: 0.3rem 0; }
    .hint { color: var(--muted); font-size: 0.85rem; }
    .error-msg { color: #d1495b; font-size: 0.88rem; margin: 0.5rem 0 0; }

    /* ── Claim terminal banner ── */
    .claim-terminal-banner {
      display: flex; align-items: flex-start; gap: 0.6rem;
      padding: 0.9rem 1rem; border-radius: var(--radius-sm, 10px);
      margin-top: 0.75rem; font-size: 0.9rem; line-height: 1.5;
      border: 1px solid rgba(84,101,255,0.12);
      background: rgba(84,101,255,0.04); color: var(--muted);
    }
    .claim-terminal-banner svg { width: 1rem; height: 1rem; flex-shrink: 0; margin-top: 2px; }
    .claim-terminal-banner strong { color: var(--ink); }

    /* ── Empty State ── */
    .empty-state { text-align: center; padding: 2.5rem 1.5rem; }
    .empty-icon { width: 3rem; height: 3rem; margin: 0 auto 0.5rem; color: var(--primary-2, #788bff); }
    .empty-icon svg { width: 100%; height: 100%; }
    .empty-state h3 { margin: 0 0 0.3rem; font-family: 'Space Grotesk', sans-serif; font-weight: 700; color: var(--ink); }
    .empty-state p { color: var(--muted); font-size: 0.9rem; margin: 0 0 1rem; max-width: 400px; margin-left: auto; margin-right: auto; }

    @media (max-width: 1100px) { .stats-row { grid-template-columns: 1fr 1fr; } }
    @media (max-width: 768px) {
      .page-header { flex-direction: column; align-items: flex-start; }
      .stats-row, .review-grid { grid-template-columns: 1fr; }
      .claim-row { grid-template-columns: 1fr auto auto; }
      .claim-date { display: none; }
    }
  `]
})
export class ClaimsConsoleComponent implements OnInit {
  view: 'list' | 'new' | 'detail' = 'list';
  claimStep = 0;
  loading = false;
  errorMessage = '';
  policies: PolicyDto[] = [];
  claims: ClaimDto[] = [];
  documents: ClaimDocumentDto[] = [];
  selectedClaim: ClaimDto | null = null;

  createModel: CreateClaimDto = {
    policyId: '',
    description: '',
    claimAmount: 0
  };

  uploadModel: UploadClaimDocumentDto = {
    fileName: '',
    fileType: '',
    fileSizeKb: 1,
    contentBase64: ''
  };

  constructor(
    private readonly claimsService: ClaimsService,
    private readonly policyService: PolicyService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    public readonly authState: AuthStateService
  ) {}

  ngOnInit(): void {
    this.syncRoute();
    this.loadData();
    this.route.params.subscribe((params) => {
      const claimId = params['id'] as string | undefined;
      if (claimId) {
        this.loadClaim(claimId);
      }
      this.syncRoute();
    });
  }

  go(path: string): void {
    this.errorMessage = '';
    if (path.includes('/new')) {
      this.claimStep = 0;
    }
    void this.router.navigateByUrl(path);
  }

  goToClaimStep(index: number): void {
    if (index < this.claimStep) {
      this.claimStep = index;
    }
  }

  getSelectedPolicy(): PolicyDto | undefined {
    return this.policies.find(p => p.policyId === this.createModel.policyId);
  }

  get activePolicies(): PolicyDto[] {
    return this.policies.filter(p => p.status === 'ACTIVE');
  }

  /** Policies that are ACTIVE and don't already have an open or approved claim. */
  get claimablePolicies(): PolicyDto[] {
    // Policy IDs that already have a claim in a blocking state
    const blockedPolicyIds = new Set(
      this.claims
        .filter(c => c.status === 'SUBMITTED' || c.status === 'UNDER_REVIEW' || c.status === 'APPROVED')
        .map(c => c.policyId)
    );
    return this.policies.filter(p => p.status === 'ACTIVE' && !blockedPolicyIds.has(p.policyId));
  }

  openClaim(claim: ClaimDto): void {
    this.selectedClaim = claim;
    this.view = 'detail';
    void this.router.navigateByUrl(`/claims/${claim.claimId}`);
    this.loadDocuments();
  }

  createClaim(): void {
    this.loading = true;
    this.errorMessage = '';
    this.claimsService.createClaim(this.createModel).subscribe({
      next: (claim) => {
        this.selectedClaim = claim;
        this.view = 'detail';
        void this.router.navigateByUrl(`/claims/${claim.claimId}`);
        this.loadData();
        this.loadDocuments();
      },
      error: (err) => {
        this.errorMessage = this.resolveError(err);
        this.loading = false;
      },
      complete: () => { this.loading = false; }
    });
  }

  submitClaim(): void {
    if (!this.selectedClaim) return;
    this.loading = true;
    this.errorMessage = '';
    this.claimsService.submitClaim(this.selectedClaim.claimId).subscribe({
      next: (claim) => {
        this.selectedClaim = claim;
        this.loadData();
      },
      error: (err) => {
        this.errorMessage = this.resolveError(err);
        this.loading = false;
      },
      complete: () => { this.loading = false; }
    });
  }

  loadDocuments(): void {
    if (!this.selectedClaim) return;
    this.claimsService.getDocuments(this.selectedClaim.claimId).subscribe({
      next: (docs) => { this.documents = docs; },
      error: (err) => { this.errorMessage = this.resolveError(err); }
    });
  }

  uploadDocument(): void {
    if (!this.selectedClaim) return;
    this.loading = true;
    this.errorMessage = '';
    this.claimsService.uploadDocument(this.selectedClaim.claimId, this.uploadModel).subscribe({
      next: () => {
        this.loadDocuments();
        this.uploadModel = { fileName: '', fileType: '', fileSizeKb: 1, contentBase64: '' };
      },
      error: (err) => {
        this.errorMessage = this.resolveError(err);
        this.loading = false;
      },
      complete: () => { this.loading = false; }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    // Extract extension — backend expects 'pdf', 'jpg', or 'png'
    const ext = file.name.split('.').pop()?.toLowerCase() ?? '';
    const allowedExt = ext === 'jpeg' ? 'jpg' : ext;

    const reader = new FileReader();
    reader.onload = () => {
      const result = String(reader.result ?? '');
      this.uploadModel.fileName    = file.name;
      this.uploadModel.fileType    = allowedExt;
      this.uploadModel.fileSizeKb  = Math.max(1, Math.ceil(file.size / 1024));
      this.uploadModel.contentBase64 = result.includes(',') ? result.split(',')[1] : result;
    };
    reader.readAsDataURL(file);
  }

  deleteDocument(docId: string): void {
    if (!this.selectedClaim) return;
    this.errorMessage = '';
    this.loading = true;
    this.claimsService.deleteDocument(this.selectedClaim.claimId, docId).subscribe({
      next: () => { this.loadDocuments(); this.loading = false; },
      error: (err) => { this.errorMessage = this.resolveError(err); this.loading = false; }
    });
  }

  private resolveError(error: unknown): string {
    const r = error as { error?: { detail?: string; title?: string }; message?: string };
    return r?.error?.detail ?? r?.error?.title ?? r?.message ?? 'Something went wrong. Please try again.';
  }

  private loadData(): void {
    forkJoin({
      policies: this.policyService.getMyPolicies(),
      claims: this.claimsService.getMyClaims()
    }).subscribe({
      next: ({ policies, claims }) => {
        this.policies = policies;
        this.claims = claims;
      }
    });
  }

  private loadClaim(id: string): void {
    this.claimsService.getClaim(id).subscribe({
      next: (claim) => {
        this.selectedClaim = claim;
        this.view = 'detail';
        this.loadDocuments();
      }
    });
  }

  private syncRoute(): void {
    const url = this.router.url;
    if (url.includes('/new')) {
      this.view = 'new';
      return;
    }
    if (url.match(/\/claims\/[a-f0-9-]+$/i)) {
      this.view = 'detail';
      return;
    }
    this.view = 'list';
  }
}
