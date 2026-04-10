import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  InsuranceProductDto,
  PolicyDto,
  PremiumCalculationDto,
  PurchasePolicyDto
} from '../../models/api-models';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class PolicyService {
  constructor(private readonly api: ApiService) {}

  getProducts(): Observable<InsuranceProductDto[]> {
    return this.api.get<InsuranceProductDto[]>('/policies/products');
  }

  getProduct(id: number): Observable<InsuranceProductDto> {
    return this.api.get<InsuranceProductDto>(`/policies/products/${id}`);
  }

  calculatePremium(productId: number, coverageAmount: number, termMonths: number): Observable<PremiumCalculationDto> {
    return this.api.get<PremiumCalculationDto>('/policies/calculate-premium', { productId, coverageAmount, termMonths });
  }

  purchasePolicy(dto: PurchasePolicyDto): Observable<PolicyDto> {
    return this.api.post<PolicyDto>('/policies/purchase', dto);
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
