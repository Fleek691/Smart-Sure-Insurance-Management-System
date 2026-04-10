import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  ApproveClaimDto,
  ClaimDocumentDto,
  ClaimDto,
  CreateClaimDto,
  RejectClaimDto,
  ReviewClaimDto,
  UploadClaimDocumentDto
} from '../../models/api-models';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class ClaimsService {
  constructor(private readonly api: ApiService) {}

  createClaim(dto: CreateClaimDto): Observable<ClaimDto> {
    return this.api.post<ClaimDto>('/claims', dto);
  }

  getMyClaims(): Observable<ClaimDto[]> {
    return this.api.get<ClaimDto[]>('/claims/my-claims');
  }

  getClaim(id: string): Observable<ClaimDto> {
    return this.api.get<ClaimDto>(`/claims/${id}`);
  }

  submitClaim(id: string): Observable<ClaimDto> {
    return this.api.post<ClaimDto>(`/claims/${id}/submit`);
  }

  uploadDocument(id: string, dto: UploadClaimDocumentDto): Observable<ClaimDocumentDto> {
    return this.api.post<ClaimDocumentDto>(`/claims/${id}/documents`, dto);
  }

  getDocuments(id: string): Observable<ClaimDocumentDto[]> {
    return this.api.get<ClaimDocumentDto[]>(`/claims/${id}/documents`);
  }

  deleteDocument(claimId: string, docId: string): Observable<void> {
    return this.api.delete<void>(`/claims/${claimId}/documents/${docId}`);
  }

  getAllClaimsForAdmin(): Observable<ClaimDto[]> {
    return this.api.get<ClaimDto[]>('/claims/admin/all');
  }

  markUnderReview(id: string, dto: ReviewClaimDto): Observable<ClaimDto> {
    return this.api.put<ClaimDto>(`/claims/admin/${id}/review`, dto);
  }

  approveClaim(id: string, dto: ApproveClaimDto): Observable<ClaimDto> {
    return this.api.put<ClaimDto>(`/claims/admin/${id}/approve`, dto);
  }

  rejectClaim(id: string, dto: RejectClaimDto): Observable<ClaimDto> {
    return this.api.put<ClaimDto>(`/claims/admin/${id}/reject`, dto);
  }
}
