import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-ask-ai',
  imports: [FormsModule],
  template: `
    <section class="box">
      <h2>Ask the data</h2>
      <div class="row">
        <input [(ngModel)]="question" (keyup.enter)="ask()"
               placeholder="e.g. Which patients are currently admitted?" />
        <button (click)="ask()" [disabled]="loading() || !question.trim()">
          {{ loading() ? 'Thinking...' : 'Ask' }}
        </button>
      </div>

      @if (error()) { <p class="err">{{ error() }}</p> }

      @if (sql()) {
        <p class="sql"><strong>SQL:</strong> <code>{{ sql() }}</code></p>
      }

      @if (columns().length) {
        <table>
          <thead><tr>@for (c of columns(); track c) { <th>{{ c }}</th> }</tr></thead>
          <tbody>
            @for (r of rows(); track $index) {
              <tr>@for (c of columns(); track c) { <td>{{ r[c] }}</td> }</tr>
            }
          </tbody>
        </table>
      } @else if (asked() && !error() && !loading()) {
        <p>No rows returned.</p>
      }
    </section>
  `,
  styles: `
    .box { margin: 1.5rem 0; padding: 1rem; border: 1px solid #ddd; border-radius: 8px; }
    .row { display: flex; gap: .5rem; }
    input { flex: 1; padding: .5rem; }
    .err { color: #b00020; }
    .sql code { background: #f4f4f4; padding: 2px 6px; word-break: break-all; }
    table { border-collapse: collapse; margin-top: .75rem; }
    th, td { border: 1px solid #ddd; padding: .35rem .6rem; text-align: left; }
  `
})
export class AskAi {
  private http = inject(HttpClient);

  question = '';
  loading = signal(false);
  asked = signal(false);
  error = signal('');
  sql = signal('');
  columns = signal<string[]>([]);
  rows = signal<Record<string, unknown>[]>([]);

  ask() {
    this.loading.set(true);
    this.error.set('');
    this.sql.set('');
    this.columns.set([]);
    this.rows.set([]);

    this.http.post<{ sql: string; rows: Record<string, unknown>[] }>(
      'http://localhost:5181/api/ai/query', { question: this.question }
    ).subscribe({
      next: res => {
        this.sql.set(res.sql);
        this.rows.set(res.rows);
        this.columns.set(res.rows.length ? Object.keys(res.rows[0]) : []);
        this.asked.set(true);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? 'Something went wrong. Try rephrasing.');
        this.asked.set(true);
        this.loading.set(false);
      }
    });
  }
}