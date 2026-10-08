import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';

export const API = 'http://localhost:5181';

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(private http: HttpClient) {}

  login(username: string, password: string) {
    return this.http
      .post<{ token: string; name: string; role: string }>(`${API}/api/auth/login`, { username, password })
      .pipe(
        tap((r) => {
          localStorage.setItem('token', r.token);
          localStorage.setItem('name', r.name);
          localStorage.setItem('role', r.role);
        })
      );
  }

  logout() {
    localStorage.clear();
  }

  get token() { return localStorage.getItem('token'); }
  get name() { return localStorage.getItem('name'); }
  get role() { return localStorage.getItem('role'); }
  get isLoggedIn() { return !!this.token; }
}
