import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CreateInsuranceProductDto,
  InsuranceProductDto,
  PaymentOrderResponseDto,
  PolicyDto,
  PremiumCalculationDto,
  PurchasePolicyDto,
  UpdateInsuranceProductDto,
  VerifyPaymentDto
} from '../../models/api-models';
import { ApiService } from './api.service';

/** Wraps policy and insurance product API calls via the gateway. */
@Injectable({ providedIn: 'root' })
export class PolicyService {
  constructor(private readonly api: ApiService) {}

  /** Returns all available insurance products. */
  getProducts(): Observable<InsuranceProductDto[]> {
    return this.api.get<InsuranceProductDto[]>('/policies/products');
  }

  getProduct(id: number): Observable<InsuranceProductDto> {
    return this.api.get<InsuranceProductDto>(`/policies/products/${id}`);
  }

  createProduct(dto: CreateInsuranceProductDto): Observable<InsuranceProductDto> {
    return this.api.post<InsuranceProductDto>('/policies/products', dto);
  }

  updateProduct(id: number, dto: UpdateInsuranceProductDto): Observable<InsuranceProductDto> {
    return this.api.put<InsuranceProductDto>(`/policies/products/${id}`, dto);
  }

  deleteProduct(id: number): Observable<void> {
    return this.api.delete<void>(`/policies/products/${id}`);
  }

  /** Calculates the monthly premium for a product, coverage, and term. */
  calculatePremium(productId: number, coverageAmount: number, termMonths: number): Observable<PremiumCalculationDto> {
    return this.api.get<PremiumCalculationDto>('/policies/calculate-premium', { productId, coverageAmount, termMonths });
  }

  /** Creates a policy directly (non-Razorpay flow). */
  purchasePolicy(dto: PurchasePolicyDto): Observable<PolicyDto> {
    return this.api.post<PolicyDto>('/policies/purchase', dto);
  }

  /** Creates a Razorpay order for payment processing. */
  createPaymentOrder(dto: PurchasePolicyDto): Observable<PaymentOrderResponseDto> {
    return this.api.post<PaymentOrderResponseDto>('/policies/payment/create-order', dto);
  }

  /** Verifies Razorpay payment signature and activates the policy. */
  verifyPayment(dto: VerifyPaymentDto): Observable<PolicyDto> {
    return this.api.post<PolicyDto>('/policies/payment/verify', dto);
  }

  getMyPolicies(): Observable<PolicyDto[]> {
    return this.api.get<PolicyDto[]>('/policies/my-policies');
  }

  getPolicy(id: string): Observable<PolicyDto> {
    return this.api.get<PolicyDto>(`/policies/${id}`);
  }

  cancelPolicy(id: string, note?: string): Observable<PolicyDto> {
    return this.api.post<PolicyDto>(`/policies/${id}/cancel`, { note });
  }

  getAllPoliciesForAdmin(): Observable<PolicyDto[]> {
    return this.api.get<PolicyDto[]>('/policies/admin/all');
  }

  updatePolicyStatus(id: string, status: string): Observable<PolicyDto> {
    return this.api.put<PolicyDto>(`/policies/admin/${id}/status`, { status });
  }
}
