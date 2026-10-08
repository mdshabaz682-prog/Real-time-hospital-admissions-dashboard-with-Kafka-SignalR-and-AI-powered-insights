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

    <div class="stats">
      <div class="stat"><span>{{ availableBeds() }}</span> Beds available</div>
      <div class="stat"><span>{{ occupiedBeds() }}</span> Beds occupied</div>
      <div class="stat"><span>{{ activeAdmissions() }}</span> Active admissions</div>
      <div class="stat"><span>{{ patients().length }}</span> Patients</div>
    </div>

    @if (error()) { <p class="error">{{ error() }}</p> }

    <h2>Admissions</h2>
    <div class="scroll">
      <table>
        <thead><tr><th>Patient</th><th>Ward</th><th>Bed</th><th>Staff</th><th>Admitted</th><th>Discharged</th><th>Status</th></tr></thead>
        <tbody>
          @for (a of admissions(); track a['id']) {
            <tr>
              <td>{{ a['patient']?.firstName }} {{ a['patient']?.lastName }}</td>
              <td>{{ a['bed']?.ward }}</td>
              <td>{{ a['bed']?.bedNumber }}</td>
              <td>{{ a['staff']?.name }}</td>
              <td>{{ date(a['admittedAt']) }}</td>
              <td>{{ date(a['dischargedAt']) }}</td>
              <td><span class="badge" [class]="'badge ' + (a['status'] || '').toLowerCase()">{{ a['status'] }}</span></td>
            </tr>
          }
        </tbody>
      </table>
    </div>

    <h2>Beds</h2>
    <div class="scroll">
      <table>
        <thead><tr><th>Ward</th><th>Bed</th><th>Status</th></tr></thead>
        <tbody>
          @for (b of beds(); track b['id']) {
            <tr>
              <td>{{ b['ward'] }}</td>
              <td>{{ b['bedNumber'] }}</td>
              <td><span [class]="'badge ' + (b['status'] || '').toLowerCase()">{{ b['status'] }}</span></td>
            </tr>
          }
        </tbody>
      </table>
    </div>

    <h2>Patients</h2>
    <div class="scroll">
      <table>
        <thead><tr><th>Name</th><th>Date of birth</th><th>Gender</th><th>Phone</th></tr></thead>
        <tbody>
          @for (p of patients(); track p['id']) {
            <tr>
              <td>{{ p['firstName'] }} {{ p['lastName'] }}</td>
              <td>{{ p['dob'] }}</td>
              <td>{{ p['gender'] }}</td>
              <td>{{ p['contactPhone'] }}</td>
            </tr>
          }
        </tbody>
      </table>
    </div>
  `,
  styles: `
    :host { display:block; font-family:system-ui,sans-serif; padding:24px 32px; }
    header { display:flex; justify-content:space-between; align-items:center; margin-bottom:20px; }
    h1 { margin:0; font-size:24px; }
    h2 { font-size:18px; margin:28px 0 8px; }
    button { margin-left:12px; padding:6px 12px; cursor:pointer; }
    .stats { display:flex; gap:16px; flex-wrap:wrap; }
    .stat { border:1px solid #ddd; border-radius:10px; padding:14px 20px; color:#555; font-size:13px; }
    .stat span { display:block; font-size:28px; font-weight:600; color:#111; }
    .scroll { overflow-x:auto; border:1px solid #ddd; border-radius:8px; }
    table { border-collapse:collapse; width:100%; font-size:14px; }
    th, td { text-align:left; padding:8px 12px; border-bottom:1px solid #eee; white-space:nowrap; }
    th { background:#f6f7f9; }
    .badge { padding:2px 10px; border-radius:12px; font-size:12px; background:#eee; }
    .badge.available { background:#d9f5e3; color:#0a6b32; }
    .badge.occupied, .badge.admitted { background:#fde2e2; color:#a11; }
    .badge.discharged { background:#e3e8f5; color:#234; }
    .error { color:#c0392b; }
  `,
})
export class Dashboard implements OnInit {
  patients = signal<Row[]>([]);
  beds = signal<Row[]>([]);
  admissions = signal<Row[]>([]);
  error = signal('');

  constructor(private http: HttpClient, public auth: AuthService, private router: Router) {}

  ngOnInit() {
    this.load('patients', this.patients);
    this.load('beds', this.beds);
    this.load('admissions', this.admissions);
  }

  availableBeds() { return this.beds().filter((b) => b['status'] === 'Available').length; }
  occupiedBeds() { return this.beds().filter((b) => b['status'] === 'Occupied').length; }
  activeAdmissions() { return this.admissions().filter((a) => !a['dischargedAt']).length; }

  date(v: string | null) {
    return v ? new Date(v + (v.endsWith('Z') ? '' : 'Z')).toLocaleString() : '';
  }

  private load(path: string, target: { set(v: Row[]): void }) {
    this.http.get<Row[]>(`${API}/api/${path}`).subscribe({
      next: (data) => target.set(data),
      error: (e) => this.error.set(`Could not load ${path} (${e.status}). Try logging in again.`),
    });
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}