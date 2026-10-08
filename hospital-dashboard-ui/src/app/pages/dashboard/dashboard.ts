import { Component, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { API, AuthService } from '../../auth';

type Row = Record<string, any>;

@Component({
  selector: 'app-dashboard',
  template: `
    <header>
      <h1>Hospital Dashboard</h1>
      <div>
        {{ auth.name }} ({{ auth.role }})
        <button (click)="logout()">Log out</button>
      </div>
    </header>

    @if (error()) { <p class="error">{{ error() }}</p> }

    @for (section of sections; track section.title) {
      <section>
        <h2>{{ section.title }} ({{ section.rows().length }})</h2>
        <div class="scroll">
          <table>
            <thead>
              <tr>@for (c of columns(section.rows()); track c) { <th>{{ c }}</th> }</tr>
            </thead>
            <tbody>
              @for (r of section.rows(); track $index) {
                <tr>@for (c of columns(section.rows()); track c) { <td>{{ show(r[c]) }}</td> }</tr>
              }
            </tbody>
          </table>
        </div>
      </section>
    }
  `,
  styles: `
    :host { display:block; font-family:system-ui,sans-serif; padding:24px 32px; }
    header { display:flex; justify-content:space-between; align-items:center; margin-bottom:24px; }
    h1 { margin:0; font-size:24px; }
    h2 { font-size:18px; margin:28px 0 8px; }
    button { margin-left:12px; padding:6px 12px; cursor:pointer; }
    .scroll { overflow-x:auto; border:1px solid #ddd; border-radius:8px; }
    table { border-collapse:collapse; width:100%; font-size:14px; }
    th, td { text-align:left; padding:8px 12px; border-bottom:1px solid #eee; white-space:nowrap; }
    th { background:#f6f7f9; }
    .error { color:#c0392b; }
  `,
})
export class Dashboard implements OnInit {
  patients = signal<Row[]>([]);
  beds = signal<Row[]>([]);
  admissions = signal<Row[]>([]);
  error = signal('');

  sections = [
    { title: 'Admissions', rows: this.admissions },
    { title: 'Beds', rows: this.beds },
    { title: 'Patients', rows: this.patients },
  ];

  constructor(private http: HttpClient, public auth: AuthService, private router: Router) {}

  ngOnInit() {
    this.load('patients', this.patients);
    this.load('beds', this.beds);
    this.load('admissions', this.admissions);
  }

  private load(path: string, target: { set(v: Row[]): void }) {
    this.http.get<Row[]>(`${API}/api/${path}`).subscribe({
      next: (data) => target.set(data),
      error: (e) => this.error.set(`Could not load ${path} (${e.status}). Try logging in again.`),
    });
  }

  columns(rows: Row[]): string[] {
    return rows.length ? Object.keys(rows[0]) : [];
  }

  show(v: any): string {
    return v !== null && typeof v === 'object' ? JSON.stringify(v) : String(v ?? '');
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
