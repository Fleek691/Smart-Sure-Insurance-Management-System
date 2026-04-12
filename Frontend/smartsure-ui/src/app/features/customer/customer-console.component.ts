import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import {
  InsuranceProductDto,
  PolicyDto,
  PremiumCalculationDto
} from '../../models/api-models';
import { AuthStateService } from '../../core/services/auth-state.service';
import { PolicyService } from '../../core/services/policy.service';
import { SharedModule } from '../../shared/shared.module';

@Component({
  selector: 'app-customer-console',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, SharedModule],
  template: `
    <section class="page-shell">

      <!-- ═══════════ PAGE HEADER ═══════════ -->
      <header class="page-header">
        <div class="header-left">
          <div class="page-icon">
            <svg *ngIf="view === 'dashboard'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="3" width="7" height="7" rx="1.5"/><rect x="3" y="14" width="7" height="7" rx="1.5"/><rect x="14" y="14" width="7" height="7" rx="1.5"/></svg>
            <svg *ngIf="view === 'buy'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2L2 7l10 5 10-5-10-5z"/><path d="M2 17l10 5 10-5"/><path d="M2 12l10 5 10-5"/></svg>
            <svg *ngIf="view === 'detail'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z"/><path d="M14 2v6h6"/><path d="M16 13H8"/><path d="M16 17H8"/><path d="M10 9H8"/></svg>
          </div>
          <div>
            <p class="eyebrow">Customer</p>
            <h1 class="page-title" *ngIf="view === 'dashboard'">Dashboard</h1>
            <h1 class="page-title" *ngIf="view === 'buy'">Buy Insurance Policy</h1>
            <h1 class="page-title" *ngIf="view === 'detail'">Policy Details</h1>
          </div>
        </div>
        <nav class="page-tabs">
          <button type="button" [class.active]="view === 'dashboard'" (click)="setView('dashboard')">
            <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5"><rect x="2" y="2" width="5" height="5" rx="1"/><rect x="9" y="2" width="5" height="5" rx="1"/><rect x="2" y="9" width="5" height="5" rx="1"/><rect x="9" y="9" width="5" height="5" rx="1"/></svg>
            Dashboard
          </button>
          <button type="button" [class.active]="view === 'buy'" (click)="setView('buy')">
            <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5"><circle cx="8" cy="8" r="6"/><path d="M8 5v6"/><path d="M5 8h6"/></svg>
            Buy Policy
          </button>
          <button type="button" [class.active]="view === 'detail'" (click)="selectedPolicy && setView('detail')" [class.disabled]="!selectedPolicy">
            <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M10 2H4a1 1 0 00-1 1v10a1 1 0 001 1h8a1 1 0 001-1V5z"/><path d="M10 2v3h3"/></svg>
            Policy Detail
          </button>
        </nav>
      </header>

      <!-- ═══════════ DASHBOARD VIEW ═══════════ -->
      <div class="view-container" *ngIf="view === 'dashboard'">

        <!-- Welcome hero (zero-state) -->
        <div class="welcome-hero" *ngIf="policies.length === 0">
          <div class="hero-content">
            <span class="hero-emoji"><svg viewBox="0 0 48 48" fill="none" stroke="#5465ff" stroke-width="2"><path d="M24 4L6 12v12c0 11 8 18 18 20 10-2 18-9 18-20V12L24 4z" fill="rgba(84,101,255,0.08)"/><path d="M17 24l4 4 10-10" stroke-linecap="round" stroke-linejoin="round"/></svg></span>
            <h2>Welcome{{ authState.userName ? ', ' + authState.userName.split(' ')[0] : '' }}!</h2>
            <p>Get started by exploring our insurance products and purchasing your first policy.</p>
            <button type="button" class="primary-button" (click)="setView('buy')">
              <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="2"><circle cx="8" cy="8" r="6"/><path d="M8 5v6"/><path d="M5 8h6"/></svg>
              Browse Products
            </button>
          </div>
        </div>

        <!-- Quick stats row -->
        <div class="stats-row">
          <div class="stat-card">
            <div class="stat-icon policies-icon">
              <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M9 2H4a2 2 0 00-2 2v12a2 2 0 002 2h8a2 2 0 002-2V7z"/><path d="M9 2v5h5"/></svg>
            </div>
            <div><span class="stat-label">Active Policies</span><strong class="stat-value">{{ policies.length }}</strong></div>
          </div>
          <div class="stat-card">
            <div class="stat-icon products-icon">
              <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.5"><rect x="2" y="4" width="16" height="12" rx="2"/><path d="M6 4V2h8v2"/></svg>
            </div>
            <div><span class="stat-label">Available Products</span><strong class="stat-value">{{ products.length }}</strong></div>
          </div>
          <div class="stat-card action-stat" (click)="setView('buy')">
            <div class="stat-icon buy-icon">
              <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.5"><circle cx="10" cy="10" r="8"/><path d="M10 6v8"/><path d="M6 10h8"/></svg>
            </div>
            <div><span class="stat-label">Quick Action</span><strong class="stat-value cta-text">Buy Policy →</strong></div>
          </div>
        </div>

        <!-- Product catalog: Type → Subtype -->
        <div class="content-card" *ngIf="products.length > 0">
          <div class="card-header">
            <h2 class="card-title">
              <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.5"><rect x="2" y="4" width="16" height="12" rx="2"/><path d="M6 4V2h8v2"/></svg>
              Available Products
            </h2>
            <button type="button" class="ghost-link" (click)="reload()">↻ Refresh</button>
          </div>

          <!-- Insurance type cards -->
          <div class="type-grid">
            <button type="button" class="type-card" *ngFor="let t of getInsuranceTypes()"
              [class.active]="selectedTypeId === t.typeId" (click)="selectType(t.typeId)">
              <span class="type-icon" [innerHTML]="getTypeIcon(t.typeName) | safeHtml"></span>
              <strong>{{ t.typeName }}</strong>
              <span class="type-count">{{ t.count }} {{ t.count === 1 ? 'plan' : 'plans' }}</span>
            </button>
          </div>

          <!-- Subtypes for selected type -->
          <div class="subtypes-section" *ngIf="selectedTypeId">
            <p class="subtype-label">Choose a plan under <strong>{{ getTypeName(selectedTypeId) }}</strong></p>
            <div class="product-grid">
              <button type="button" class="product-card" *ngFor="let product of getSubtypes(selectedTypeId)" (click)="selectProduct(product)">
                <div class="product-type-badge">{{ product.typeName }}</div>
                <strong class="product-name">{{ product.subTypeName }}</strong>
                <div class="product-premium">
                  <span>Base premium</span>
                  <strong>{{ product.basePremium | formatCurrency }}<small>/mo</small></strong>
                </div>
                <span class="product-cta">Select & Configure →</span>
              </button>
            </div>
          </div>
        </div>

        <!-- My policies list -->
        <div class="content-card">
          <div class="card-header">
            <h2 class="card-title">
              <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M9 2H4a2 2 0 00-2 2v12a2 2 0 002 2h8a2 2 0 002-2V7z"/><path d="M9 2v5h5"/></svg>
              My Policies
            </h2>
            <app-status-badge *ngIf="policies.length > 0" value="ACTIVE"></app-status-badge>
          </div>
          <div class="policy-list" *ngIf="policies.length > 0">
            <article class="policy-row" *ngFor="let policy of policies" (click)="openPolicy(policy)">
              <div class="policy-info">
                <strong>{{ policy.policyNumber }}</strong>
                <span class="policy-type">{{ policy.typeName }} · {{ policy.subTypeName }}</span>
              </div>
              <div class="policy-numbers">
                <span>{{ policy.coverageAmount | formatCurrency }}</span>
                <span class="premium-tag">{{ policy.monthlyPremium | formatCurrency }}/mo</span>
              </div>
              <app-status-badge [value]="policy.status"></app-status-badge>
              <svg class="row-arrow" viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="2"><path d="M6 4l4 4-4 4"/></svg>
            </article>
          </div>
          <div class="empty-state" *ngIf="policies.length === 0">
            <div class="empty-icon"><svg viewBox="0 0 48 48" fill="none" stroke="#788bff" stroke-width="1.5"><rect x="10" y="6" width="28" height="36" rx="3" fill="rgba(84,101,255,0.06)"/><path d="M18 18h12"/><path d="M18 24h12"/><path d="M18 30h8"/></svg></div>
            <h3>No policies yet</h3>
            <p>Purchase your first insurance policy to see it listed here.</p>
            <button type="button" class="secondary-button" (click)="setView('buy')">Browse Products</button>
          </div>
        </div>
      </div>

      <!-- ═══════════ BUY POLICY VIEW ═══════════ -->
      <div class="view-container" *ngIf="view === 'buy'">

        <!-- Stepper -->
        <app-stepper
          [steps]="['Choose Product', 'Configure Coverage', 'Review & Purchase']"
          [activeIndex]="step"
          (stepClick)="goToStep($event)">
        </app-stepper>

        <!-- Step 0: Choose product (Type → Subtype) -->
        <div class="content-card" *ngIf="step === 0">
          <div class="card-header">
            <h2 class="card-title">Step 1: Choose insurance type</h2>
          </div>

          <!-- Type selection -->
          <div class="type-grid" *ngIf="products.length > 0">
            <button type="button" class="type-card" *ngFor="let t of getInsuranceTypes()"
              [class.active]="wizardTypeId === t.typeId" (click)="selectWizardType(t.typeId)">
              <span class="type-icon" [innerHTML]="getTypeIcon(t.typeName) | safeHtml"></span>
              <strong>{{ t.typeName }}</strong>
              <span class="type-count">{{ t.count }} {{ t.count === 1 ? 'plan' : 'plans' }}</span>
            </button>
          </div>

          <!-- Subtype selection -->
          <div class="subtypes-section" *ngIf="wizardTypeId">
            <p class="subtype-label">Select a plan under <strong>{{ getTypeName(wizardTypeId) }}</strong></p>
            <div class="product-grid">
              <button type="button" class="product-card selectable" *ngFor="let product of getSubtypes(wizardTypeId)"
                [class.selected]="purchaseModel.productId === product.productId"
                (click)="selectProductForPurchase(product)">
                <div class="product-type-badge">{{ product.typeName }}</div>
                <strong class="product-name">{{ product.subTypeName }}</strong>
                <div class="product-premium">
                  <span>Base premium</span>
                  <strong>{{ product.basePremium | formatCurrency }}<small>/mo</small></strong>
                </div>
                <div class="selection-check" *ngIf="purchaseModel.productId === product.productId">
                  <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M3 8.5l3 3 7-7"/></svg>
                </div>
              </button>
            </div>
          </div>

          <div class="empty-state" *ngIf="products.length === 0">
            <div class="empty-icon"><svg viewBox="0 0 48 48" fill="none" stroke="#788bff" stroke-width="1.5"><rect x="8" y="14" width="32" height="26" rx="3" fill="rgba(84,101,255,0.06)"/><path d="M8 22h32"/><path d="M20 14V8h8v6"/><circle cx="24" cy="30" r="4"/></svg></div>
            <h3>No products available</h3>
            <p>Insurance products haven't been configured yet. Please check back later.</p>
            <button type="button" class="secondary-button" (click)="reload()">↻ Refresh</button>
          </div>
          <div class="step-actions">
            <div></div>
            <button type="button" class="primary-button" [disabled]="!purchaseModel.productId" (click)="nextStep()">
              Next: Configure Coverage →
            </button>
          </div>
        </div>

        <!-- Step 1: Coverage -->
        <div class="content-card" *ngIf="step === 1">
          <div class="card-header">
            <h2 class="card-title">Configure your coverage</h2>
            <span class="selected-product-tag" *ngIf="getSelectedProduct() as prod">{{ prod.typeName }} · {{ prod.subTypeName }}</span>
          </div>
          <div class="form-grid">
            <label class="form-field">
              <span class="field-label">Coverage Amount (₹)</span>
              <input name="coverageAmount" [(ngModel)]="purchaseModel.coverageAmount" type="number" min="10000" step="10000" placeholder="e.g. 500000" />
              <span class="field-hint">Maximum amount payable under this policy</span>
            </label>
            <label class="form-field">
              <span class="field-label">Term (months)</span>
              <input name="termMonths" [(ngModel)]="purchaseModel.termMonths" type="number" min="1" max="120" placeholder="e.g. 12" />
              <span class="field-hint">Duration of your policy coverage</span>
            </label>
            <label class="form-field full-width">
              <span class="field-label">Coverage Start Date</span>
              <input name="insuranceDate" [(ngModel)]="purchaseModel.insuranceDate" type="date" />
              <span class="field-hint">When you'd like coverage to begin</span>
            </label>
          </div>
          <div class="step-actions">
            <button type="button" class="secondary-button" (click)="prevStep()">← Back</button>
            <button type="button" class="primary-button" (click)="calculateAndNext()" [disabled]="loading || !purchaseModel.coverageAmount || !purchaseModel.termMonths">
              {{ loading ? 'Calculating...' : 'Calculate Premium & Review →' }}
            </button>
          </div>
        </div>

        <!-- Step 2: Review & Purchase -->
        <div class="content-card" *ngIf="step === 2">
          <div class="card-header">
            <h2 class="card-title">Review & Confirm Purchase</h2>
          </div>

          <div class="review-grid" *ngIf="quote">
            <div class="review-item"><span>Product</span><strong>{{ getSelectedProduct()?.typeName }} · {{ getSelectedProduct()?.subTypeName }}</strong></div>
            <div class="review-item"><span>Coverage Amount</span><strong>{{ purchaseModel.coverageAmount | formatCurrency }}</strong></div>
            <div class="review-item"><span>Term</span><strong>{{ purchaseModel.termMonths }} months</strong></div>
            <div class="review-item"><span>Start Date</span><strong>{{ purchaseModel.insuranceDate }}</strong></div>
            <div class="review-item highlight">
              <span>Monthly Premium</span>
              <strong class="premium-amount">{{ quote.monthlyPremium | formatCurrency }}<small>/month</small></strong>
            </div>
          </div>

          <p class="hint" *ngIf="message">{{ message }}</p>

          <div class="step-actions">
            <button type="button" class="secondary-button" (click)="prevStep()">← Back</button>
            <button type="button" class="confirm-button" (click)="purchasePolicy()" [disabled]="loading">
              <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 8.5l3 3 7-7"/></svg>
              {{ loading ? 'Processing...' : 'Confirm Purchase' }}
            </button>
          </div>
        </div>
      </div>

      <!-- ═══════════ POLICY DETAIL VIEW ═══════════ -->
      <div class="view-container" *ngIf="view === 'detail'">
        <div class="content-card" *ngIf="selectedPolicy; else noPolicy">
          <div class="card-header">
            <h2 class="card-title">{{ selectedPolicy.policyNumber }}</h2>
            <app-status-badge [value]="selectedPolicy.status"></app-status-badge>
          </div>

          <div class="review-grid">
            <div class="review-item"><span>Product</span><strong>{{ selectedPolicy.typeName }} · {{ selectedPolicy.subTypeName }}</strong></div>
            <div class="review-item"><span>Coverage</span><strong>{{ selectedPolicy.coverageAmount | formatCurrency }}</strong></div>
            <div class="review-item"><span>Monthly Premium</span><strong>{{ selectedPolicy.monthlyPremium | formatCurrency }}/mo</strong></div>
            <div class="review-item"><span>Coverage Period</span><strong>{{ selectedPolicy.insuranceDate | date:'mediumDate' }} — {{ selectedPolicy.endDate | date:'mediumDate' }}</strong></div>
            <div class="review-item"><span>Issued</span><strong>{{ selectedPolicy.issuedDate | date:'mediumDate' }}</strong></div>
            <div class="review-item"><span>Last Updated</span><strong>{{ selectedPolicy.updatedAt | date:'medium' }}</strong></div>
          </div>

          <div class="step-actions">
            <button type="button" class="secondary-button" (click)="setView('dashboard')">← Back to Dashboard</button>
            <button type="button" class="danger-button" (click)="cancelPolicy()" [disabled]="loading || selectedPolicy.status === 'CANCELLED'">
              {{ selectedPolicy.status === 'CANCELLED' ? 'Already Cancelled' : 'Cancel Policy' }}
            </button>
          </div>
          <p class="hint" *ngIf="message">{{ message }}</p>
        </div>
        <ng-template #noPolicy>
          <div class="content-card empty-state">
            <div class="empty-icon"><svg viewBox="0 0 48 48" fill="none" stroke="#788bff" stroke-width="1.5"><path d="M28 6H14a4 4 0 00-4 4v28a4 4 0 004 4h20a4 4 0 004-4V16L28 6z" fill="rgba(84,101,255,0.06)"/><path d="M28 6v10h10"/><path d="M20 26h8"/><path d="M20 32h4"/></svg></div>
            <h3>No policy selected</h3>
            <p>Select a policy from your dashboard to view its details.</p>
            <button type="button" class="secondary-button" (click)="setView('dashboard')">Go to Dashboard</button>
          </div>
        </ng-template>
      </div>
    </section>
  `,
  styles: [`
    :host { display: block; animation: fadeInUp 0.4s ease; }

    /* ── Page Shell ── */
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

    /* ── View Container ── */
    .view-container { display: grid; gap: 1.25rem; }

    /* ── Welcome Hero ── */
    .welcome-hero {
      border-radius: var(--radius-lg, 20px); padding: 2.5rem;
      background: linear-gradient(135deg, rgba(84, 101, 255, 0.07) 0%, rgba(155, 177, 255, 0.12) 50%, rgba(226, 253, 255, 0.2) 100%);
      border: 1px solid rgba(84, 101, 255, 0.1);
      text-align: center;
    }
    .hero-content { max-width: 440px; margin: 0 auto; }
    .hero-emoji { display: block; margin-bottom: 0.75rem; width: 3.5rem; height: 3.5rem; color: var(--primary, #5465ff); }
    .hero-emoji svg { width: 100%; height: 100%; }
    .welcome-hero h2 { font-family: 'Space Grotesk', sans-serif; font-size: 1.6rem; font-weight: 700; color: var(--ink); margin: 0 0 0.5rem; }
    .welcome-hero p { color: var(--muted); font-size: 0.95rem; line-height: 1.6; margin: 0 0 1.25rem; }
    .welcome-hero .primary-button { display: inline-flex; align-items: center; gap: 0.5rem; }
    .welcome-hero .primary-button svg { width: 16px; height: 16px; }

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
    .policies-icon { background: rgba(84, 101, 255, 0.08); color: #5465ff; }
    .products-icon { background: rgba(16, 185, 129, 0.08); color: #059669; }
    .buy-icon { background: rgba(245, 158, 11, 0.08); color: #d97706; }
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

    /* ── Product Grid ── */
    .product-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 0.85rem; }
    .product-card {
      all: unset; cursor: pointer; display: grid; gap: 0.5rem;
      border-radius: var(--radius, 14px); padding: 1.15rem;
      border: 1.5px solid rgba(84, 101, 255, 0.1);
      background: rgba(255, 255, 255, 0.75);
      transition: all 0.2s ease; position: relative;
    }
    .product-card:hover { border-color: rgba(84, 101, 255, 0.25); box-shadow: var(--shadow, 0 4px 12px rgba(0,0,0,0.06)); transform: translateY(-2px); }
    .product-card.selected { border-color: var(--primary, #5465ff); background: rgba(84, 101, 255, 0.04); box-shadow: 0 4px 16px rgba(84, 101, 255, 0.15); }
    .product-type-badge { display: inline-block; background: rgba(84, 101, 255, 0.08); color: #5465ff; font-size: 0.7rem; font-weight: 700; padding: 0.25rem 0.6rem; border-radius: var(--radius-full, 999px); text-transform: uppercase; letter-spacing: 0.06em; }
    .product-name { font-size: 1rem; color: var(--ink); }
    .product-premium { display: grid; gap: 0.15rem; margin-top: 0.3rem; }
    .product-premium span { font-size: 0.75rem; color: var(--muted); font-weight: 500; }
    .product-premium strong { font-size: 1.1rem; color: var(--primary, #5465ff); font-family: 'Space Grotesk', sans-serif; }
    .product-premium small { font-size: 0.75rem; color: var(--muted); font-weight: 400; }
    .product-cta { font-size: 0.8rem; color: var(--primary, #5465ff); font-weight: 600; opacity: 0; transition: opacity 0.2s; }
    .product-card:hover .product-cta { opacity: 1; }
    .selection-check { position: absolute; top: 0.75rem; right: 0.75rem; width: 1.5rem; height: 1.5rem; background: #059669; border-radius: 999px; display: grid; place-items: center; }
    .selection-check svg { width: 0.9rem; height: 0.9rem; color: white; }
    .selected-product-tag { background: rgba(84, 101, 255, 0.08); color: #5465ff; font-size: 0.78rem; font-weight: 600; padding: 0.35rem 0.75rem; border-radius: var(--radius-full, 999px); }

    /* ── Type Grid (hierarchical selection) ── */
    .type-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(160px, 1fr)); gap: 0.75rem; margin-bottom: 0.5rem; }
    .type-card {
      all: unset; cursor: pointer; display: grid; gap: 0.4rem; text-align: center;
      border-radius: var(--radius, 14px); padding: 1.25rem 1rem;
      border: 1.5px solid rgba(84, 101, 255, 0.1); background: rgba(255, 255, 255, 0.75);
      transition: all 0.25s ease;
    }
    .type-card:hover { border-color: rgba(84, 101, 255, 0.25); transform: translateY(-2px); box-shadow: var(--shadow, 0 4px 12px rgba(0,0,0,0.06)); }
    .type-card.active { border-color: var(--primary, #5465ff); background: rgba(84, 101, 255, 0.04); box-shadow: 0 4px 18px rgba(84, 101, 255, 0.15); }
    .type-icon { display: flex; justify-content: center; align-items: center; width: 2.5rem; height: 2.5rem; margin: 0 auto 0.2rem; color: var(--primary, #5465ff); }
    .type-icon svg { width: 100%; height: 100%; }
    .type-card strong { font-size: 0.92rem; color: var(--ink); }
    .type-card .type-count { font-size: 0.75rem; color: var(--muted); font-weight: 500; }

    .subtypes-section { animation: fadeInUp 0.3s ease; margin-top: 0.75rem; }
    .subtype-label { font-size: 0.88rem; color: var(--muted); margin: 0 0 0.75rem; }
    .subtype-label strong { color: var(--primary, #5465ff); }

    /* ── Policy List ── */
    .policy-list { display: grid; gap: 0.6rem; }
    .policy-row {
      display: grid; grid-template-columns: 1fr auto auto 24px; align-items: center; gap: 1rem;
      border-radius: var(--radius, 14px); padding: 1rem 1.15rem;
      border: 1px solid rgba(84, 101, 255, 0.08); background: rgba(255, 255, 255, 0.7);
      cursor: pointer; transition: all 0.2s ease;
    }
    .policy-row:hover { border-color: rgba(84, 101, 255, 0.2); box-shadow: var(--shadow-sm, 0 2px 6px rgba(0,0,0,0.04)); transform: translateX(4px); }
    .policy-info strong { display: block; font-size: 0.9rem; color: var(--ink); }
    .policy-type { display: block; font-size: 0.78rem; color: var(--muted); margin-top: 2px; }
    .policy-numbers { text-align: right; }
    .policy-numbers span { display: block; font-size: 0.85rem; color: var(--ink); font-weight: 600; }
    .premium-tag { font-size: 0.75rem !important; color: var(--muted) !important; font-weight: 400 !important; }
    .row-arrow { width: 16px; height: 16px; color: var(--muted); transition: transform 0.2s; }
    .policy-row:hover .row-arrow { transform: translateX(3px); color: var(--primary); }

    /* ── Forms ── */
    .form-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 1rem; }
    .form-field { display: grid; gap: 0.3rem; }
    .form-field.full-width { grid-column: 1 / -1; }
    .field-label { font-weight: 600; color: var(--ink); font-size: 0.88rem; }
    .field-hint { font-size: 0.75rem; color: var(--muted); }
    input, select, textarea {
      border: 1.5px solid rgba(84, 101, 255, 0.14); background: rgba(255, 255, 255, 0.8);
      border-radius: var(--radius-sm, 10px); padding: 0.82rem 1rem; color: var(--ink); font-size: 0.92rem;
      transition: all 0.2s ease;
    }
    input:focus, select:focus, textarea:focus { border-color: var(--primary-2, #788bff); background: white; box-shadow: 0 0 0 4px rgba(84, 101, 255, 0.1); outline: none; }

    /* ── Review Grid ── */
    .review-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 0.75rem; margin-bottom: 1.25rem; }
    .review-item {
      display: grid; gap: 0.3rem; padding: 1rem;
      border-radius: var(--radius-sm, 10px); background: rgba(255, 255, 255, 0.75);
      border: 1px solid rgba(84, 101, 255, 0.06);
    }
    .review-item span { font-size: 0.78rem; color: var(--muted); font-weight: 500; text-transform: uppercase; letter-spacing: 0.04em; }
    .review-item strong { font-size: 0.95rem; color: var(--ink); }
    .review-item.highlight { background: linear-gradient(135deg, rgba(84, 101, 255, 0.06) 0%, rgba(226, 253, 255, 0.25) 100%); border-color: rgba(84, 101, 255, 0.12); grid-column: 1 / -1; }
    .premium-amount { font-size: 1.4rem !important; font-family: 'Space Grotesk', sans-serif; color: var(--primary, #5465ff) !important; }
    .premium-amount small { font-size: 0.85rem; color: var(--muted); font-weight: 400; }

    /* ── Step Actions ── */
    .step-actions { display: flex; justify-content: space-between; align-items: center; gap: 1rem; margin-top: 1.25rem; padding-top: 1.25rem; border-top: 1px solid rgba(84, 101, 255, 0.06); }

    /* ── Buttons ── */
    .primary-button, .secondary-button, .confirm-button, .danger-button {
      border: 0; border-radius: var(--radius-sm, 10px); padding: 0.82rem 1.5rem;
      font-weight: 700; font-size: 0.88rem; cursor: pointer; transition: all 0.2s ease;
      display: inline-flex; align-items: center; gap: 0.4rem;
    }
    .primary-button { color: white; background: linear-gradient(135deg, #5465ff 0%, #788bff 100%); box-shadow: 0 4px 14px rgba(84, 101, 255, 0.25); }
    .primary-button:hover:not(:disabled) { transform: translateY(-1px); box-shadow: 0 6px 20px rgba(84, 101, 255, 0.35); }
    .primary-button:disabled, .confirm-button:disabled { opacity: 0.55; cursor: not-allowed; }
    .secondary-button { background: rgba(84, 101, 255, 0.07); color: var(--primary, #5465ff); }
    .secondary-button:hover { background: rgba(84, 101, 255, 0.12); }
    .confirm-button {
      color: white; background: linear-gradient(135deg, #059669, #10b981);
      box-shadow: 0 4px 14px rgba(5, 150, 105, 0.25);
    }
    .confirm-button:hover:not(:disabled) { transform: translateY(-1px); box-shadow: 0 6px 20px rgba(5, 150, 105, 0.35); }
    .confirm-button svg { width: 16px; height: 16px; }
    .danger-button { background: rgba(239, 68, 68, 0.07); color: #dc2626; border: 1px solid rgba(239, 68, 68, 0.12); }
    .danger-button:hover:not(:disabled) { background: rgba(239, 68, 68, 0.12); }
    .danger-button:disabled { opacity: 0.45; cursor: not-allowed; }

    .ghost-link { background: transparent; border: 0; color: var(--primary, #5465ff); cursor: pointer; font-weight: 700; font-size: 0.82rem; padding: 0.3rem 0; transition: color 0.2s; }
    .ghost-link:hover { color: #4354e6; }
    .hint { color: var(--muted); font-size: 0.85rem; margin-top: 0.5rem; }

    /* ── Empty State ── */
    .empty-state { text-align: center; padding: 2.5rem 1.5rem; }
    .empty-icon { width: 3rem; height: 3rem; margin: 0 auto 0.5rem; color: var(--primary-2, #788bff); }
    .empty-icon svg { width: 100%; height: 100%; }
    .empty-state h3 { margin: 0 0 0.3rem; font-family: 'Space Grotesk', sans-serif; font-weight: 700; color: var(--ink); }
    .empty-state p { color: var(--muted); font-size: 0.9rem; margin: 0 0 1rem; }

    @media (max-width: 1100px) { .stats-row { grid-template-columns: 1fr 1fr; } }
    @media (max-width: 768px) {
      .page-header { flex-direction: column; align-items: flex-start; }
      .stats-row, .product-grid, .review-grid, .form-grid { grid-template-columns: 1fr; }
      .review-item.highlight { grid-column: 1; }
      .policy-row { grid-template-columns: 1fr auto; }
      .policy-numbers { display: none; }
    }
  `]

})
export class CustomerConsoleComponent implements OnInit {
  view: 'dashboard' | 'buy' | 'detail' = 'dashboard';
  step = 0;
  loading = false;
  message = '';
  products: InsuranceProductDto[] = [];
  policies: PolicyDto[] = [];
  selectedTypeId: number | null = null;
  wizardTypeId: number | null = null;
  selectedPolicy: PolicyDto | null = null;
  quote: PremiumCalculationDto | null = null;

  purchaseModel = {
    productId: 0,
    coverageAmount: 250000,
    termMonths: 12,
    insuranceDate: new Date().toISOString().slice(0, 10)
  };

  constructor(
    private readonly policyService: PolicyService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    public readonly authState: AuthStateService
  ) {}

  ngOnInit(): void {
    this.syncRoute();
    this.loadData();
    this.route.params.subscribe((params) => {
      const policyId = params['id'] as string | undefined;
      if (policyId) {
        this.loadPolicy(policyId);
      }
      this.syncRoute();
    });
  }

  setView(view: 'dashboard' | 'buy' | 'detail'): void {
    this.view = view;
    this.message = '';
    if (view === 'buy') {
      this.step = 0;
      this.quote = null;
      void this.router.navigateByUrl('/customer/buy-policy');
    } else if (view === 'dashboard') {
      void this.router.navigateByUrl('/customer/dashboard');
    } else if (this.selectedPolicy) {
      void this.router.navigateByUrl(`/customer/policy/${this.selectedPolicy.policyId}`);
    }
  }

  reload(): void {
    this.loadData();
  }

  // ── Wizard Navigation ──
  nextStep(): void {
    if (this.step < 2) {
      this.step++;
    }
  }

  prevStep(): void {
    if (this.step > 0) {
      this.step--;
    }
  }

  goToStep(index: number): void {
    if (index < this.step) {
      this.step = index;
    }
  }

  selectProduct(product: InsuranceProductDto): void {
    this.purchaseModel.productId = product.productId;
    this.wizardTypeId = product.typeId;
    this.step = 0;
    this.view = 'buy';
    void this.router.navigateByUrl('/customer/buy-policy');
  }

  selectProductForPurchase(product: InsuranceProductDto): void {
    this.purchaseModel.productId = product.productId;
  }

  selectType(typeId: number): void {
    this.selectedTypeId = this.selectedTypeId === typeId ? null : typeId;
  }

  selectWizardType(typeId: number): void {
    this.wizardTypeId = typeId;
    this.purchaseModel.productId = 0;
  }

  getInsuranceTypes(): { typeId: number; typeName: string; count: number }[] {
    const map = new Map<number, { typeId: number; typeName: string; count: number }>();
    for (const p of this.products) {
      const existing = map.get(p.typeId);
      if (existing) {
        existing.count++;
      } else {
        map.set(p.typeId, { typeId: p.typeId, typeName: p.typeName, count: 1 });
      }
    }
    return Array.from(map.values());
  }

  getSubtypes(typeId: number): InsuranceProductDto[] {
    return this.products.filter(p => p.typeId === typeId);
  }

  getTypeName(typeId: number): string {
    return this.products.find(p => p.typeId === typeId)?.typeName ?? '';
  }

  getTypeIcon(typeName: string): string {
    const name = typeName.toLowerCase();
    const s = 'viewBox="0 0 32 32" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"';
    if (name.includes('health')) return `<svg ${s}><path d="M16 28s-10-6.5-10-14a6 6 0 0112-1 6 6 0 0112 1c0 7.5-10 14-10 14z" fill="rgba(84,101,255,0.08)"/><path d="M13 15h6"/><path d="M16 12v6"/></svg>`;
    if (name.includes('life')) return `<svg ${s}><path d="M16 28s-10-6.5-10-14a6 6 0 0112-1 6 6 0 0112 1c0 7.5-10 14-10 14z" fill="rgba(220,38,38,0.08)" stroke="#ef4444"/></svg>`;
    if (name.includes('auto') || name.includes('vehicle') || name.includes('motor')) return `<svg ${s}><rect x="4" y="14" width="24" height="10" rx="3" fill="rgba(84,101,255,0.08)"/><circle cx="10" cy="24" r="3"/><circle cx="22" cy="24" r="3"/><path d="M7 14l3-6h12l3 6"/><path d="M4 18h2"/><path d="M26 18h2"/></svg>`;
    if (name.includes('home') || name.includes('property')) return `<svg ${s}><path d="M4 16L16 6l12 10" fill="rgba(84,101,255,0.08)"/><rect x="8" y="16" width="16" height="12" rx="1" fill="rgba(84,101,255,0.08)"/><rect x="13" y="21" width="6" height="7"/><rect x="11" y="18" width="4" height="3" rx="0.5"/><rect x="17" y="18" width="4" height="3" rx="0.5"/></svg>`;
    if (name.includes('travel')) return `<svg ${s}><circle cx="16" cy="16" r="12" fill="rgba(84,101,255,0.08)"/><ellipse cx="16" cy="16" rx="5" ry="12"/><path d="M4 16h24"/><path d="M6 10h20"/><path d="M6 22h20"/></svg>`;
    if (name.includes('business') || name.includes('commercial')) return `<svg ${s}><rect x="6" y="10" width="20" height="18" rx="2" fill="rgba(84,101,255,0.08)"/><path d="M6 16h20"/><rect x="10" y="4" width="12" height="6" rx="1"/><rect x="12" y="20" width="3" height="3"/><rect x="17" y="20" width="3" height="3"/></svg>`;
    return `<svg ${s}><path d="M16 4L6 10v8c0 8 5 14 10 16 5-2 10-8 10-16v-8L16 4z" fill="rgba(84,101,255,0.08)"/><path d="M12 16l3 3 6-6"/></svg>`;
  }

  getSelectedProduct(): InsuranceProductDto | undefined {
    return this.products.find(p => p.productId === this.purchaseModel.productId);
  }

  calculateAndNext(): void {
    if (!this.purchaseModel.productId) {
      return;
    }
    this.loading = true;
    this.policyService.calculatePremium(this.purchaseModel.productId, this.purchaseModel.coverageAmount, this.purchaseModel.termMonths).subscribe({
      next: (quote) => {
        this.quote = quote;
        this.step = 2;
      },
      error: () => {
        this.message = 'Unable to calculate premium. Please check your inputs.';
        this.loading = false;
      },
      complete: () => {
        this.loading = false;
      }
    });
  }

  purchasePolicy(): void {
    this.loading = true;
    this.message = '';

    const dto = {
      productId: this.purchaseModel.productId,
      coverageAmount: this.purchaseModel.coverageAmount,
      termMonths: this.purchaseModel.termMonths,
      insuranceDate: this.purchaseModel.insuranceDate
    };

    // Step 1 — create Razorpay order on the backend
    this.policyService.createPaymentOrder(dto).subscribe({
      next: (order) => {
        this.loading = false;
        this.openRazorpayCheckout(order);
      },
      error: (err) => {
        this.message = this.resolveError(err);
        this.loading = false;
      }
    });
  }

  private openRazorpayCheckout(order: import('../../models/api-models').PaymentOrderResponseDto): void {
    const product = this.getSelectedProduct();
    const userName = this.authState.userName ?? '';

    const options = {
      key: order.razorpayKeyId,
      amount: order.amountPaise,
      currency: order.currency,
      name: 'SmartSure',
      description: product ? `${product.typeName} · ${product.subTypeName}` : 'Insurance Policy',
      order_id: order.razorpayOrderId,
      theme: { color: '#5465FF' },
      handler: (response: { razorpay_order_id: string; razorpay_payment_id: string; razorpay_signature: string }) => {
        // Step 2 — verify signature and activate policy
        this.loading = true;
        this.message = 'Verifying payment…';

        this.policyService.verifyPayment({
          pendingOrderToken: order.pendingOrderToken,
          razorpayOrderId: response.razorpay_order_id,
          razorpayPaymentId: response.razorpay_payment_id,
          razorpaySignature: response.razorpay_signature
        }).subscribe({
          next: (policy) => {
            this.loading = false;
            this.message = '';
            this.selectedPolicy = policy;
            this.view = 'detail';
            void this.router.navigateByUrl(`/customer/policy/${policy.policyId}`);
            this.loadData();
          },
          error: (err) => {
            this.message = this.resolveError(err);
            this.loading = false;
          }
        });
      },
      modal: {
        ondismiss: () => {
          this.message = 'Payment cancelled. You can try again.';
        }
      },
      prefill: { name: userName }
    };

    // Load Razorpay script lazily if not already present
    const existingScript = document.getElementById('razorpay-script');
    if (existingScript) {
      this.launchRazorpay(options);
      return;
    }

    const script = document.createElement('script');
    script.id = 'razorpay-script';
    script.src = 'https://checkout.razorpay.com/v1/checkout.js';
    script.onload = () => this.launchRazorpay(options);
    script.onerror = () => {
      this.message = 'Unable to load payment gateway. Please check your connection and try again.';
    };
    document.body.appendChild(script);
  }

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  private launchRazorpay(options: Record<string, any>): void {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const Razorpay = (window as any)['Razorpay'];
    if (!Razorpay) {
      this.message = 'Payment gateway failed to load. Please refresh and try again.';
      return;
    }
    const rzp = new Razorpay(options);
    rzp.on('payment.failed', (response: { error: { description: string } }) => {
      this.message = `Payment failed: ${response.error.description}`;
    });
    rzp.open();
  }

  private resolveError(error: unknown): string {
    const r = error as { error?: { detail?: string; title?: string }; message?: string };
    return r?.error?.detail ?? r?.error?.title ?? r?.message ?? 'Something went wrong. Please try again.';
  }

  openPolicy(policy: PolicyDto): void {
    this.selectedPolicy = policy;
    this.view = 'detail';
    void this.router.navigateByUrl(`/customer/policy/${policy.policyId}`);
  }

  cancelPolicy(): void {
    if (!this.selectedPolicy) {
      return;
    }
    this.loading = true;
    this.policyService.cancelPolicy(this.selectedPolicy.policyId).subscribe({
      next: (policy) => {
        this.selectedPolicy = policy;
        this.message = `Policy ${policy.policyNumber} has been cancelled.`;
        this.loadData();
      },
      error: () => {
        this.message = 'Unable to cancel policy.';
        this.loading = false;
      },
      complete: () => {
        this.loading = false;
      }
    });
  }

  private loadData(): void {
    forkJoin({
      products: this.policyService.getProducts(),
      policies: this.policyService.getMyPolicies()
    }).subscribe({
      next: ({ products, policies }) => {
        this.products = products;
        this.policies = policies;
      }
    });
  }

  private loadPolicy(id: string): void {
    this.policyService.getPolicy(id).subscribe({
      next: (policy) => {
        this.selectedPolicy = policy;
        this.view = 'detail';
      }
    });
  }

  private syncRoute(): void {
    const url = this.router.url;
    if (url.includes('/buy-policy')) {
      this.view = 'buy';
      return;
    }
    if (url.includes('/policy/')) {
      this.view = 'detail';
      return;
    }
    this.view = 'dashboard';
  }
}
