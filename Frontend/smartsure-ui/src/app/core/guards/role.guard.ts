import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, Router } from '@angular/router';
import { AuthStateService } from '../services/auth-state.service';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const authState = inject(AuthStateService);
  const router = inject(Router);
  const requiredRoles = (route.data['roles'] as string[] | undefined) ?? [];

  if (!authState.isAuthenticated) {
    return router.parseUrl('/auth/login');
  }

  if (requiredRoles.length === 0 || authState.hasAnyRole(requiredRoles)) {
    return true;
  }

  return router.parseUrl(authState.primaryRoute());
};
