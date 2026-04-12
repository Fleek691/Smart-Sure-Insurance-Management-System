import { Component, OnDestroy, OnInit } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter, Subscription } from 'rxjs';
import { AuthStateService } from './core/services/auth-state.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit, OnDestroy {
  compactShell = true;
  private readonly subscriptions = new Subscription();

  constructor(public readonly authState: AuthStateService, private readonly router: Router) {
    this.authState.restoreSession();
  }

  ngOnInit(): void {
    this.updateShellMode(this.router.url);
    this.subscriptions.add(
      this.router.events.pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd)).subscribe((event) => {
        this.updateShellMode(event.urlAfterRedirects);
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  logout(): void {
    this.authState.clearSession();
    void this.router.navigateByUrl('/auth/login');
  }

  private updateShellMode(url: string): void {
    this.compactShell = url.startsWith('/auth');
  }
}
