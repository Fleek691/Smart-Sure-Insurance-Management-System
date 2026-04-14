import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AuthResponseDto,
  ForgotPasswordDto,
  LoginDto,
  OtpDispatchResponseDto,
  ProfileDto,
  RegisterDto,
  ResendRegistrationOtpDto,
  ResetPasswordDto,
  UpdateProfileDto,
  VerifyRegistrationOtpDto
} from '../../models/api-models';
import { ApiService } from './api.service';

/** Wraps authentication and user profile API calls via the gateway. */
@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(private readonly api: ApiService) {}

  login(dto: LoginDto): Observable<AuthResponseDto> {
    return this.api.post<AuthResponseDto>('/auth/login', dto);
  }

  register(dto: RegisterDto): Observable<OtpDispatchResponseDto> {
    return this.api.post<OtpDispatchResponseDto>('/auth/register', dto);
  }

  verifyRegistrationOtp(dto: VerifyRegistrationOtpDto): Observable<AuthResponseDto> {
    return this.api.post<AuthResponseDto>('/auth/register/verify-otp', dto);
  }

  resendRegistrationOtp(dto: ResendRegistrationOtpDto): Observable<OtpDispatchResponseDto> {
    return this.api.post<OtpDispatchResponseDto>('/auth/register/resend-otp', dto);
  }

  requestPasswordReset(dto: ForgotPasswordDto): Observable<void> {
    return this.api.post<void>('/auth/forgot-password', dto);
  }

  resetPassword(dto: ResetPasswordDto): Observable<void> {
    return this.api.post<void>('/auth/reset-password', dto);
  }

  getGoogleLoginUrl(): Observable<string> {
    return this.api.text('/auth/google');
  }

  refreshToken(token: string): Observable<AuthResponseDto> {
    return this.api.post<AuthResponseDto>('/auth/refresh-token', { token });
  }

  getProfile(): Observable<ProfileDto> {
    return this.api.get<ProfileDto>('/users/profile');
  }

  updateProfile(dto: UpdateProfileDto): Observable<ProfileDto> {
    return this.api.put<ProfileDto>('/users/profile', dto);
  }

  getUsers(): Observable<ProfileDto[]> {
    return this.api.get<ProfileDto[]>('/users');
  }

  updateUserStatus(userId: string, isActive: boolean): Observable<void> {
    return this.api.put<void>(`/users/${userId}/status`, null, { isActive });
  }

  updateUserRole(userId: string, role: string): Observable<void> {
    return this.api.put<void>(`/users/${userId}/role`, null, { role });
  }
}
