import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AuditLogDto,
  ClaimsReportDto,
  DashboardStatsDto,
  PagedResultDto,
  PolicyReportDto,
  RevenueReportDto
} from '../../models/api-models';
import { ApiService } from './api.service';

/** Wraps admin dashboard, reports, and audit log API calls via the gateway. */
@Injectable({ providedIn: 'root' })
export class AdminService {
  constructor(private readonly api: ApiService) {}

  getDashboardStats(): Observable<DashboardStatsDto> {
    return this.api.get<DashboardStatsDto>('/admin/dashboard/stats');
  }

  getPolicyReport(from?: string, to?: string, type?: string): Observable<PolicyReportDto> {
    return this.api.get<PolicyReportDto>('/admin/reports/policies', { from, to, type });
  }

  getClaimsReport(from?: string, to?: string, status?: string): Observable<ClaimsReportDto> {
    return this.api.get<ClaimsReportDto>('/admin/reports/claims', { from, to, status });
  }

  getRevenueReport(from?: string, to?: string): Observable<RevenueReportDto> {
    return this.api.get<RevenueReportDto>('/admin/reports/revenue', { from, to });
  }

  exportReport(reportId: string): Observable<Blob> {
    return this.api.download('/admin/reports/export', { reportId });
  }

  getAuditLogs(from?: string, to?: string, action?: string, entityType?: string, page = 1, pageSize = 20): Observable<PagedResultDto<AuditLogDto>> {
    return this.api.get<PagedResultDto<AuditLogDto>>('/admin/audit-logs', { from, to, action, entityType, page, pageSize });
  }
}
