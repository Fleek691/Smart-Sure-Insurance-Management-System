export interface OtpDispatchResponseDto {
  message?: string;
  success?: boolean;
}

export interface AuthResponseDto {
  userId: string;
  fullName: string;
  email: string;
  token: string;
  refreshToken: string;
  expiresAtUtc: string;
  roles: string[];
}

export interface RegisterDto {
  fullName: string;
  email: string;
  password: string;
  phoneNumber?: string;
  address?: string;
}

export interface VerifyRegistrationOtpDto {
  email: string;
  otp: string;
}

export interface ResendRegistrationOtpDto {
  email: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface ForgotPasswordDto {
  email: string;
}

export interface ResetPasswordDto {
  email: string;
  token: string;
  newPassword: string;
}

export interface ProfileDto {
  userId: string;
  fullName: string;
  email: string;
  phoneNumber?: string | null;
  address?: string | null;
  createdAt?: string;
  lastLoginAt?: string | null;
  isActive?: boolean;
  roles?: string[];
}

export interface UpdateProfileDto {
  fullName: string;
  phoneNumber?: string;
  address?: string;
}

export interface InsuranceProductDto {
  productId: number;
  typeId: number;
  typeName: string;
  subTypeName: string;
  basePremium: number;
}

export interface PremiumCalculationDto {
  productId: number;
  coverageAmount: number;
  termMonths: number;
  monthlyPremium: number;
}

export interface PurchasePolicyDto {
  productId: number;
  coverageAmount: number;
  termMonths: number;
  insuranceDate: string;
}

export interface PaymentOrderResponseDto {
  razorpayOrderId: string;
  amountPaise: number;
  currency: string;
  razorpayKeyId: string;
  monthlyPremium: number;
  pendingOrderToken: string;
}

export interface VerifyPaymentDto {
  pendingOrderToken: string;
  razorpayOrderId: string;
  razorpayPaymentId: string;
  razorpaySignature: string;
}

export interface PolicyDto {
  policyId: string;
  policyNumber: string;
  userId: string;
  typeId: number;
  subTypeId: number;
  typeName: string;
  subTypeName: string;
  issuedDate: string;
  insuranceDate: string;
  coverageAmount: number;
  monthlyPremium: number;
  endDate: string;
  status: string;
  createdAt: string;
  updatedAt: string;
}

export interface UpdatePolicyStatusDto {
  status: string;
}

export interface CreateClaimDto {
  policyId: string;
  description: string;
  claimAmount: number;
}

export interface UploadClaimDocumentDto {
  fileName: string;
  fileType: string;
  fileSizeKb: number;
  contentBase64: string;
}

export interface ClaimDto {
  claimId: string;
  claimNumber: string;
  policyId: string;
  userId: string;
  description: string;
  claimAmount: number;
  status: string;
  adminNote?: string | null;
  reviewedBy?: string | null;
  createdDate: string;
  submittedAt?: string | null;
  reviewedAt?: string | null;
  updatedAt: string;
}

export interface ClaimDocumentDto {
  docId: string;
  claimId: string;
  fileName: string;
  megaNzFileId: string;
  fileUrl: string;
  fileType: string;
  fileSizeKb: number;
  uploadedAt: string;
}

export interface ReviewClaimDto {
  note?: string;
}

export interface ApproveClaimDto {
  approvedAmount?: number;
  note?: string;
}

export interface RejectClaimDto {
  reason: string;
}

export interface DashboardStatsDto {
  totalPolicies: number;
  totalClaims: number;
  totalRevenue: number;
}

export interface AuditLogDto {
  logId: string;
  userId: string;
  action: string;
  entityType: string;
  entityId: string;
  details?: string | null;
  timeStamp: string;
}

export interface PagedResultDto<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface ReportSeriesPoint {
  label: string;
  value: number;
}

export interface PolicyReportDto {
  reportId?: string;
  period?: string;
  totalCount?: number;
  data?: unknown[];
}

export interface ClaimsReportDto {
  reportId?: string;
  period?: string;
  statusBreakdown?: Record<string, number>;
  totalCount?: number;
  data?: unknown[];
}

export interface RevenueReportDto {
  reportId?: string;
  period?: string;
  trend?: string;
  data?: ReportSeriesPoint[];
}
