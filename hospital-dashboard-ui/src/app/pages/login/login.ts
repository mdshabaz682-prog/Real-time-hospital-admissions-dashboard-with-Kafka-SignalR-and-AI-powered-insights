import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../auth';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  template: `
    <div class="card">
      <h1>Hospital Dashboard</h1>
      <p class="sub">Sign in to continue</p>
      <form (ngSubmit)="submit()">
        <label>Username</label>
        <input name="u" [(ngModel)]="username" autocomplete="username" />
        <label>Password</label>
        <input name="p" type="password" [(ngModel)]="password" autocomplete="current-password" />
        <button type="submit" [disabled]="loading()">{{ loading() ? 'Signing in...' : 'Sign in' }}</button>
      </form>
      @if (error()) { <p class="error">{{ error() }}</p> }
    </div>
  `,
  styles: `
    :host { display:flex; justify-content:center; padding-top:80px; font-family:system-ui,sans-serif; }
    .card { width:340px; padding:32px; border:1px solid #ddd; border-radius:12px; box-shadow:0 4px 16px rgba(0,0,0,.06); }
    h1 { margin:0; font-size:22px; }
    .sub { color:#666; margin:4px 0 20px; }
    label { display:block; font-size:13px; margin-top:12px; }
    input { width:100%; padding:10px; margin-top:4px; box-sizing:border-box; border:1px solid #ccc; border-radius:6px; }
    button { width:100%; margin-top:20px; padding:11px; background:#0b5fff; color:#fff; border:0; border-radius:6px; font-size:15px; cursor:pointer; }
    button:disabled { opacity:.6; }
    .error { color:#c0392b; margin-top:14px; }
  `,
})
export class Login {
  username = '';
  password = '';
  loading = signal(false);
  error = signal('');

  constructor(private auth: AuthService, private router: Router) {}

  submit() {
    this.loading.set(true);
    this.error.set('');
    this.auth.login(this.username, this.password).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: () => {
        this.loading.set(false);
        this.error.set('Login failed. Check your username and password.');
      },
    });
  }
}
