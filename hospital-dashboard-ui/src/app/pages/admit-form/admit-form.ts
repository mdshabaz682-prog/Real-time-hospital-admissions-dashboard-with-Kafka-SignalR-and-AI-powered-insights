import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

type Row = Record<string, any>;
const API = 'http://localhost:5181';

@Component({
  selector: 'app-admit-form',
  imports: [FormsModule],
  template: `
    <section class="box">
      <h2>Admit patient</h2>
      <div class="row">
        <select [(ngModel)]="patientId">
          <option value="">Select patient...</option>
          @for (p of patients(); track p['id']) {
            <option [value]="p['id']">{{ p['firstName'] }} {{ p['lastName'] }}</option>
          }
        </select>

        <select [(ngModel)]="bedId">
          <option value="">Select available bed...</option>
          @for (b of beds(); track b['id']) {
            <option [value]="b['id']">{{ b['ward'] }} - bed {{ b['bedNumber'] }}</option>
          }
        </select>

        <button (click)="admit()" [disabled]="busy() || !patientId || !bedId">
          {{ busy() ? 'Sending...' : 'Admit' }}
        </button>
      </div>
      @if (message()) { <p class="ok">{{ message() }}</p> }
      @if (error()) { <p class="err">{{ error() }}</p> }
    </section>
  `,
  styles: `
    .box { margin: 1.5rem 0; padding: 1rem; border: 1px solid #ddd; border-radius: 8px; }
    .row { display: flex; gap: .5rem; flex-wrap: wrap; }
    select { padding: .5rem; min-width: 200px; }
    .ok { color: #1b7f3b; }
    .err { color: #b00020; }
  `
})
export class AdmitForm implements OnInit {
  private http = inject(HttpClient);

  patients = signal<Row[]>([]);
  beds = signal<Row[]>([]);
  patientId = '';
  bedId = '';
  busy = signal(false);
  message = signal('');
  error = signal('');

  ngOnInit() {
    this.load();
  }

  load() {
    this.http.get<Row[]>(`${API}/api/patients`).subscribe(p => this.patients.set(p));
    this.http.get<Row[]>(`${API}/api/beds`).subscribe(b =>
      this.beds.set(b.filter(x => x['status'] === 'Available')));
  }

  admit() {
    this.busy.set(true);
    this.message.set('');
    this.error.set('');

    this.http.post(`${API}/api/admissions`, {
      patientId: this.patientId,
      bedId: this.bedId
    }).subscribe({
      next: () => {
        this.message.set('Admission submitted. It will appear on the dashboard in a moment.');
        this.patientId = '';
        this.bedId = '';
        this.busy.set(false);
        setTimeout(() => this.load(), 1500);
      },
      error: err => {
        const body = err?.error;
        this.error.set(typeof body === 'string' ? body : body?.error ?? 'Could not admit this patient.');
        this.busy.set(false);
      }
    });
  }
}